using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;

/// <summary>
/// ヘルプ画面を表示するViewクラス
/// </summary>
public class HelpView : BaseWindowView
{
    [Header("UIコンポーネント")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image helpImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI pageText;

    private readonly Subject<Unit> _onPreviousClicked = new();
    private readonly Subject<Unit> _onNextClicked = new();

    // イベント
    public Observable<Unit> OnPreviousClicked => _onPreviousClicked;
    public Observable<Unit> OnNextClicked => _onNextClicked;

    protected override void Awake()
    {
        base.Awake();

        // ボタンイベントの購読
        previousButton.OnClickAsObservable()
            .Subscribe(_ => _onPreviousClicked.OnNext(Unit.Default))
            .AddTo(Disposables);

        nextButton.OnClickAsObservable()
            .Subscribe(_ => _onNextClicked.OnNext(Unit.Default))
            .AddTo(Disposables);
    }

    /// <summary>
    /// ヘルプデータを表示
    /// </summary>
    /// <param name="helpData">表示するヘルプデータ</param>
    /// <param name="currentIndex">現在のページ番号（0始まり）</param>
    /// <param name="totalCount">全ページ数</param>
    public void DisplayHelp(HelpData helpData, int currentIndex, int totalCount)
    {
        if (!helpData) return;

        titleText.text = helpData.Title;
        helpImage.sprite = helpData.Image;
        descriptionText.text = helpData.Description;

        // ページ番号表示（1始まりで表示）
        pageText.text = $"{currentIndex + 1} / {totalCount}";

        var currentSelected = SafeNavigationManager.GetCurrentSelected();
        
        // 前へ/次へボタンの有効/無効
        previousButton.interactable = currentIndex > 0;
        nextButton.interactable = currentIndex < totalCount - 1;

        // 選択が外れた場合に再選択
        SafeNavigationManager.SetSelectedGameObjectSafe(currentSelected);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _onPreviousClicked?.Dispose();
        _onNextClicked?.Dispose();
    }
}
