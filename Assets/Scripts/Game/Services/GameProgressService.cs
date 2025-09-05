using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    private StoryNode _currentNode;
    private int _currentStep;
    
    // プレイヤー状態
    private int _currentMentalPower = GameConstants.MAX_MENTAL_POWER;
    private readonly List<SavedCard> _currentDeck = new();
    private EvolutionStatsData _evolutionStats = new();
    private EnemyStats _enemyStats = new();
    
    // 人格ログ
    private PersonalityLogData _personalityLog = new();
    private MoveLog _currentPlayerMove;
    private MoveLog _currentEnemyMove;
    private readonly List<TurnEvent> _currentEvents = new();
    private ChapterLog _currentChapter;
    
    private readonly SaveDataManager _saveDataManager;
    private readonly CardPoolService _cardPoolService;
    private readonly SceneTransitionManager _sceneTransitionManager;
    
    public GameProgressService(SaveDataManager saveDataManager, CardPoolService cardPoolService, SceneTransitionManager sceneTransitionManager)
    {
        _saveDataManager = saveDataManager;
        _cardPoolService = cardPoolService;
        _sceneTransitionManager = sceneTransitionManager;
        
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
        _currentDeck.AddRange(loadedData.SavedDeck);
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
    public StoryNode GetCurrentNode() => _currentNode;
    
    /// <summary>
    /// 次に発生するストーリーノードを決定
    /// </summary>
    /// <returns>次のストーリーノード</returns>
    public StoryNode GetNextNode()
    {
        StoryNode nextNode;
        switch (_currentStep)
        {
            // プロローグ1 - 次のバトルへ直接遷移
            case 0:
                nextNode = new NovelNode("prologue1", false);
                break;
            // アルヴ - バトル後は次のノベルへ直接遷移
            case 1:
                nextNode = new BattleNode("E001", false);
                break;
            // プロローグ2 - ノベル後はホームに戻る（デフォルトtrue）
            case 2:
                nextNode = new NovelNode("prologue2");
                break;
            // 敵2は分岐 - バトル後はホームに戻る
            case 3:
                nextNode = new BattleNode(UnityEngine.Random.Range(0f,1f) > 0.5f ? "E002" : "E003");
                break;
            default:
                // この先は未定
                nextNode = new NovelNode("ending");
                break;
        }

        _currentNode = nextNode;
        return nextNode;
    }
    
    /// <summary>
    /// 次のシーンタイプを取得
    /// </summary>
    public SceneType GetNextSceneType()
    {
        var nextNode = GetNextNode();
        return nextNode switch
        {
            BattleNode => SceneType.Battle,
            NovelNode => SceneType.Novel,
            _ => SceneType.Home
        };
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
    
    // === プレイヤー状態管理メソッド ===
    
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
    }
    
    /// <summary>
    /// デッキ情報を更新（CardModelから変換）
    /// </summary>
    public void UpdateDeckFromCardModels(IReadOnlyList<CardModel> cardModels)
    {
        _currentDeck.Clear();
        foreach (var cardModel in cardModels)
        {
            if (cardModel?.Data)
            {
                _currentDeck.Add(new SavedCard(
                    cardModel.Data.CardId,
                    cardModel.InstanceId,
                    cardModel.IsCollapsed
                ));
            }
        }
    }
    
    /// <summary>
    /// CardIdのリストからCardModelのリストに変換
    /// </summary>
    public List<CardModel> ConvertDeckToCardModels()
    {
        var cardModels = new List<CardModel>();
        foreach (var savedCard in _currentDeck)
        {
            var cardData = _cardPoolService.GetCardById(savedCard.cardId);
            if (cardData)
            {
                var cardModel = new CardModel(cardData, savedCard.instanceId, savedCard.isCollapsed);
                cardModels.Add(cardModel);
            }
            else
            {
                Debug.LogWarning($"[GameProgressService] カードID '{savedCard.cardId}' が見つかりませんでした");
            }
        }
        return cardModels;
    }
    
    /// <summary>
    /// デッキ表示用の詳細情報を取得
    /// </summary>
    public (List<CardData> allCards, List<CardData> activeCards, List<CardData> collapsedCards) GetDeckDisplayData()
    {
        var cardModels = ConvertDeckToCardModels();
        var allCards = cardModels.Select(cm => cm.Data).ToList();
        var activeCards = cardModels.Where(cm => !cm.IsCollapsed).Select(cm => cm.Data).ToList();
        var collapsedCards = cardModels.Where(cm => cm.IsCollapsed).Select(cm => cm.Data).ToList();
        
        return (allCards, activeCards, collapsedCards);
    }
    
    /// <summary>
    /// デッキ表示用のCardModelリストを取得
    /// </summary>
    public List<CardModel> GetDeckCardModels()
    {
        return ConvertDeckToCardModels();
    }
    
    /// <summary>
    /// 敵の統計をリセット
    /// </summary>
    public void ResetEnemyStats()
    {
        _enemyStats = new EnemyStats();
    }
    
    /// <summary>
    /// ゲーム結果を記録（プレイヤー分）
    /// </summary>
    public void RecordPlayerGameResult(bool playerWon, PlayerMove playerMove, bool playerCollapsed)
    {
        _evolutionStats.RecordGameResult(playerWon, playerMove, playerCollapsed);
    }
    
    /// <summary>
    /// カード進化チェック（プレイヤー分）
    /// </summary>
    public CardData CheckPlayerCardEvolution(CardData card)
    {
        if (_evolutionStats.CheckAllEvolutionConditions(card))
        {
            return card.EvolutionTarget;
        }
        
        if (_evolutionStats.CheckAllDegradationConditions(card))
        {
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
    
    // === 人格ログ管理メソッド ===
    
    /// <summary>
    /// 新しいチャプターを開始
    /// </summary>
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
    }
}