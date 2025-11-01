using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using R3;
using LitMotion;
using Void2610.UnityTemplate;

/// <summary>
/// カード風選択肢表示を担当するViewクラス
/// 2つの選択肢ボタンとカード画像を表示し、ユーザーの選択を受け取る
/// </summary>
public class CardChoiceView : BaseWindowView
{
    [Header("UI要素")]
    [SerializeField] private Button choice1Button;
    [SerializeField] private TextMeshProUGUI choice1Text;
    [SerializeField] private Image choice1CardImage;
    [SerializeField] private Button choice2Button;
    [SerializeField] private TextMeshProUGUI choice2Text;
    [SerializeField] private Image choice2CardImage;

    private readonly Subject<int> _choiceSelectedSubject = new();
    
    /// <summary>
    /// カード風選択肢を表示して選択を待つ
    /// </summary>
    public async UniTask<int> ShowCardChoice(CardChoiceData cardChoiceData, Sprite cardImage1, Sprite cardImage2)
    {
        SetupUIElements(cardChoiceData, cardImage1, cardImage2);

        Show(); // Show()内でGetPreferredNavigationTarget()が呼ばれ、最初の選択肢が選択される
        await UniTask.Delay(500);

        var selectedIndex = await _choiceSelectedSubject.FirstAsync();
        Hide();

        return selectedIndex;
    }

    /// <summary>
    /// 優先ナビゲーション対象を返す（最初の選択肢ボタン）
    /// </summary>
    protected override GameObject GetPreferredNavigationTarget()
    {
        return choice1Button ? choice1Button.gameObject : null;
    }
    
    /// <summary>
    /// UI要素を設定
    /// </summary>
    private void SetupUIElements(CardChoiceData cardChoiceData, Sprite cardImage1, Sprite cardImage2)
    {
        choice1Text.text = cardChoiceData.Option1;
        choice2Text.text = cardChoiceData.Option2;

        choice1CardImage.sprite = cardImage1;
        choice2CardImage.sprite = cardImage2;
    }

    /// <summary>
    /// 選択肢ボタンがクリックされた時の処理
    /// </summary>
    private void OnChoiceButtonClicked(int choiceIndex)
    {
        if (!IsShowing)
            return;

        _choiceSelectedSubject.OnNext(choiceIndex);
    }

    protected override void Awake()
    {
        base.Awake();

        choice1Text.text = "";
        choice2Text.text = "";

        // ボタンクリックイベントを購読
        choice1Button.OnClickAsObservable()
            .Subscribe(_ => OnChoiceButtonClicked(0))
            .AddTo(Disposables);

        choice2Button.OnClickAsObservable()
            .Subscribe(_ => OnChoiceButtonClicked(1))
            .AddTo(Disposables);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _choiceSelectedSubject?.Dispose();
    }
}
