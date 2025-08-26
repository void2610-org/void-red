using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using Game.PersonalityLog;
using R3;

/// <summary>
/// ゲーム全体の進行度を管理し、次のイベントを決定するサービス
/// ストーリーの分岐ロジック、結果管理、シーン遷移を統合管理
/// </summary>
public class GameProgressService
{
    /// <summary>
    /// バトル結果やノベル選択結果を管理する辞書
    /// </summary>
    private readonly Dictionary<string, string> _results = new();
    
    private StoryNode _currentStoryNode;
    private int _currentStep;
    
    // プレイヤー状態
    private int _currentMentalPower = GameConstants.MAX_MENTAL_POWER;
    private List<string> _currentDeck = new();
    private EvolutionStatsData _evolutionStats = new();
    private EnemyStats _enemyStats = new();
    
    // 人格ログ
    private PersonalityLogData _personalityLog = new();
    private MoveLog _currentPlayerMove;
    private MoveLog _currentEnemyMove;
    private List<TurnEvent> _currentEvents = new();
    private ChapterLog _currentChapter;
    
    private readonly SaveDataManager _saveDataManager;
    
    public GameProgressService(SaveDataManager saveDataManager)
    {
        _saveDataManager = saveDataManager;
        
        // 起動時に自動でセーブデータをロード
        LoadAllGameData();
    }
    
    /// <summary>
    /// 起動時の全ゲームデータ自動ロード
    /// </summary>
    private void LoadAllGameData()
    {
        var loadedData = _saveDataManager.LoadGameData();
        
        // ストーリー進行データのロード
        _currentStep = loadedData.CurrentStep;
        _results.Clear();
        var loadedResults = loadedData.GetResults();
        foreach (var result in loadedResults)
        {
            _results[result.Key] = result.Value;
        }
        
        // プレイヤー状態のロード
        _currentMentalPower = loadedData.CurrentMentalPower;
        _currentDeck.Clear();
        _currentDeck.AddRange(loadedData.CurrentDeck);
        _evolutionStats = loadedData.EvolutionStats ?? new EvolutionStatsData();
        
        // 人格ログのロード
        _personalityLog = loadedData.PersonalityLog ?? new PersonalityLogData();
        
        // 新規データかどうかを判定（Step0かつ結果が0件の場合）
        var isNewData = _currentStep == 0 && _results.Count == 0;
        var dataType = isNewData ? "新規データ" : "既存データ";
        
        Debug.Log($"[GameProgressService] {dataType}自動ロード: Step {_currentStep}");
    }
    
    /// <summary>
    /// 全データを初期状態にリセット（デバッグ用）
    /// </summary>
    public void ResetToDefaultData()
    {
        // ストーリー進行データをリセット
        _currentStep = 0;
        _results.Clear();
        
        // プレイヤー状態をリセット
        _currentMentalPower = GameConstants.MAX_MENTAL_POWER;
        _currentDeck.Clear();
        _evolutionStats = new EvolutionStatsData();
        _enemyStats = new EnemyStats();
        
        // 人格ログをリセット
        _personalityLog = new PersonalityLogData();
        _currentPlayerMove = null;
        _currentEnemyMove = null;
        _currentEvents.Clear();
        _currentChapter = null;
        
        // リセット後に即座にセーブファイルに反映
        SaveAndPersist();
        
        Debug.Log("[GameProgressService] 全データを初期状態にリセット完了");
    }
    
    /// <summary>
    /// 現在のストーリーノードを取得
    /// </summary>
    /// <returns>現在のストーリーノード</returns>
    public StoryNode GetCurrentNode()
    {
        return GetNextNode();
    }
    
    /// <summary>
    /// 次に発生するストーリーノードを決定（結果辞書による分岐対応）
    /// </summary>
    /// <returns>次のストーリーノード</returns>
    public StoryNode GetNextNode()
    {
        Debug.Log($"[GameProgressService] 次ノード決定: 現在Step {_currentStep}");
        
        // 結果辞書を使用したストーリー分岐ロジック
        StoryNode nextNode = _currentStep switch
        {
            // 導入ノベル
            0 => new NovelNode("S001"),
            // 最初のバトル
            1 => new BattleNode("E001"),
            // 最初のバトル結果に応じた分岐
            2 => GetResult("1") == "win" 
                ? new NovelNode("S011") 
                : new NovelNode("S012"),
            // 2番目のバトル
            3 => new BattleNode("E002"),
            // 2番目のバトル結果に応じた分岐
            4 => GetResult("3") == "win"
                ? new NovelNode("S021")
                : new NovelNode("S022"),
            // 最終バトル
            5 => new BattleNode("E003"),
            // ゲーム終了
            _ => new EndingNode()
        };
        
        Debug.Log($"[GameProgressService] 決定されたノード: {nextNode.GetType().Name} ({nextNode.NodeId})");
        return nextNode;
    }
    
    /// <summary>
    /// 結果を取得
    /// </summary>
    /// <param name="id">結果のID</param>
    /// <returns>結果の値（存在しない場合は空文字）</returns>
    private string GetResult(string id)
    {
        return _results.TryGetValue(id, out var value) ? value : "";
    }
    
    /// <summary>
    /// 現在のバトル結果を記録
    /// </summary>
    public void RecordBattleResultAndSave(bool isPlayerWin)
    {
        // TODO: カードとか精神力もここで保存したい
        var result = isPlayerWin ? "win" : "lose";
        _results[_currentStep.ToString()] = result;
        _currentStep++;
        SaveAndPersist();
    }
    
    /// <summary>
    /// ノベル完了（複数選択記録 + 進行 + セーブを統合）
    /// </summary>
    public void RecordNovelResultAndSave(Dictionary<string, string> choices)
    {
        foreach (var choice in choices)
            _results[choice.Key] = choice.Value;
        _currentStep++;
        SaveAndPersist();
    }
    
    /// <summary>
    /// ゲーム進行状態をセーブ（全サービス統合）
    /// </summary>
    private void SaveAndPersist()
    {
        var saveData = CreateGameSaveData();
        
        // 実際にファイルに保存
        _saveDataManager.SaveGameData(saveData);
        
        Debug.Log("[GameProgressService] 統合セーブ完了");
    }
    
    /// <summary>
    /// 現在の状態からGameSaveDataを作成
    /// </summary>
    private GameSaveData CreateGameSaveData()
    {
        var saveData = new GameSaveData();
        
        // ストーリー進行データを設定
        saveData.UpdateGameProgress(_currentStep, _results);
        
        // プレイヤー状態を設定
        saveData.UpdateMentalPower(_currentMentalPower);
        saveData.UpdateDeck(_currentDeck);
        
        // 進化統計データと人格ログデータを設定
        saveData.UpdateEvolutionStats(_evolutionStats);
        saveData.UpdatePersonalityLog(_personalityLog);
        
        return saveData;
    }
    
    // === GameStatsServiceから移動したメソッド ===
    
    /// <summary>
    /// プレイヤーの精神力を取得
    /// </summary>
    public int GetPlayerMentalPower()
    {
        return _currentMentalPower;
    }
    
    /// <summary>
    /// プレイヤーの精神力を更新
    /// </summary>
    public void UpdatePlayerMentalPower(int mentalPower)
    {
        _currentMentalPower = Mathf.Clamp(mentalPower, 0, GameConstants.MAX_MENTAL_POWER);
        Debug.Log($"[GameProgressService] プレイヤー精神力更新: {_currentMentalPower}");
    }
    
    /// <summary>
    /// デッキ情報を更新
    /// </summary>
    public void UpdateDeck(List<string> deck)
    {
        _currentDeck.Clear();
        _currentDeck.AddRange(deck);
        Debug.Log($"[GameProgressService] デッキ更新: {_currentDeck.Count}枚");
    }
    
    /// <summary>
    /// 敵の統計をリセット
    /// </summary>
    public void ResetEnemyStats()
    {
        _enemyStats = new EnemyStats();
        Debug.Log("[GameProgressService] 敵統計リセット");
    }
    
    /// <summary>
    /// ゲーム結果を記録（プレイヤー分）
    /// </summary>
    public void RecordPlayerGameResult(bool playerWon, PlayerMove playerMove, bool playerCollapsed)
    {
        _evolutionStats.RecordGameResult(playerWon, playerMove, playerCollapsed);
        Debug.Log($"[GameProgressService] プレイヤー結果記録: {(playerWon ? "勝利" : "敗北")}, Move: {playerMove}, Collapsed: {playerCollapsed}");
    }
    
    /// <summary>
    /// カード進化チェック（プレイヤー分）
    /// </summary>
    public CardData CheckPlayerCardEvolution(CardData card)
    {
        if (_evolutionStats.CheckAllEvolutionConditions(card))
        {
            Debug.Log($"[GameProgressService] カード進化: {card.CardName} → {card.EvolutionTarget.CardName}");
            return card.EvolutionTarget;
        }
        
        if (_evolutionStats.CheckAllDegradationConditions(card))
        {
            Debug.Log($"[GameProgressService] カード劣化: {card.CardName} → {card.DegradationTarget.CardName}");
            return card.DegradationTarget;
        }
        
        return card;
    }
    
    /// <summary>
    /// プレイヤー進化統計データを取得
    /// </summary>
    public EvolutionStatsData PlayerEvolutionStats => _evolutionStats;
    
    /// <summary>
    /// 敵統計データを取得
    /// </summary>
    public EnemyStats EnemyStats => _enemyStats;
    
    // === PersonalityLogServiceから移動したメソッド ===
    
    /// <summary>
    /// 新しいチャプターを開始
    /// </summary>
    public void StartChapter(EnemyData enemyData)
    {
        _currentChapter = _personalityLog.StartNewChapter(enemyData);
        Debug.Log($"[GameProgressService] チャプター開始: {enemyData.EnemyName}");
    }
    
    /// <summary>
    /// チャプターを完了
    /// </summary>
    public void CompleteChapter()
    {
        if (_currentChapter != null)
        {
            _currentChapter.CompleteChapter();
            Debug.Log("[GameProgressService] チャプター完了");
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
        Debug.Log("[GameProgressService] ターン開始");
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
            Debug.Log("[GameProgressService] ターン終了");
        }
    }
    
    /// <summary>
    /// プレイヤーのムーブを記録
    /// </summary>
    public void LogPlayerMove(PlayerMove move, int currentMentalPower)
    {
        _currentPlayerMove = new MoveLog(move, currentMentalPower);
        Debug.Log($"[GameProgressService] プレイヤームーブ記録: {move}");
    }
    
    /// <summary>
    /// 敵のムーブを記録
    /// </summary>
    public void LogEnemyMove(PlayerMove move, int currentMentalPower)
    {
        _currentEnemyMove = new MoveLog(move, currentMentalPower);
        Debug.Log($"[GameProgressService] 敵ムーブ記録: {move}");
    }
    
    /// <summary>
    /// 共鳴イベントを記録
    /// </summary>
    public void LogResonance(string actorId, CardData resonanceCard)
    {
        var resonanceEvent = new ResonanceEvent(actorId, resonanceCard);
        _currentEvents.Add(resonanceEvent);
        Debug.Log($"[GameProgressService] 共鳴イベント記録: {actorId} - {resonanceCard.CardName}");
    }
    
    /// <summary>
    /// カード進化イベントを記録
    /// </summary>
    public void LogCardEvolution(string actorId, CardData fromCard, CardData toCard)
    {
        var evolutionEvent = new CardEvolutionEvent(actorId, fromCard, toCard);
        _currentEvents.Add(evolutionEvent);
        Debug.Log($"[GameProgressService] カード進化イベント記録: {actorId} - {fromCard.CardName} → {toCard.CardName}");
    }
    
    /// <summary>
    /// カード崩壊イベントを記録
    /// </summary>
    public void LogCardCollapse(string actorId, CardData collapseCard)
    {
        var collapseEvent = new CardCollapseEvent(actorId, collapseCard);
        _currentEvents.Add(collapseEvent);
        Debug.Log($"[GameProgressService] カード崩壊イベント記録: {actorId} - {collapseCard.CardName}");
    }
    
    /// <summary>
    /// 人格ログデータを取得
    /// </summary>
    public PersonalityLogData GetPersonalityLogData()
    {
        return _personalityLog;
    }
    
    /// <summary>
    /// GameSaveDataからゲーム進行状態を復元
    /// </summary>
    public void LoadState(GameSaveData saveData)
    {
        _currentStep = saveData.CurrentStep;
        _results.Clear();
        
        var loadedResults = saveData.GetResults();
        foreach (var result in loadedResults)
        {
            _results[result.Key] = result.Value;
        }
        
        Debug.Log($"[GameProgressService] ゲーム進行状態をロード: Step {_currentStep}, Results {_results.Count}件");
    }
    
    /// <summary>
    /// 指定したシーンに遷移
    /// </summary>
    /// <param name="targetScene">遷移先のシーンタイプ</param>
    /// <returns>遷移完了のUniTask</returns>
    public async UniTask TransitionToScene(SceneType targetScene)
    {
        var sceneName = targetScene.ToSceneName();
        var asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        await UniTask.WaitUntil(() => asyncOperation.isDone);
    }
}