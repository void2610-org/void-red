using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;
using Void2610.UnityTemplate;

/// <summary>
/// ボタン選択時の背景グロー効果を管理するコンポーネント
/// ISelectHandler/IDeselectHandlerを使用してSelectable選択状態に基づいてグロー画像を制御
/// </summary>
[RequireComponent(typeof(Selectable))]
public class NormalButtonAnimation : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image target;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unselectedColor;
    [SerializeField] private float selectedScale;
    
    private MotionHandle _colorMotion;
    private MotionHandle _scaleMotion;
    private float _defaultScale;
    
    private const float DURATION = 0.3f;
    private const Ease EASE = Ease.OutCubic;

    private void Awake()
    {
        _defaultScale = target.transform.localScale.x;
        OnDeselect(new BaseEventData(EventSystem.current));
    }

    /// <summary>
    /// Selectableが選択された時の処理
    /// </summary>
    public void OnSelect(BaseEventData eventData)
    {
        _colorMotion.TryCancel();
        _scaleMotion.TryCancel();
        
        _colorMotion = target.ColorTo(selectedColor, DURATION, EASE);
        _scaleMotion = target.rectTransform.ScaleTo(Vector3.one * _defaultScale * selectedScale, DURATION, EASE);
    }
    
    /// <summary>
    /// Selectableの選択が解除された時の処理
    /// </summary>
    public void OnDeselect(BaseEventData eventData)
    {
        _colorMotion.TryCancel();
        _scaleMotion.TryCancel();
        
        _colorMotion = target.ColorTo(unselectedColor, DURATION, EASE);
        _scaleMotion  = target.rectTransform.ScaleTo(Vector3.one * _defaultScale, DURATION, EASE);
    }
    
    private void OnDestroy()
    {
        _colorMotion.TryCancel();
    }
}