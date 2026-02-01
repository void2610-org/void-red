using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// 記憶育成フェーズのメインビュー
/// BaseWindowViewを継承してウィンドウとして振る舞う
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class MemoryGrowthView : MonoBehaviour
{
    [SerializeField] private MemoryThemeListView themeListView;
    [SerializeField] private MemoryDetailView detailView;
    [SerializeField] private GameObject hideObject;
    [SerializeField] private Button nextButton;

    private CanvasGroup _canvasGroup;

    public async UniTask WaitForContinueAsync() => await nextButton.OnClickAsync();

    /// <summary>
    /// 記憶育成フェーズを表示
    /// </summary>
    /// <param name="allThemes">全獲得テーマリスト</param>
    public void ShowMemoryGrowth(IReadOnlyList<AcquiredTheme> allThemes)
    {
        // テーマリストを初期化
        themeListView.Initialize(allThemes);
        // テーマ選択時のハンドラを設定
        themeListView.OnThemeSelected
            .Subscribe(theme => HandleThemeSelected(theme).Forget())
            .AddTo(this);
        
        // ウィンドウを表示
        _canvasGroup.FadeIn(0.5f);
    }
    
    private async UniTask HandleThemeSelected(AcquiredTheme theme)
    {
        hideObject.SetActive(false);
        await detailView.ShowAndWaitClose(theme);
        hideObject.SetActive(true);
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
}
