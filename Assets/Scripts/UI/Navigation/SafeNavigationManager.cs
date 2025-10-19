using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer.Unity;

public class SafeNavigationManager : ITickable
{
    private GameObject _previousSelected;
    private static bool _allowProgrammaticChange = false;
    private static EventSystem _eventSystem;
    
    public static GameObject GetCurrentSelected() => _eventSystem?.currentSelectedGameObject;
    
    public SafeNavigationManager()
    {
        _eventSystem = EventSystem.current;
    }
    
    public static void SetSelectedGameObjectSafe(GameObject go)
    {
        _allowProgrammaticChange = true;
        if (!_eventSystem) _eventSystem = EventSystem.current;
        _eventSystem.SetSelectedGameObject(go);
    }

    public static async UniTask SelectRootForceSelectable()
    {
        while (true)
        {
            _eventSystem = EventSystem.current;
            var canvas = Object.FindAnyObjectByType<Canvas>();
            var selectable = canvas?.transform.GetComponentsInChildren<ForceSelectable>().FirstOrDefault();
            if (!_eventSystem || !canvas || canvas.name == "SceneTransitionCanvas" || !selectable)
            {
                await UniTask.Yield();
                continue;
            }

            SetSelectedGameObjectSafe(selectable.gameObject);
            // Tick()で_allowProgrammaticChangeがfalseにリセットされるまで待機
            while (_allowProgrammaticChange)
            {
                await UniTask.Yield();
            }
            break;
        }
    }
    
    /// <summary>
    /// ナビゲーション移動がCanvasGroupを跨いでいるかどうかをチェック
    /// </summary>
    private bool IsSameCanvasGroup(GameObject currentSelected, GameObject previousSelected)
    {
        var currentGroup = currentSelected.GetComponentInParent<CanvasGroup>();
        var previousGroup = previousSelected.GetComponentInParent<CanvasGroup>();
        
        if (currentGroup == null && previousGroup == null)
        {
            return true; // 両方ともCanvasGroupがない場合は同じとみなす
        }
        
        return currentGroup == previousGroup;
    }
    
    public void Tick()
    {
        if (!_eventSystem)
        {
            _eventSystem = EventSystem.current;
            _allowProgrammaticChange = false;
            return;
        }

        var currentSelected = _eventSystem.currentSelectedGameObject;

        if (!currentSelected)
        {
            _previousSelected = null;
            SelectRootForceSelectable().Forget();
            _allowProgrammaticChange = false;
            return;
        }

        if (!_previousSelected)
        {
            _previousSelected = currentSelected;
            _allowProgrammaticChange = false;
            return;
        }

        if (currentSelected != _previousSelected)
        {
            if (!_allowProgrammaticChange)
            {
                // グループが異なる場合は選択をキャンセル
                if (!IsSameCanvasGroup(currentSelected, _previousSelected))
                {
                    _eventSystem.SetSelectedGameObject(_previousSelected);
                    _allowProgrammaticChange = false;
                    return;
                }
            }
            _previousSelected = currentSelected;
        }

        _allowProgrammaticChange = false;
    }
}