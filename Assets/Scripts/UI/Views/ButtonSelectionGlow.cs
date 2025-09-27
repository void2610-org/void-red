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
public class ButtonSelectionGlow : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image glowImage;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.15f;
    [SerializeField] private Ease fadeInEase = Ease.OutCubic;
    [SerializeField] private Ease fadeOutEase = Ease.OutCubic;
    
    private MotionHandle _currentMotion;
    
    private void Awake()
    {
        // 初期状態では非表示
        glowImage.SetAlpha(0f);
    }
    
    /// <summary>
    /// Selectableが選択された時の処理
    /// </summary>
    public void OnSelect(BaseEventData eventData)
    {
        if (_currentMotion.IsActive()) _currentMotion.Cancel();
        _currentMotion = glowImage.FadeIn(fadeInDuration, fadeInEase);
    }
    
    /// <summary>
    /// Selectableの選択が解除された時の処理
    /// </summary>
    public void OnDeselect(BaseEventData eventData)
    {
        if (_currentMotion.IsActive()) _currentMotion.Cancel();
        _currentMotion = glowImage.FadeOut(fadeOutDuration, fadeOutEase);
    }
    
    private void OnDestroy()
    {
        if (_currentMotion.IsActive())
            _currentMotion.Cancel();
    }
}