using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// キャラクター記憶ビュー
/// 右側パネルでキャラクターとカードを表示
/// </summary>
public class MemoryDetailView : BaseWindowView
{
    [Header("カード表示")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardView cardPrefab;

    private readonly List<CardView> _cardViews = new();

    /// <summary>
    /// 既存テーマのカードを表示（リストクリック時）
    /// </summary>
    public async UniTask ShowAndWaitClose(AcquiredTheme theme)
    {
        foreach (var cardView in _cardViews.Where(cardView => cardView)) Destroy(cardView.gameObject);
        _cardViews.Clear();
        
        ShowCards(theme.AcquiredCards);
        base.Show();

        await closeButton.OnClickAsync();
        base.Hide();
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
}
