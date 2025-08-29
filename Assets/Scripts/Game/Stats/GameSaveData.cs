using System;
using System.Collections.Generic;
using UnityEngine;
using Game.PersonalityLog;

/// <summary>
/// 全ゲーム情報を統合したセーブデータクラス
/// </summary>
[Serializable]
public class GameSaveData
{
    [Header("基礎ゲームデータ")]
    [SerializeField] private int currentMentalPower = GameConstants.MAX_MENTAL_POWER;
    [SerializeField] private List<string> currentDeck = new();
    
    [Header("ゲーム進行データ")]
    [SerializeField] private int currentStep = 0;
    [SerializeField] private List<string> resultKeys = new();
    [SerializeField] private List<string> resultValues = new();
    
    [Header("統計・進化データ")]
    [SerializeField] private EvolutionStatsData evolutionStats = new();
    
    [Header("人格ログデータ")]
    [SerializeField] private PersonalityLogData personalityLog = new();
    
    // プロパティ
    public int CurrentMentalPower => currentMentalPower;
    public List<string> CurrentDeck => currentDeck;
    public int CurrentStep => currentStep;
    public EvolutionStatsData EvolutionStats => evolutionStats;
    public PersonalityLogData PersonalityLog => personalityLog;
    
    /// <summary>
    /// 精神力を更新
    /// </summary>
    public void UpdateMentalPower(int mentalPower)
    {
        currentMentalPower = Mathf.Clamp(mentalPower, 0, GameConstants.MAX_MENTAL_POWER);
    }
    
    /// <summary>
    /// デッキ情報を更新
    /// </summary>
    public void UpdateDeck(List<string> deck)
    {
        currentDeck.Clear();
        currentDeck.AddRange(deck);
    }
    
    /// <summary>
    /// ゲーム進行情報を更新
    /// </summary>
    public void UpdateGameProgress(int step, Dictionary<string, string> results)
    {
        currentStep = step;
        
        resultKeys.Clear();
        resultValues.Clear();
        
        foreach (var result in results)
        {
            resultKeys.Add(result.Key);
            resultValues.Add(result.Value);
        }
    }
    
    /// <summary>
    /// 進化統計データを更新
    /// </summary>
    public void UpdateEvolutionStats(EvolutionStatsData evolutionStatsData)
    {
        evolutionStats = evolutionStatsData ?? new EvolutionStatsData();
    }
    
    /// <summary>
    /// 人格ログデータを更新
    /// </summary>
    public void UpdatePersonalityLog(PersonalityLogData personalityLogData)
    {
        personalityLog = personalityLogData ?? new PersonalityLogData();
    }
    
    /// <summary>
    /// 結果辞書を取得
    /// </summary>
    public Dictionary<string, string> GetResults()
    {
        var results = new Dictionary<string, string>();
        
        for (int i = 0; i < Mathf.Min(resultKeys.Count, resultValues.Count); i++)
        {
            results[resultKeys[i]] = resultValues[i];
        }
        
        return results;
    }
    
    /// <summary>
    /// デバッグ用情報文字列
    /// </summary>
    public string GetDebugInfo()
    {
        return $"Step: {currentStep}, MentalPower: {currentMentalPower}, Deck: {currentDeck.Count}cards, Results: {resultKeys.Count}entries";
    }
}