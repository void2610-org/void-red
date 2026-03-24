using UnityEngine;

/// <summary>
/// カード情報を定義するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Card", menuName = "VoidRed/Card Data")]
public class CardData : ScriptableObject
{
    [Header("基本情報")]
    [SerializeField] private string cardName;
    [SerializeField, TextArea(2, 5)] private string description;
    [SerializeField] private Sprite image;

    [Header("記憶情報")]
    [SerializeField] private MemoryType memoryType;
    [SerializeField] private EmotionType cardEmotion;

    public string CardId => name;
    public string CardName => cardName;
    public string Description => description;
    public Sprite CardImage => image;
    public MemoryType MemoryType => memoryType;
    /// <summary>このカードが司る感情</summary>
    public EmotionType CardEmotion => cardEmotion;
}
