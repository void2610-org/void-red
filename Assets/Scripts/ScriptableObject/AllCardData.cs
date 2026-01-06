using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Void2610.UnityTemplate;

#if UNITY_EDITOR
#endif

/// <summary>
/// 全てのCardDataを管理するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "AllCardData", menuName = "VoidRed/All Card Data")]
public class AllCardData : ScriptableObject
{
    [SerializeField] private List<CardData> cardList = new ();
    
    // プロパティ
    public List<CardData> CardList => cardList;
    public int Count => cardList.Count;
    
    /// <summary>
    /// 同じディレクトリ内の全てのCardDataを自動的に登録
    /// </summary>
    public void RegisterAllCards()
    {
#if UNITY_EDITOR
        this.RegisterAssetsInSameDirectory(cardList, x => x.CardName);
#endif
    }
    
    /// <summary>
    /// ランダムなカードを取得
    /// </summary>
    public CardData GetRandomCard()
    {
        if (cardList.Count == 0) return null;
        return cardList[Random.Range(0, cardList.Count)];
    }
    
    /// <summary>
    /// 複数のランダムなカードを取得（重複なし）
    /// </summary>
    public List<CardData> GetRandomCards(int count)
    {
        if (count >= cardList.Count)
        {
            return new List<CardData>(cardList);
        }
        
        var shuffled = new List<CardData>(cardList);
        for (int i = 0; i < shuffled.Count; i++)
        {
            var temp = shuffled[i];
            var randomIndex = Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }
        
        return shuffled.Take(count).ToList();
    }
    
}