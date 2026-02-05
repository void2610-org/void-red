using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// カードプールを管理するサービスクラス
/// VContainerによりシングルトンとして管理される
/// </summary>
public class CardPoolService
{
    private readonly AllCardData _allCardData;
    private readonly List<CardData> _availableCards;

    /// <summary>
    /// コンストラクタ（AllCardDataListをDIで受け取る）
    /// </summary>
    /// <param name="allCardData">全カードデータリスト</param>
    public CardPoolService(AllCardData allCardData)
    {
        _allCardData = allCardData;
        _availableCards = _allCardData.CardList.ToList();
    }

    /// <summary>
    /// ランダムなカードを1枚取得
    /// </summary>
    /// <returns>ランダムなカード</returns>
    public CardData GetRandomCard()
    {
        if (_availableCards.Count == 0)
        {
            return null;
        }

        var randomIndex = Random.Range(0, _availableCards.Count);
        return _availableCards[randomIndex];
    }

    /// <summary>
    /// 複数のランダムなカードを取得（重複なし）
    /// </summary>
    /// <param name="count">取得するカード数</param>
    /// <returns>ランダムなカードのリスト（重複なし）</returns>
    public List<CardData> GetRandomCards(int count)
    {
        if (count <= 0) return new List<CardData>();
        if (_availableCards.Count == 0) return new List<CardData>();

        // 要求数が利用可能カード数を超える場合は、利用可能な分だけ返す
        var actualCount = Mathf.Min(count, _availableCards.Count);

        // シャッフルしてから先頭から取得（重複なし）
        var shuffledCards = new List<CardData>(_availableCards);
        for (int i = 0; i < shuffledCards.Count; i++)
        {
            var temp = shuffledCards[i];
            var randomIndex = Random.Range(i, shuffledCards.Count);
            shuffledCards[i] = shuffledCards[randomIndex];
            shuffledCards[randomIndex] = temp;
        }

        return shuffledCards.Take(actualCount).ToList();
    }

    /// <summary>
    /// 複数のランダムなカードを取得（重複あり）
    /// </summary>
    /// <param name="count">取得するカード数</param>
    /// <returns>ランダムなカードのリスト（重複あり）</returns>
    public List<CardData> GetRandomCardsWithDuplicates(int count)
    {
        if (count <= 0) return new List<CardData>();
        if (_availableCards.Count == 0) return new List<CardData>();

        var result = new List<CardData>();
        for (var i = 0; i < count; i++)
        {
            var randomIndex = Random.Range(0, _availableCards.Count);
            result.Add(_availableCards[randomIndex]);
        }

        return result;
    }

    /// <summary>
    /// 指定した条件に合うカードを取得
    /// </summary>
    /// <param name="predicate">検索条件</param>
    /// <returns>条件に合うカードのリスト</returns>
    public List<CardData> GetCardsWhere(System.Func<CardData, bool> predicate)
    {
        return _availableCards.Where(predicate).ToList();
    }

    /// <summary>
    /// 特定のカードIDでカードを取得
    /// </summary>
    /// <param name="cardName">カード名</param>
    /// <returns>見つかったカード（存在しない場合はnull）</returns>
    public CardData GetCardByName(string cardName)
    {
        return _availableCards.FirstOrDefault(card => card.CardName == cardName);
    }

    /// <summary>
    /// CardIdでカードを取得（全カード対象、進化・劣化先も含む）
    /// </summary>
    /// <param name="cardId">カードID</param>
    /// <returns>見つかったカード（存在しない場合はnull）</returns>
    public CardData GetCardById(string cardId)
    {
        return _allCardData.CardList.FirstOrDefault(card => card.CardId == cardId);
    }

    /// <summary>
    /// 初期デッキに使用可能なカードの数を取得
    /// </summary>
    /// <returns>進化・劣化先を除いたカード数</returns>
    public int GetInitialDeckCardCount()
    {
        return _availableCards.Count;
    }

}
