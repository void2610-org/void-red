using UnityEngine;

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
    [SerializeField] private Color color = Color.white;
    [SerializeField] private bool isTextColorBlack = false;
    
    public string CardId => cardId;
    public string CardName => cardName;
    public CardAttribute Attribute => attribute;
    public Sprite CardImage => image;
    public Color Color => color;
    public bool IsTextColorBlack => isTextColorBlack;
}