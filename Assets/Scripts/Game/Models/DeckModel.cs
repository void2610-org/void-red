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
    /// 獲得した全カード（崩壊しても記録として残る）
    /// </summary>
    public List<CardData> AllCards => new (_allCards);
    
    /// <summary>
    /// 現在プレイに使用可能なカード（崩壊したカードは除く）
    /// </summary>
    public List<CardData> ActiveCards => new (_activeCards);
    
    /// <summary>
    /// 崩壊したカード（将来のメニュー表示用）
    /// </summary>
    public List<CardData> CollapsedCards => new (_collapsedCards);
    
    private readonly List<CardData> _deck = new();           // 山札
    private readonly List<CardData> _allCards = new();       // 獲得した全カード
    private readonly List<CardData> _activeCards = new();    // 使用可能なカード
    private readonly List<CardData> _collapsedCards = new(); // 崩壊したカード
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="initialCards">初期カードリスト</param>
    public DeckModel(List<CardData> initialCards)
    {
        if (initialCards is { Count: > 0 })
        {
            _allCards.AddRange(initialCards);      // 獲得した全カードに追加
            _activeCards.AddRange(initialCards);   // 使用可能なカードに追加
            _deck.AddRange(initialCards);          // 山札に追加
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
        _activeCards.Clear();
        _collapsedCards.Clear();
        
        if (cardDataList is { Count: > 0 })
        {
            _allCards.AddRange(cardDataList);      // 獲得した全カードに追加
            _activeCards.AddRange(cardDataList);   // 使用可能なカードに追加
            _deck.AddRange(cardDataList);          // 山札に追加
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
        _activeCards.Clear();
        _collapsedCards.Clear();
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
    /// 指定したカードを別のカードで置き換える（進化・劣化時の処理）
    /// </summary>
    /// <param name="oldCard">置き換える元のカード</param>
    /// <param name="newCard">新しいカード</param>
    /// <returns>置き換えに成功したかどうか</returns>
    public bool ReplaceCard(CardData oldCard, CardData newCard)
    {
        if (!oldCard || !newCard) return false;
        
        // AllCardsで置き換え（獲得記録）
        var allCardsIndex = _allCards.FindIndex(card => card == oldCard);
        if (allCardsIndex >= 0)
        {
            _allCards[allCardsIndex] = newCard;
        }
        
        // ActiveCardsで置き換え（使用可能カード）
        var activeIndex = _activeCards.FindIndex(card => card == oldCard);
        if (activeIndex >= 0)
        {
            _activeCards[activeIndex] = newCard;
        }
        
        // DrawPileで置き換え（山札）
        var deckIndex = _deck.FindIndex(card => card == oldCard);
        if (deckIndex >= 0)
        {
            _deck[deckIndex] = newCard;
        }
        
        return allCardsIndex >= 0 || activeIndex >= 0 || deckIndex >= 0;
    }
    
    /// <summary>
    /// カードを崩壊させる（ActiveCardsから削除してCollapsedCardsに移動）
    /// </summary>
    /// <param name="cardData">崩壊するカード</param>
    /// <returns>崩壊に成功したかどうか</returns>
    public void CollapseCard(CardData cardData)
    {
        // ActiveCardsとDrawPileから削除
        var removedFromActive = _activeCards.Remove(cardData);
        var removedFromDeck = _deck.Remove(cardData);
        
        // 成功した場合はCollapsedCardsに追加
        if (removedFromActive)
        {
            _collapsedCards.Add(cardData);
        }
    }
    
    /// <summary>
    /// 使用可能なカードが全て失われたかどうか（真のゲームオーバー条件）
    /// </summary>
    public bool IsActiveCardsEmpty => _activeCards.Count == 0;
}