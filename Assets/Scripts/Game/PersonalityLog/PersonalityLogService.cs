using System;
using System.Collections.Generic;
using UnityEngine;
using R3;
using Game.PersonalityLog;

/// <summary>
/// 人格ログの管理を行うサービスクラス
/// </summary>
public class PersonalityLogService : IDisposable
{
    private const string PERSONALITY_LOG_KEY = "personality_log";
    
    public Observable<Unit> OnLogUpdated => _onLogUpdated;
    
    private MoveLog _currentPlayerMove;
    private MoveLog _currentEnemyMove;
    private List<TurnEvent> _currentEvents;
    private ChapterLog _currentChapter;
    private PersonalityLogData _logData;
    private readonly Subject<Unit> _onLogUpdated = new();
    
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
            _onLogUpdated.OnNext(Unit.Default);
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
    public void LogResonance(string actorId, CardData resonanceCard)
    {
        var resonanceEvent = new ResonanceEvent(actorId, resonanceCard);
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
    /// <returns>保存が成功したかどうか</returns>
    public bool SavePersonalityLog()
    {
        var json = JsonUtility.ToJson(_logData, true);
        return DataPersistence.SaveData(PERSONALITY_LOG_KEY, json);
    }
    
    /// <summary>
    /// ログデータを取得
    /// </summary>
    public PersonalityLogData GetLogData()
    {
        return _logData;
    }
    
    /// <summary>
    /// 人格ログファイルを削除（デバッグ用）
    /// </summary>
    /// <returns>削除が成功したかどうか</returns>
    public bool DeletePersonalityLog()
    {
        return DataPersistence.DeleteData(PERSONALITY_LOG_KEY);
    }
    
    /// <summary>
    /// 人格ログデータを再読み込み（デバッグ用）
    /// </summary>
    public void ReloadPersonalityLog()
    {
        LoadPersonalityLog();
        _onLogUpdated.OnNext(Unit.Default);
    }
    
    /// <summary>
    /// リソース解放
    /// </summary>
    public void Dispose()
    {
        _onLogUpdated?.Dispose();
    }
}