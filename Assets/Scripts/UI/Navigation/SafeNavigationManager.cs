using UnityEngine;
using UnityEngine.EventSystems;
using VContainer.Unity;

public class SafeNavigationManager : ITickable
{
    private GameObject _previousSelected;
    private static bool _allowProgrammaticChange = false;
    private static EventSystem _eventSystem;
    
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
    
    /// <summary>
    /// ナビゲーション移動がCanvasGroupを跨いでいるかどうかをチェック
    /// </summary>
    private bool IsSameCanvasGroup(GameObject currentSelected, GameObject previousSelected)
    {
        var currentGroup = currentSelected.GetComponentInParent<CanvasGroup>();
        var previousGroup = previousSelected.GetComponentInParent<CanvasGroup>();
        var result = (currentGroup && previousGroup && currentGroup == previousGroup);

        return result;
    }
    
    public void Tick()
    {
        if (!_eventSystem) return;
        
        var currentSelected = _eventSystem.currentSelectedGameObject;

        if (!currentSelected)
        {
            _previousSelected = null;
            // UIManager.Instance.ResetSelectedGameObject();
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
                    return;
                }
            }
            _previousSelected = currentSelected;
        }

        _allowProgrammaticChange = false;
    }
}