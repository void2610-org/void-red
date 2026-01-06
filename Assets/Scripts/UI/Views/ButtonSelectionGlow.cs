using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using LitMotion;
using Void2610.UnityTemplate;

/// <summary>
/// ボタン選択時の背景グロー効果を管理するコンポーネント
/// ISelectHandler/IDeselectHandlerを使用してSelectable選択状態に基づいてグロー画像を制御
/// </summary>
[RequireComponent(typeof(Selectable))]
public class ButtonSelectionGlow : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image glowImage;
    
    private MotionHandle _currentMotion;
    
    private const float FADE_DURATION = 0.3f;
    private const Ease FADE_EASE = Ease.OutCubic;
    
    private void Awake()
    {
        if (SafeNavigationManager.GetCurrentSelected() != this.gameObject)
            OnDeselect(new BaseEventData(EventSystem.current));
    }
    
    /// <summary>
    /// Selectableが選択された時の処理
    /// </summary>
    public void OnSelect(BaseEventData eventData)
    {
        _currentMotion.TryCancel();
        _currentMotion = glowImage.FadeIn(FADE_DURATION, FADE_EASE, ignoreTimeScale: true);
    }
    
    /// <summary>
    /// Selectableの選択が解除された時の処理
    /// </summary>
    public void OnDeselect(BaseEventData eventData)
    {
        _currentMotion.TryCancel();
        _currentMotion = glowImage.FadeOut(FADE_DURATION, FADE_EASE, ignoreTimeScale: true);
    }
    
    private void OnDestroy()
    {
        _currentMotion.TryCancel();
    }
}