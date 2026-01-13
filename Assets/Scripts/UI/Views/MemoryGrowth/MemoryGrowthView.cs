using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using R3;

/// <summary>
/// 記憶育成フェーズのメインビュー
/// BaseWindowViewを継承してウィンドウとして振る舞う
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class MemoryGrowthView : BaseWindowView
{
    [SerializeField] private MemoryThemeListView themeListView;
    [SerializeField] private CharacterMemoryView characterView;

    private readonly Subject<Unit> _onContinueClicked = new();

    /// <summary>
    /// 記憶育成フェーズを表示
    /// </summary>
    /// <param name="allThemes">全獲得テーマリスト</param>
    public void ShowMemoryGrowth(IReadOnlyList<AcquiredTheme> allThemes)
    {
        // 左側のテーマリストを初期化
        themeListView.Initialize(allThemes);

        // テーマ選択時のハンドラを設定
        themeListView.OnThemeSelected
            .Subscribe(theme => characterView?.ShowThemeCards(theme))
            .AddTo(Disposables);

        // ウィンドウを表示
        Show();
    }

    /// <summary>
    /// 続けるボタンクリックを待機
    /// </summary>
    public async UniTask WaitForContinueAsync()
    {
        await _onContinueClicked.FirstAsync();
    }

    protected override void Awake()
    {
        base.Awake();

        closeButton.OnClickAsObservable()
            .Subscribe(_ => _onContinueClicked.OnNext(Unit.Default))
            .AddTo(Disposables);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _onContinueClicked?.Dispose();
    }
}
