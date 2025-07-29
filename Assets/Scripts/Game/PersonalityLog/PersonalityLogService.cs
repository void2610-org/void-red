using System.Collections.Generic;
using UnityEngine;
using Game.PersonalityLog;

/// <summary>
/// 人格ログの管理を行うサービスクラス
/// </summary>
public class PersonalityLogService
{
    private PersonalityLogData _logData;
    private const string PERSONALITY_LOG_KEY = "personality_log";
    
    // 現在のログ状態
    private ChapterLog _currentChapter;
    
    // ターン構築用の一時データ
    private MoveLog _currentPlayerMove;
    private MoveLog _currentEnemyMove;
    private List<TurnEvent> _currentEvents;
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public PersonalityLogService()
    {
        LoadPersonalityLog();
    }
    
    /// <summary>
    /// 新しいチャプターを開始
    /// </summary>
    public void StartChapter(EnemyData enemyData)
    {
        _currentChapter = _logData.StartNewChapter(enemyData);
    }
    
    /// <summary>
    /// チャプターを完了
    /// </summary>
    public void CompleteChapter()
    {
        if (_currentChapter != null)
        {
            _currentChapter.CompleteChapter();
            SavePersonalityLog();
        }
    }
    
    /// <summary>
    /// 新しいターンを開始
    /// </summary>
    public void StartTurn()
    {
        _currentPlayerMove = null;
        _currentEnemyMove = null;
        _currentEvents = new List<TurnEvent>();
    }
    
    /// <summary>
    /// ターンを終了
    /// </summary>
    public void EndTurn()
    {
        if (_currentChapter != null)
        {
            var turnLog = new TurnLog(_currentPlayerMove, _currentEnemyMove, _currentEvents);
            _currentChapter.AddTurn(turnLog);
        }
    }
    
    /// <summary>
    /// プレイヤーのムーブを記録
    /// </summary>
    public void LogPlayerMove(PlayerMove move, int currentMentalPower)
    {
        _currentPlayerMove = new MoveLog(move, currentMentalPower);
    }
    
    /// <summary>
    /// 敵のムーブを記録
    /// </summary>
    public void LogEnemyMove(PlayerMove move, int currentMentalPower)
    {
        _currentEnemyMove = new MoveLog(move, currentMentalPower);
    }
    
    /// <summary>
    /// 共鳴イベントを記録
    /// </summary>
    public void LogResonance(CardData resonanceCard)
    {
        var resonanceEvent = new ResonanceEvent(resonanceCard);
        _currentEvents.Add(resonanceEvent);
    }
    
    /// <summary>
    /// カード進化イベントを記録
    /// </summary>
    public void LogCardEvolution(string actorId, CardData fromCard, CardData toCard)
    {
        var evolutionEvent = new CardEvolutionEvent(actorId, fromCard, toCard);
        _currentEvents.Add(evolutionEvent);
    }
    
    /// <summary>
    /// カード崩壊イベントを記録
    /// </summary>
    public void LogCardCollapse(string actorId, CardData collapseCard)
    {
        var collapseEvent = new CardCollapseEvent(actorId, collapseCard);
        _currentEvents.Add(collapseEvent);
    }
    
    /// <summary>
    /// 人格ログをロード
    /// </summary>
    private void LoadPersonalityLog()
    {
        var json = DataPersistence.LoadData(PERSONALITY_LOG_KEY);
        
        if (string.IsNullOrEmpty(json))
        {
            _logData = new PersonalityLogData();
        }
        else
        {
            _logData = JsonUtility.FromJson<PersonalityLogData>(json) ?? new PersonalityLogData();
        }
    }
    
    /// <summary>
    /// 人格ログをセーブ
    /// </summary>
    public void SavePersonalityLog()
    {
        var json = JsonUtility.ToJson(_logData, true);
        DataPersistence.SaveData(PERSONALITY_LOG_KEY, json);
    }
    
    /// <summary>
    /// ログデータを取得
    /// </summary>
    public PersonalityLogData GetLogData()
    {
        return _logData;
    }
}