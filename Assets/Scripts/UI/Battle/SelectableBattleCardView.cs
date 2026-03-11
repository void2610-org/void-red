using R3;
using UnityEngine;

/// <summary>
/// 対象選択用のクリック専用バトルカードView
/// </summary>
public class SelectableBattleCardView : MonoBehaviour
{
    [SerializeField] private CardView cardView;
    [SerializeField] private BattleCardNumberLabel numberView;

    public Observable<CardModel> OnClicked => _onClicked;

    private readonly Subject<CardModel> _onClicked = new();
    private CardModel _cardModel;

    /// <summary>カード表示とバトル数字を初期化する</summary>
    /// <param name="cardModel">表示対象のカードモデル</param>
    public void Initialize(CardModel cardModel)
    {
        _cardModel = cardModel;
        cardView.Initialize(cardModel.Data);
        numberView.SetNumber(cardModel.BattleNumber);
    }

    private void Awake()
    {
        cardView.OnClicked
            .Subscribe(_ => _onClicked.OnNext(_cardModel))
            .AddTo(this);
    }

    private void OnDestroy() => _onClicked.Dispose();
}
