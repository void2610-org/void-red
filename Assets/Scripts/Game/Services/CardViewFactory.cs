using UnityEngine;

/// <summary>
/// CardViewの生成を担当するFactoryクラス
/// </summary>
public class CardViewFactory
{
    private readonly CardView _cardPrefab;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="cardPrefab">カードプレハブ</param>
    public CardViewFactory(CardView cardPrefab)
    {
        _cardPrefab = cardPrefab;
    }

    /// <summary>
    /// カードを生成
    /// </summary>
    /// <param name="cardData">カードデータ</param>
    /// <param name="parent">親Transform</param>
    /// <param name="isInteractable">インタラクト可能かどうか</param>
    /// <returns>生成されたCardView</returns>
    public CardView CreateCard(CardData cardData, Transform parent, bool isInteractable = true)
    {
        var cardView = Object.Instantiate(_cardPrefab, parent);
        cardView.Initialize(cardData);
        cardView.SetInteractable(isInteractable);
        return cardView;
    }
}
