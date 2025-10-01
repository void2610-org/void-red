using UnityEngine;
using System.Collections.Generic;
using Void2610.UnityTemplate;

/// <summary>
/// 敵情報を定義するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Enemy", menuName = "VoidRed/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("基本情報")]
    [SerializeField] private string enemyId;
    [SerializeField] private string enemyName;
    [SerializeField, TextArea(3, 5)] private string description;
    
    [Header("敵画像")]
    [SerializeField] private Sprite defaultSprite;
    
    [Header("属性別画像")]
    [SerializeField] private SerializableDictionary<CardAttribute, Sprite> attributeSprites = new ();
    
    [Header("ステータス")]
    [SerializeField] private int maxMentalPower = 10;
    [SerializeField] private int initialMentalPower = 10;
    
    [Header("デッキ構成")]
    [SerializeField] private List<CardData> initialDeck = new ();
    
    [Header("共鳴システム")]
    [SerializeField] private CardData resonanceCard;

    [Header("テーマ設定")]
    [SerializeField] private ThemeData majorTheme; // 大テーマ（どちらかが2勝時）
    [SerializeField] private List<ThemeData> minorThemes = new(); // 小テーマ（通常時）

    // プロパティ
    public string EnemyId => enemyId;
    public string EnemyName => enemyName;
    public string Description => description;
    public Sprite DefaultSprite => defaultSprite;
    public int MaxMentalPower => maxMentalPower;
    public int InitialMentalPower => initialMentalPower;
    public List<CardData> InitialDeck => initialDeck;
    public CardData ResonanceCard => resonanceCard;
    public ThemeData MajorTheme => majorTheme;
    public List<ThemeData> MinorThemes => minorThemes;
    
    /// <summary>
    /// 指定された属性に対応するSpriteを取得
    /// 対応するSpriteがない場合はnullを返す
    /// </summary>
    public Sprite GetSpriteForAttribute(CardAttribute attribute)
    {
        if (attributeSprites.TryGetValue(attribute, out var sprite))
        {
            return sprite;
        }
        return null;
    }
}