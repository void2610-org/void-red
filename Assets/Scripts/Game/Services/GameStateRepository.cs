using R3;
using UnityEngine;
using Game.PersonalityLog;

/// <summary>
/// ゲーム状態データの保持とI/Oを担当するリポジトリ
/// データの永続化とイベント発行を管理
/// </summary>
public class GameStateRepository
{
    public StoryProgressData StoryProgress { get; } = new();
    public PlayerProgressData PlayerProgress { get; } = new();
    public NovelProgressData NovelProgress { get; } = new();
    public PersonalityLogData PersonalityLogData { get; } = new();

    // 依存サービス
    private readonly SaveDataManager _saveDataManager;
    private readonly CardPoolService _cardPoolService;

    private readonly Subject<Unit> _onDataSaved = new();

    public Observable<Unit> OnDataSaved => _onDataSaved;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public GameStateRepository(SaveDataManager saveDataManager, CardPoolService cardPoolService)
    {
        _saveDataManager = saveDataManager;
        _cardPoolService = cardPoolService;

        // 起動時に自動ロード
        LoadAll();
    }
    
    /// <summary>
    /// 全データをセーブ
    /// </summary>
    public void SaveAll()
    {
        var saveData = CreateGameSaveData();
        _saveDataManager.SaveGameData(saveData);
        _onDataSaved.OnNext(Unit.Default);
    }

    /// <summary>
    /// 全データをロード
    /// </summary>
    private void LoadAll()
    {
        var loadedData = _saveDataManager.LoadGameData();

        // ストーリー進行データのロード
        StoryProgress.CurrentStep = loadedData.CurrentStep;
        StoryProgress.BattleResults.Clear();
        var loadedResults = loadedData.GetBattleResults();
        foreach (var result in loadedResults)
        {
            StoryProgress.BattleResults[result.Key] = result.Value;
        }

        // プレイヤー進行データのロード
        PlayerProgress.Deck.Clear();
        PlayerProgress.Deck.AddRange(loadedData.SavedDeck);
        PlayerProgress.EvolutionStats = loadedData.EvolutionStats ?? new EvolutionStatsData();

        PlayerProgress.ViewedCardIds.Clear();
        var viewedIds = loadedData.GetViewedCardIds();
        foreach (var id in viewedIds)
        {
            PlayerProgress.ViewedCardIds.Add(id);
        }

        // ノベル進行データのロード
        NovelProgress.LoadFrom(loadedData.GetAllChoiceResults());

        // 人格ログのロード
        PersonalityLogData.LoadFrom(loadedData.PersonalityLog);

        // 新規データかどうかを判定
        var isNewData = StoryProgress.CurrentStep == 0 && StoryProgress.BattleResults.Count == 0;
        var dataType = isNewData ? "新規データ" : "既存データ";

        Debug.Log($"[GameStateRepository] {dataType}自動ロード: Step {StoryProgress.CurrentStep}");
    }

    /// <summary>
    /// 現在の状態からGameSaveDataを作成
    /// </summary>
    private GameSaveData CreateGameSaveData()
    {
        var saveData = new GameSaveData();

        // ストーリー進行データを設定
        saveData.UpdateGameProgress(StoryProgress.CurrentStep, StoryProgress.BattleResults);

        // プレイヤー進行データを設定
        saveData.UpdateDeck(PlayerProgress.Deck);
        saveData.UpdateEvolutionStats(PlayerProgress.EvolutionStats);

        // 閲覧済みカードをセーブデータに追加
        foreach (var cardId in PlayerProgress.ViewedCardIds)
        {
            saveData.RecordCardView(cardId);
        }

        // ノベル選択結果をセーブデータに追加
        foreach (var choiceResult in NovelProgress.GetAllChoiceResults())
        {
            saveData.AddNovelChoiceResult(choiceResult);
        }

        // 人格ログデータを設定
        saveData.UpdatePersonalityLog(PersonalityLogData);

        return saveData;
    }

    /// <summary>
    /// 有効なセーブデータが存在するかチェック
    /// </summary>
    public bool HasSaveData()
    {
        return _saveDataManager.SaveFileExists() &&
               (StoryProgress.CurrentStep > 0 || StoryProgress.BattleResults.Count > 0);
    }

    /// <summary>
    /// 全データをリセット
    /// </summary>
    public void ResetAll()
    {
        StoryProgress.Reset();
        PlayerProgress.Reset();
        NovelProgress.Reset();
        PersonalityLogData.Reset();

        SaveAll();

        Debug.Log("[GameStateRepository] 全データを初期状態にリセット完了");
    }

    /// <summary>
    /// CardModelsからデッキを更新
    /// </summary>
    public void UpdateDeckFromCardModels(System.Collections.Generic.IReadOnlyList<CardModel> cardModels)
    {
        PlayerProgress.Deck.Clear();
        foreach (var cardModel in cardModels)
        {
            if (cardModel?.Data)
            {
                PlayerProgress.Deck.Add(new SavedCard(
                    cardModel.Data.CardId,
                    cardModel.InstanceId,
                    cardModel.IsCollapsed
                ));
            }
        }
    }

    /// <summary>
    /// デッキをCardModelsに変換
    /// </summary>
    public System.Collections.Generic.List<CardModel> ConvertDeckToCardModels()
    {
        var cardModels = new System.Collections.Generic.List<CardModel>();
        foreach (var savedCard in PlayerProgress.Deck)
        {
            var cardData = _cardPoolService.GetCardById(savedCard.cardId);
            if (cardData)
            {
                var cardModel = new CardModel(cardData, savedCard.instanceId, savedCard.isCollapsed);
                cardModels.Add(cardModel);
            }
            else
            {
                Debug.LogWarning($"[GameStateRepository] カードID '{savedCard.cardId}' が見つかりませんでした");
            }
        }
        return cardModels;
    }
}
