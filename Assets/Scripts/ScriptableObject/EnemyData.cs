using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 敵情報を定義するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Enemy", menuName = "VoidRed/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("基本情報")]
    [SerializeField] private string enemyId;
    [SerializeField] private string enemyName;
    [SerializeField] private Sprite enemyImage;
    [SerializeField, TextArea(3, 5)] private string description;
    
    [Header("ステータス")]
    [SerializeField] private int maxMentalPower = 10;
    [SerializeField] private int initialMentalPower = 10;
    
    [Header("デッキ構成")]
    [SerializeField] private List<CardData> initialDeck = new ();
    
    // プロパティ
    public string EnemyId => enemyId;
    public string EnemyName => enemyName;
    public Sprite EnemyImage => enemyImage;
    public string Description => description;
    public int MaxMentalPower => maxMentalPower;
    public int InitialMentalPower => initialMentalPower;
    public List<CardData> InitialDeck => initialDeck;
}