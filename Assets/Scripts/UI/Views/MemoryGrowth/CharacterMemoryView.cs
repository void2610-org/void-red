using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// キャラクター記憶ビュー
/// 右側パネルでキャラクターとカードを表示
/// </summary>
public class CharacterMemoryView : MonoBehaviour
{
    [Header("カード表示")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardView cardPrefab;

    private readonly List<CardView> _cardViews = new();

    /// <summary>
    /// 既存テーマのカードを表示（リストクリック時）
    /// </summary>
    public void ShowThemeCards(AcquiredTheme theme)
    {
        ClearCards();
        ShowCards(theme.AcquiredCards);
    }

    /// <summary>
    /// カードを表示
    /// </summary>
    private void ShowCards(IReadOnlyList<CardModel> cards)
    {
        foreach (var card in cards)
        {
            var cardView = Instantiate(cardPrefab, cardContainer);
            cardView.Initialize(card.Data);
            _cardViews.Add(cardView);
        }
    }

    /// <summary>
    /// カードをクリア
    /// </summary>
    private void ClearCards()
    {
        foreach (var cardView in _cardViews)
            if (cardView) Destroy(cardView.gameObject);
        _cardViews.Clear();
    }

    private void OnDestroy()
    {
        ClearCards();
    }
}
