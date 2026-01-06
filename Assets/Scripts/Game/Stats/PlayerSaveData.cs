using System;
using UnityEngine;

/// <summary>
/// プレイヤー固有のセーブデータ（統計データ + 将来の章クリア状況等）
/// </summary>
[Serializable]
public class PlayerSaveData
{
    [Header("ゲーム進行データ")]
    [SerializeField] private int currentChapter = 0;
    
    [Header("プレイヤー関連データ")]
    [SerializeField] private int currentMentalPower = GameConstants.MAX_MENTAL_POWER;
    
    public int CurrentChapter => currentChapter;
    public int CurrentMentalPower => currentMentalPower;
    
    /// <summary>
    /// 現在の精神力を更新
    /// </summary>
    /// <param name="mentalPower">新しい精神力の値</param>
    public void UpdateMentalPower(int mentalPower)
    {
        currentMentalPower = Mathf.Clamp(mentalPower, 0, GameConstants.MAX_MENTAL_POWER);
    }
    
    /// <summary>
    /// デバッグ用：統計情報を文字列で取得
    /// </summary>
    public string GetStatsString()
    {
        return $"Chapter: {currentChapter}, MentalPower: {currentMentalPower}";
    }
}