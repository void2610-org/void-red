using LitMotion;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// ボタン選択時の背景グロー効果を管理するコンポーネント
/// ISelectHandler/IDeselectHandlerを使用してSelectable選択状態に基づいてグロー画像を制御
/// </summary>
[RequireComponent(typeof(Selectable))]
public class HomeButtonAnimation : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image target;
    [SerializeField] private float selectedScale;
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = Color.white;

    private const float DURATION = 0.1f;
    private const Ease EASE = Ease.OutCubic;

    private MotionHandle _scaleMotion;
    private MotionHandle _colorMotion;
    private float _defaultScale;

    private void Awake()
    {
        _defaultScale = target.transform.localScale.x;

        if (SafeNavigationManager.GetCurrentSelected() != this.gameObject)
            OnDeselect(new BaseEventData(EventSystem.current));
    }

    /// <summary>
    /// Selectableが選択された時の処理
    /// </summary>
    public void OnSelect(BaseEventData eventData)
    {
        _scaleMotion.TryCancel();
        _colorMotion.TryCancel();

        _scaleMotion = target.rectTransform.ScaleTo(Vector3.one * _defaultScale * selectedScale, DURATION, EASE, ignoreTimeScale: true);
        _colorMotion = target.ColorTo(selectedColor, DURATION, EASE, ignoreTimeScale: true);
    }

    /// <summary>
    /// Selectableの選択が解除された時の処理
    /// </summary>
    public void OnDeselect(BaseEventData eventData)
    {
        _scaleMotion.TryCancel();
        _colorMotion.TryCancel();

        _scaleMotion = target.rectTransform.ScaleTo(Vector3.one * _defaultScale, DURATION, EASE, ignoreTimeScale: true);
        _colorMotion = target.ColorTo(unselectedColor, DURATION, EASE, ignoreTimeScale: true);
    }

    private void OnDestroy()
    {
        _scaleMotion.TryCancel();
        _colorMotion.TryCancel();
    }
}
