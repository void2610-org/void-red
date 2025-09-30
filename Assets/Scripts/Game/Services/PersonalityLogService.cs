using System.Collections.Generic;
using Game.PersonalityLog;

/// <summary>
/// バトルシーンでの人格ログを管理するサービス
/// プレイヤーと敵のムーブ、ターン、チャプター、各種イベントを記録
/// </summary>
public class PersonalityLogService
{
    // 人格ログデータ
    private PersonalityLogData _personalityLog = new();
    
    // 現在のターン情報
    private MoveLog _currentPlayerMove;
    private MoveLog _currentEnemyMove;
    private readonly List<TurnEvent> _currentEvents = new();
    private ChapterLog _currentChapter;
    
    /// <summary>
    /// 新しいチャプターを開始
    /// </summary>
    /// <param name="enemyData">敵データ</param>
    public void StartChapter(EnemyData enemyData)
    {
        _currentChapter = _personalityLog.StartNewChapter(enemyData);
    }
    
    /// <summary>
    /// チャプターを完了
    /// </summary>
    public void CompleteChapter()
    {
        if (_currentChapter != null)
        {
            _currentChapter.CompleteChapter();
        }
    }
    
    /// <summary>
    /// 新しいターンを開始
    /// </summary>
    public void StartTurn()
    {
        _currentPlayerMove = null;
        _currentEnemyMove = null;
        _currentEvents.Clear();
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
    /// <param name="move">プレイヤーの手</param>
    /// <param name="currentMentalPower">現在の精神力</param>
    public void LogPlayerMove(PlayerMove move, int currentMentalPower)
    {
        _currentPlayerMove = new MoveLog(move, currentMentalPower);
    }
    
    /// <summary>
    /// 敵のムーブを記録
    /// </summary>
    /// <param name="move">敵の手</param>
    /// <param name="currentMentalPower">現在の精神力</param>
    public void LogEnemyMove(PlayerMove move, int currentMentalPower)
    {
        _currentEnemyMove = new MoveLog(move, currentMentalPower);
    }
    
    /// <summary>
    /// 共鳴イベントを記録
    /// </summary>
    /// <param name="actorId">アクターID（"player"または"enemy"）</param>
    /// <param name="resonanceCard">共鳴したカード</param>
    public void LogResonance(string actorId, CardData resonanceCard)
    {
        var resonanceEvent = new ResonanceEvent(actorId, resonanceCard);
        _currentEvents.Add(resonanceEvent);
    }
    
    /// <summary>
    /// カード進化イベントを記録
    /// </summary>
    /// <param name="actorId">アクターID（"player"または"enemy"）</param>
    /// <param name="fromCard">進化前のカード</param>
    /// <param name="toCard">進化後のカード</param>
    public void LogCardEvolution(string actorId, CardData fromCard, CardData toCard)
    {
        var evolutionEvent = new CardEvolutionEvent(actorId, fromCard, toCard);
        _currentEvents.Add(evolutionEvent);
    }
    
    /// <summary>
    /// カード崩壊イベントを記録
    /// </summary>
    /// <param name="actorId">アクターID（"player"または"enemy"）</param>
    /// <param name="collapseCard">崩壊したカード</param>
    public void LogCardCollapse(string actorId, CardData collapseCard)
    {
        var collapseEvent = new CardCollapseEvent(actorId, collapseCard);
        _currentEvents.Add(collapseEvent);
    }
    
    /// <summary>
    /// 人格ログデータを取得
    /// </summary>
    /// <returns>現在の人格ログデータ</returns>
    public PersonalityLogData GetPersonalityLogData()
    {
        return _personalityLog;
    }
    
    /// <summary>
    /// 人格ログデータを設定（セーブデータからのロード用）
    /// </summary>
    /// <param name="personalityLogData">ロードする人格ログデータ</param>
    public void SetPersonalityLogData(PersonalityLogData personalityLogData)
    {
        _personalityLog = personalityLogData ?? new PersonalityLogData();
        // 現在のチャプターとターン情報はクリア（バトル開始時に再初期化される）
        _currentChapter = null;
        _currentPlayerMove = null;
        _currentEnemyMove = null;
        _currentEvents.Clear();
    }
}