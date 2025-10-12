using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.PersonalityLog;
using R3;

/// <summary>
/// ゲーム全体の進行度を管理するサービス（ファサード）
/// GameStateRepositoryに委譲し、ビジネスロジックを提供
/// </summary>
public class GameProgressService
{
    private readonly GameStateRepository _repository;

    /// <summary>
    /// データセーブ時のイベント
    /// </summary>
    public Observable<Unit> OnDataSaved => _repository.OnDataSaved;

    public GameProgressService(SaveDataManager saveDataManager, CardPoolService cardPoolService)
    {
        _repository = new GameStateRepository(saveDataManager, cardPoolService);

        // 現在のノードを初期化
        _repository.StoryProgress.CurrentNode = GetNextNode();
    }

    /// <summary>
    /// 有効なセーブデータが存在するかチェック（ストーリー進行ベース）
    /// </summary>
    public bool HasSaveData()
    {
        return _repository.HasSaveData();
    }

    /// <summary>
    /// 全データを初期状態にリセット（デバッグ用）
    /// </summary>
    public void ResetToDefaultData()
    {
        _repository.ResetAll();
        _repository.StoryProgress.CurrentNode = GetNextNode();
    }

    /// <summary>
    /// 現在のストーリーノードを取得
    /// </summary>
    public StoryNode GetCurrentNode() => _repository.StoryProgress.CurrentNode;

    /// <summary>
    /// 次に発生するストーリーノードを決定（結果辞書による分岐対応）
    /// </summary>
    public StoryNode GetNextNode()
    {
        StoryNode nextNode;
        var currentStep = _repository.StoryProgress.CurrentStep;

        switch (currentStep)
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

    public SceneType GetCurrentSceneType() => GetSceneTypeForNode(_repository.StoryProgress.CurrentNode);
    public SceneType GetNextSceneType() => GetSceneTypeForNode(GetNextNode());

    /// <summary>
    /// StoryNodeから対応するSceneTypeを取得
    /// </summary>
    private SceneType GetSceneTypeForNode(StoryNode node)
    {
        return node switch
        {
            BattleNode => SceneType.Battle,
            NovelNode => SceneType.Novel,
            EndingNode => SceneType.Home,
            _ => SceneType.Home
        };
    }

    /// <summary>
    /// 現在のバトル結果を記録
    /// </summary>
    public void RecordBattleResultAndSave(bool isPlayerWin)
    {
        var nodeId = _repository.StoryProgress.CurrentNode.NodeId;
        _repository.StoryProgress.RecordBattleResult(nodeId, isPlayerWin);
        _repository.StoryProgress.AdvanceStep();
        _repository.StoryProgress.CurrentNode = GetNextNode();
        _repository.SaveAll();
    }

    /// <summary>
    /// ノベル完了（複数選択記録 + 進行 + セーブを統合）
    /// </summary>
    public void RecordNovelResultAndSave(Dictionary<string, string> choices)
    {
        _repository.StoryProgress.AdvanceStep();
        _repository.StoryProgress.CurrentNode = GetNextNode();
        _repository.SaveAll();
    }

    /// <summary>
    /// ノベル選択結果を記録してセーブ
    /// </summary>
    public void RecordNovelChoiceAndSave(NovelChoiceResult choiceResult)
    {
        _repository.NovelProgress.RecordChoice(choiceResult);
        Debug.Log($"[GameProgressService] 選択結果を記録: {choiceResult.ScenarioId} - Choice{choiceResult.ChoiceIndex}: {choiceResult.SelectedOptionIndex}");
        _repository.SaveAll();
    }

    /// <summary>
    /// デッキ情報を更新（CardModelから変換）
    /// </summary>
    public void UpdateDeckFromCardModels(IReadOnlyList<CardModel> cardModels)
    {
        _repository.UpdateDeckFromCardModels(cardModels);
    }

    /// <summary>
    /// CardIdのリストからCardModelのリストに変換
    /// </summary>
    public List<CardModel> ConvertDeckToCardModels()
    {
        return _repository.ConvertDeckToCardModels();
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
    /// カード閲覧をリストで記録
    /// </summary>
    public void RecordCardViews(List<CardData> cardDataList)
    {
        foreach (var cardData in cardDataList)
            RecordCardView(cardData);
    }

    /// <summary>
    /// カード閲覧を記録
    /// </summary>
    public void RecordCardView(CardData cardData)
    {
        if (!cardData || string.IsNullOrEmpty(cardData.CardId)) return;
        _repository.PlayerProgress.RecordCardView(cardData.CardId);
    }

    /// <summary>
    /// 閲覧済みカードIDリストを取得
    /// </summary>
    public HashSet<string> GetViewedCardIds()
    {
        return new HashSet<string>(_repository.PlayerProgress.ViewedCardIds);
    }

    /// <summary>
    /// 特定のシナリオの選択結果を取得
    /// </summary>
    public List<NovelChoiceResult> GetChoiceResultsByScenario(string scenarioId)
    {
        return _repository.NovelProgress.GetChoiceResultsByScenario(scenarioId);
    }

    /// <summary>
    /// ゲーム結果を記録（プレイヤー分）
    /// </summary>
    public void RecordPlayerGameResult(bool playerWon, PlayerMove playerMove, bool playerCollapsed)
    {
        _repository.PlayerProgress.EvolutionStats.RecordGameResult(playerWon, playerMove, playerCollapsed);
    }

    /// <summary>
    /// カード進化チェック（プレイヤー分）
    /// </summary>
    public CardData CheckPlayerCardEvolution(CardData card)
    {
        if (_repository.PlayerProgress.EvolutionStats.CheckAllEvolutionConditions(card))
        {
            return card.EvolutionTarget;
        }

        if (_repository.PlayerProgress.EvolutionStats.CheckAllDegradationConditions(card))
        {
            return card.DegradationTarget;
        }

        return card;
    }

    /// <summary>
    /// 人格ログデータを更新（PersonalityLogServiceから取得してセーブ用に保持）
    /// </summary>
    public void UpdatePersonalityLogData(PersonalityLogData personalityLogData)
    {
        _repository.PersonalityLogData.LoadFrom(personalityLogData);
    }

    /// <summary>
    /// 人格ログデータを取得（PersonalityLogServiceの初期化用）
    /// </summary>
    public PersonalityLogData GetPersonalityLogData()
    {
        return _repository.PersonalityLogData;
    }

    /// <summary>
    /// 新しいカードをプレイヤーのデッキに追加してセーブ
    /// </summary>
    /// <param name="cardData">追加するカードデータ</param>
    public void AddCardToDeckAndSave(CardData cardData)
    {
        // 新しいインスタンスIDを生成
        var instanceId = System.Guid.NewGuid().ToString();
        
        // 新規獲得カードは崩壊していない状態で追加
        const bool isCollapsed = false;
        
        // SavedCardとしてデッキに追加
        var savedCard = new SavedCard(cardData.CardId, instanceId, isCollapsed);
        _repository.PlayerProgress.Deck.Add(savedCard);
        
        Debug.Log($"[GameProgressService] カードをデッキに追加: {cardData.CardName} (ID: {cardData.CardId})");
        
        // セーブデータを更新
        _repository.SaveAll();
    }
}
