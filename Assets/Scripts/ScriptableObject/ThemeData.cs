using System.Collections.Generic;
using UnityEngine;
using Void2610.UnityTemplate;

/// <summary>
/// テーマ会話データ
/// </summary>
[System.Serializable]
public class ThemeDialogue
{
    [SerializeField] private bool isPlayer; // true=プレイヤー, false=敵
    [TextArea(2, 4)]
    [SerializeField] private string message; // 会話内容

    public bool IsPlayer => isPlayer;
    public string Message => message;
}

/// <summary>
/// テーマデータを保持するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "NewThemeData", menuName = "VoidRed/Theme Data")]
public class ThemeData : ScriptableObject
{
    [Header("テーマ情報")]
    [SerializeField] private string title;
    [TextArea(2, 4)] [SerializeField] private string description;
    [SerializeField] private SerializableDictionary<CardAttribute, float> attributeMultipliers = new SerializableDictionary<CardAttribute, float>();
    [SerializeField] private List<KeywordType> keywords = new();

    [Header("会話")]
    [SerializeField] private List<ThemeDialogue> dialogues = new();

    public string Title => title;
    public string Description => description;
    public SerializableDictionary<CardAttribute, float> AttributeMultipliers => attributeMultipliers;
    public List<KeywordType> Keywords => keywords;
    public List<ThemeDialogue> Dialogues => dialogues;

    /// <summary>
    /// 指定された属性のスコア倍率を取得
    /// </summary>
    /// <param name="attribute">カード属性</param>
    /// <returns>スコア倍率（設定されていない場合は1.0）</returns>
    public float GetMultiplier(CardAttribute attribute)
    {
        return attributeMultipliers.TryGetValue(attribute, out var multiplier) ? multiplier : 1.0f;
    }
}