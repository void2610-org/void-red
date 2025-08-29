using System.Collections.Generic;
using System.Linq;
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
    public List<CardModel> DrawPile => new (_deck);
    
    /// <summary>
    /// 獲得した全カード（崩壊しても記録として残る）
    /// </summary>
    public List<CardModel> AllCards => new (_allCards);
    
    /// <summary>
    /// 現在プレイに使用可能なカード（崩壊したカードは除く）
    /// </summary>
    public List<CardModel> ActiveCards => _allCards.Where(c => !c.IsCollapsed).ToList();
    
    /// <summary>
    /// 崩壊したカード
    /// </summary>
    public List<CardModel> CollapsedCards => _allCards.Where(c => c.IsCollapsed).ToList();
    
    private readonly List<CardModel> _deck = new();           // 山札
    private readonly List<CardModel> _allCards = new();       // 獲得した全カード
    
    /// <summary>
    /// コンストラクタ（新規デッキ作成用）
    /// </summary>
    /// <param name="initialCards">初期カードデータリスト</param>
    public DeckModel(List<CardData> initialCards)
    {
        if (initialCards is { Count: > 0 })
        {
            foreach (var cardData in initialCards)
            {
                var cardModel = new CardModel(cardData);
                _allCards.Add(cardModel);
                _deck.Add(cardModel);
            }
            Shuffle();
        }
    }
    
    /// <summary>
    /// コンストラクタ（復元用）
    /// </summary>
    /// <param name="cardModels">復元するカードモデルリスト</param>
    public DeckModel(List<CardModel> cardModels)
    {
        if (cardModels is { Count: > 0 })
        {
            _allCards.AddRange(cardModels);
            // 崩壊していないカードのみ山札に追加
            _deck.AddRange(cardModels.Where(c => !c.IsCollapsed));
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
            foreach (var cardData in cardDataList)
            {
                var cardModel = new CardModel(cardData);
                _allCards.Add(cardModel);
                _deck.Add(cardModel);
            }
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
    public CardModel DrawCard()
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
    public List<CardModel> DrawCards(int count)
    {
        var drawnCards = new List<CardModel>();
        
        for (int i = 0; i < count && _deck.Count > 0; i++)
        {
            drawnCards.Add(DrawCard());
        }
        
        return drawnCards;
    }
    
    /// <summary>
    /// カードをデッキに戻す
    /// </summary>
    /// <param name="cardModel">戻すカードモデル</param>
    public void ReturnCard(CardModel cardModel)
    {
        if (cardModel == null) return;
        
        // 崩壊していないカードのみ山札に戻す
        if (!cardModel.IsCollapsed)
        {
            _deck.Add(cardModel);
            Shuffle();
        }
    }
    
    /// <summary>
    /// 複数のカードをデッキに戻す
    /// </summary>
    /// <param name="cardModels">戻すカードモデルのリスト</param>
    public void ReturnCards(List<CardModel> cardModels)
    {
        if (cardModels == null || cardModels.Count == 0) return;
        
        // 崩壊していないカードのみ山札に戻す
        var nonCollapsedCards = cardModels.Where(c => !c.IsCollapsed).ToList();
        if (nonCollapsedCards.Count > 0)
        {
            _deck.AddRange(nonCollapsedCards);
            Shuffle();
        }
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
    public List<CardModel> GetDeckContents()
    {
        return new List<CardModel>(_deck);
    }
    
    /// <summary>
    /// 指定したカードを別のカードで置き換える（進化・劣化時の処理）
    /// </summary>
    /// <param name="oldCard">置き換える元のカード</param>
    /// <param name="newCardData">新しいカードデータ</param>
    /// <returns>置き換えに成功したかどうか</returns>
    public bool ReplaceCard(CardModel oldCard, CardData newCardData)
    {
        if (oldCard == null || newCardData == null) return false;
        
        // AllCardsで置き換え
        var allCardsIndex = _allCards.FindIndex(card => card.InstanceId == oldCard.InstanceId);
        if (allCardsIndex >= 0)
        {
            var newCard = new CardModel(newCardData);
            _allCards[allCardsIndex] = newCard;
            
            // DrawPileでも置き換え（山札）
            var deckIndex = _deck.FindIndex(card => card.InstanceId == oldCard.InstanceId);
            if (deckIndex >= 0)
            {
                _deck[deckIndex] = newCard;
            }
            
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// カードを崩壊させる
    /// </summary>
    /// <param name="cardModel">崩壊するカード</param>
    public void CollapseCard(CardModel cardModel)
    {
        if (cardModel == null) return;
        
        // カードの崩壊フラグを設定
        cardModel.IsCollapsed = true;
        
        // 山札から削除
        _deck.Remove(cardModel);
    }
    
    /// <summary>
    /// 使用可能なカードが全て失われたかどうか（真のゲームオーバー条件）
    /// </summary>
    public bool IsActiveCardsEmpty => ActiveCards.Count == 0;
}