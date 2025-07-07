using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// デッキのデータと操作を担当するModelクラス
/// カードの管理、シャッフル、ドロー機能を提供
/// </summary>
public class DeckModel
{
    public int Count => _deck.Count;
    public bool IsEmpty => _deck.Count == 0;
    
    /// <summary>
    /// 山札（ドロー可能なカード）
    /// </summary>
    public List<CardData> DrawPile => new (_deck);
    
    /// <summary>
    /// デッキ全体（手札含む概念的な全カード）
    /// </summary>
    public List<CardData> AllCards => new (_allCards);
    
    private readonly List<CardData> _deck = new();           // 山札
    private readonly List<CardData> _allCards = new();       // デッキ全体
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="initialCards">初期カードリスト</param>
    public DeckModel(List<CardData> initialCards)
    {
        if (initialCards is { Count: > 0 })
        {
            _allCards.AddRange(initialCards);  // デッキ全体に追加
            _deck.AddRange(initialCards);      // 山札に追加
            Shuffle();
        }
    }
    
    /// <summary>
    /// デッキを初期化（既存のカードをクリアして新しいカードで置き換え）
    /// </summary>
    /// <param name="cardDataList">デッキに追加するカードリスト</param>
    public void InitializeDeck(List<CardData> cardDataList)
    {
        _deck.Clear();
        _allCards.Clear();
        if (cardDataList is { Count: > 0 })
        {
            _allCards.AddRange(cardDataList);  // デッキ全体に追加
            _deck.AddRange(cardDataList);      // 山札に追加
            Shuffle();
        }
    }
    
    /// <summary>
    /// デッキをクリア
    /// </summary>
    public void Clear()
    {
        _deck.Clear();
        _allCards.Clear();
    }
    
    /// <summary>
    /// カードを1枚ドロー
    /// </summary>
    /// <returns>ドローしたカード（デッキが空の場合はnull）</returns>
    public CardData DrawCard()
    {
        if (_deck.Count == 0) return null;
        
        var card = _deck[0];
        _deck.RemoveAt(0);
        return card;
    }
    
    /// <summary>
    /// 指定した枚数のカードをドロー
    /// </summary>
    /// <param name="count">ドローする枚数</param>
    /// <returns>ドローしたカードのリスト</returns>
    public List<CardData> DrawCards(int count)
    {
        var drawnCards = new List<CardData>();
        
        for (int i = 0; i < count && _deck.Count > 0; i++)
        {
            drawnCards.Add(DrawCard());
        }
        
        return drawnCards;
    }
    
    /// <summary>
    /// カードをデッキに戻す
    /// </summary>
    /// <param name="cardData">戻すカードデータ</param>
    public void ReturnCard(CardData cardData)
    {
        if (!cardData) return;
        
        _deck.Add(cardData);
        Shuffle();
    }
    
    /// <summary>
    /// 複数のカードをデッキに戻す
    /// </summary>
    /// <param name="cardDataList">戻すカードデータのリスト</param>
    public void ReturnCards(List<CardData> cardDataList)
    {
        if (cardDataList == null || cardDataList.Count == 0) return;
        
        _deck.AddRange(cardDataList);
        Shuffle();
    }
    
    /// <summary>
    /// デッキをシャッフル
    /// </summary>
    public void Shuffle()
    {
        for (var i = 0; i < _deck.Count; i++)
        {
            var temp = _deck[i];
            var randomIndex = Random.Range(i, _deck.Count);
            _deck[i] = _deck[randomIndex];
            _deck[randomIndex] = temp;
        }
    }
    
    /// <summary>
    /// 山札の内容を取得（読み取り専用）
    /// </summary>
    /// <returns>山札のカードリスト（コピー）</returns>
    public List<CardData> GetDeckContents()
    {
        return new List<CardData>(_deck);
    }
    
    /// <summary>
    /// 指定したインデックスのカードを別のカードで置き換える（デッキ全体基準）
    /// </summary>
    /// <param name="index">置き換えるカードのインデックス（AllCards基準）</param>
    /// <param name="newCard">新しいカード</param>
    public void ReplaceCard(int index, CardData newCard)
    {
        if (index < 0 || index >= _allCards.Count || !newCard) return;
        
        var oldCard = _allCards[index];
        _allCards[index] = newCard;
        
        // 山札内にも同じカードがあれば置き換える
        var deckIndex = _deck.FindIndex(card => card == oldCard);
        if (deckIndex >= 0) _deck[deckIndex] = newCard;
    }
}