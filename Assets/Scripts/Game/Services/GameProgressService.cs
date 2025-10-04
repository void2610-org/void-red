using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    
    private StoryNode _currentNode; // 現在のノードを保持
    private int _currentStep;
    
    // プレイヤー状態
    private readonly List<SavedCard> _currentDeck = new();
    private EvolutionStatsData _evolutionStats = new();
    private EnemyStats _enemyStats = new();
    
    // 人格ログデータ（セーブ・ロード用）
    private PersonalityLogData _personalityLogData = new();
    
    // 閲覧済みカード
    private readonly HashSet<string> _viewedCardIds = new();
    
    // ノベル選択結果
    private readonly List<NovelChoiceResult> _novelChoiceResults = new();
    
    private readonly SaveDataManager _saveDataManager;
    private readonly CardPoolService _cardPoolService;
    
    private readonly Subject<Unit> _onDataSaved = new();
    
    public GameProgressService(SaveDataManager saveDataManager, CardPoolService cardPoolService)
    {
        _saveDataManager = saveDataManager;
        _cardPoolService = cardPoolService;
        
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
        _currentDeck.Clear();
        _currentDeck.AddRange(loadedData.SavedDeck);
        _evolutionStats = loadedData.EvolutionStats ?? new EvolutionStatsData();
        
        // 人格ログのロード
        _personalityLogData = loadedData.PersonalityLog ?? new PersonalityLogData();
        
        // 閲覧済みカードをメモリにロード
        _viewedCardIds.Clear();
        var viewedIds = loadedData.GetViewedCardIds();
        foreach (var id in viewedIds)
        {
            _viewedCardIds.Add(id);
        }
        
        // ノベル選択結果をメモリにロード
        _novelChoiceResults.Clear();
        _novelChoiceResults.AddRange(loadedData.GetAllChoiceResults());
        
        // 新規データかどうかを判定（Step0かつ結果が0件の場合）
        var isNewData = _currentStep == 0 && _results.Count == 0;
        var dataType = isNewData ? "新規データ" : "既存データ";
        
        Debug.Log($"[GameProgressService] {dataType}自動ロード: Step {_currentStep}");
        
        // 現在のノードを初期化
        _currentNode = GetNextNode();
    }
    
    /// <summary>
    /// 有効なセーブデータが存在するかチェック（ストーリー進行ベース）
    /// </summary>
    /// <returns>ストーリーが進行しているセーブデータの存在有無</returns>
    public bool HasSaveData()
    {
        return _saveDataManager.SaveFileExists() && (_currentStep > 0 || _results.Count > 0);
    }
    
    /// <summary>
    /// 全データを初期状態にリセット（デバッグ用）
    /// </summary>
    public void ResetToDefaultData()
    {
        // ストーリー進行データをリセット
        _currentStep = 0;
        _results.Clear();
        _currentNode = GetNextNode(); // 現在のノードを初期化
        
        // プレイヤー状態をリセット
        _currentDeck.Clear();
        _evolutionStats = new EvolutionStatsData();
        _enemyStats = new EnemyStats();
        
        // 人格ログをリセット
        _personalityLogData = new PersonalityLogData();
        
        // ノベル選択結果をリセット
        _novelChoiceResults.Clear();
        
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
    /// 次に発生するストーリーノードを決定（結果辞書による分岐対応）
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
            default:
                // この先は未定
                nextNode = new NovelNode("ending");
                break;
        }

        return nextNode;
    }
    
    /// <summary>
    /// StoryNodeから対応するSceneTypeを取得
    /// </summary>
    /// <param name="node">ストーリーノード</param>
    /// <returns>対応するシーンタイプ</returns>
    private SceneType GetSceneTypeForNode(StoryNode node)
    {
        return node switch
        {
            BattleNode => SceneType.Battle,
            NovelNode => SceneType.Novel,
            EndingNode => SceneType.Home, // エンディング後はホームへ
            _ => SceneType.Home
        };
    }
    
    public SceneType GetCurrentSceneType() => GetSceneTypeForNode(_currentNode);
    public SceneType GetNextSceneType() => GetSceneTypeForNode(GetNextNode());
    private string GetResult(string id) => _results.GetValueOrDefault(id, "");
    
    /// <summary>
    /// 現在のバトル結果を記録
    /// </summary>
    public void RecordBattleResultAndSave(bool isPlayerWin)
    {
        var result = isPlayerWin ? "win" : "lose";
        _results[_currentStep.ToString()] = result;
        _currentStep++;
        _currentNode = GetNextNode(); // 次のノードに更新
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
        _currentNode = GetNextNode(); // 次のノードに更新
        SaveAndPersist();
    }
    
    /// <summary>
    /// ノベル選択結果を記録してセーブ
    /// </summary>
    /// <param name="scenarioId">シナリオID</param>
    /// <param name="choiceIndex">選択肢番号</param>
    /// <param name="selectedOptionIndex">選択された選択肢のインデックス</param>
    public void RecordNovelChoiceAndSave(string scenarioId, int choiceIndex, int selectedOptionIndex)
    {
        var choiceResult = new NovelChoiceResult(scenarioId, choiceIndex, selectedOptionIndex);
        _novelChoiceResults.Add(choiceResult);
        
        Debug.Log($"[GameProgressService] 選択結果を記録: {choiceResult}");
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
        // データセーブイベントを発火
        _onDataSaved.OnNext(Unit.Default);
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
        saveData.UpdateDeck(_currentDeck);
        
        // 進化統計データと人格ログデータを設定
        saveData.UpdateEvolutionStats(_evolutionStats);
        saveData.UpdatePersonalityLog(_personalityLogData);
        
        // 閲覧済みカードをセーブデータに追加
        foreach (var cardId in _viewedCardIds)
        {
            saveData.RecordCardView(cardId);
        }
        
        // ノベル選択結果をセーブデータに追加
        foreach (var choiceResult in _novelChoiceResults)
        {
            saveData.AddNovelChoiceResult(choiceResult);
        }
        
        return saveData;
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
    /// カード閲覧をリストで記録
    /// </summary>
    /// <param name="cardDataList">閲覧したカードデータのリスト</param>
    public void RecordCardViews(List<CardData> cardDataList)
    {
        foreach (var cardData in cardDataList)
            RecordCardView(cardData);
    }

    /// <summary>
    /// カード閲覧を記録
    /// </summary>
    /// <param name="cardData">閲覧したカードデータ</param>
    public void RecordCardView(CardData cardData)
    {
        if (!cardData || string.IsNullOrEmpty(cardData.CardId)) return;
        _viewedCardIds.Add(cardData.CardId);
    }
    
    /// <summary>
    /// 閲覧済みカードIDリストを取得
    /// </summary>
    /// <returns>閲覧済みカードIDのHashSet</returns>
    public HashSet<string> GetViewedCardIds()
    {
        return new HashSet<string>(_viewedCardIds);
    }
    
    /// <summary>
    /// 特定のシナリオの選択結果を取得
    /// </summary>
    /// <param name="scenarioId">シナリオID</param>
    /// <returns>該当する選択結果のリスト</returns>
    public List<NovelChoiceResult> GetChoiceResultsByScenario(string scenarioId)
    {
        return _novelChoiceResults.FindAll(result => result.ScenarioId == scenarioId);
    }
    
    /// <summary>
    /// 全ての選択結果を取得
    /// </summary>
    /// <returns>全選択結果のリスト</returns>
    public List<NovelChoiceResult> GetAllNovelChoiceResults()
    {
        return new List<NovelChoiceResult>(_novelChoiceResults);
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
    
    /// <summary>
    /// データセーブ時のイベント
    /// </summary>
    public Observable<Unit> OnDataSaved => _onDataSaved;
    
    // === 人格ログデータ連携メソッド ===
    
    /// <summary>
    /// 人格ログデータを更新（PersonalityLogServiceから取得してセーブ用に保持）
    /// </summary>
    /// <param name="personalityLogData">更新する人格ログデータ</param>
    public void UpdatePersonalityLogData(PersonalityLogData personalityLogData)
    {
        _personalityLogData = personalityLogData ?? new PersonalityLogData();
    }
    
    /// <summary>
    /// 人格ログデータを取得（PersonalityLogServiceの初期化用）
    /// </summary>
    /// <returns>保存されている人格ログデータ</returns>
    public PersonalityLogData GetPersonalityLogData()
    {
        return _personalityLogData;
    }
}