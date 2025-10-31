using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Void2610.UnityTemplate;

/// <summary>
/// カード情報を定義するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Card", menuName = "VoidRed/Card Data")]
public class CardData : ScriptableObject
{
    [Header("基本情報")]
    [SerializeField] private string cardId;
    [SerializeField] private string cardName;
    [SerializeField] private CardAttribute attribute;
    [SerializeField] private Sprite image;
    [SerializeField] private float scoreMultiplier = 1.0f;
    [SerializeField] private int collapseThreshold = 3;
    [SerializeField] private Color color = Color.white;
    [SerializeField] private bool isTextColorBlack = false;
    [SerializeField] private List<string> keywords = new();
    
    [Header("進化システム")]
    [SerializeField] private List<EvolutionConditionGroup> evolutionConditionGroups = new List<EvolutionConditionGroup>();
    [SerializeField] private CardData evolutionTarget;
    
    [Header("カードタイプ")]
    [SerializeField] private bool isTransformationTarget = false; // 進化・劣化先のカードかどうか（初期デッキには含まない）
    
    [Header("劣化システム")]
    [SerializeField] private List<EvolutionConditionGroup> degradationConditionGroups = new List<EvolutionConditionGroup>();
    [SerializeField] private CardData degradationTarget;
    
    public string CardId => cardId;
    public string CardName => cardName;
    public CardAttribute Attribute => attribute;
    public Sprite CardImage => image;
    public float ScoreMultiplier => scoreMultiplier;
    public int CollapseThreshold => collapseThreshold;
    public Color Color => color;
    public bool IsTextColorBlack => isTextColorBlack;
    public List<string> Keywords => keywords;
    
    public List<EvolutionConditionGroup> EvolutionConditionGroups => evolutionConditionGroups;
    public CardData EvolutionTarget => evolutionTarget;
    public bool IsTransformationTarget => isTransformationTarget;
    
    public List<EvolutionConditionGroup> DegradationConditionGroups => degradationConditionGroups;
    public CardData DegradationTarget => degradationTarget;
    
    /// <summary>
    /// 進化可能かどうかを判定
    /// </summary>
    public bool CanEvolve => evolutionTarget && evolutionConditionGroups.Count > 0;
    
    /// <summary>
    /// 劣化可能かどうかを判定
    /// </summary>
    public bool CanDegrade => degradationTarget && degradationConditionGroups.Count > 0;

    /// <summary>
    /// カード情報から表示用の説明文を生成
    /// </summary>
    /// <returns>カードの説明文</returns>
    public string GetCardDescription()
    {
        // カードのタイプや効果に基づいて説明文を生成
        var ks = Keywords is { Count: > 0 }
            ? string.Join(", ", Keywords)
            : "なし";

        return $"属性: {Attribute.ToJapaneseName()}\nキーワード: {ks}";
    }
}