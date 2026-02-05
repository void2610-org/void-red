using System.Collections.Generic;
using UnityEngine;
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

    [Header("敵画像")]
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite iconSprite;
    [SerializeField] private Sprite frameSprite;

    [Header("属性別画像")]
    [SerializeField] private SerializableDictionary<CardAttribute, Sprite> attributeSprites = new();

    public string EnemyId => enemyId;
    public string EnemyName => enemyName;
    public Sprite DefaultSprite => defaultSprite;
    public Sprite IconSprite => iconSprite;
    public Sprite FrameSprite => frameSprite;

    /// <summary>
    /// 指定された属性に対応するSpriteを取得
    /// 対応するSpriteがない場合はnullを返す
    /// </summary>
    public Sprite GetSpriteForAttribute(CardAttribute attribute)
    {
        return attributeSprites.GetValueOrDefault(attribute);
    }
}
