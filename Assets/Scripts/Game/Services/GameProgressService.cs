using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

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
    private readonly SaveDataManager _saveDataManager;
    private readonly GameStatsService _gameStatsService;
    private readonly PersonalityLogService _personalityLogService;
    
    public GameProgressService(SaveDataManager saveDataManager, GameStatsService gameStatsService, PersonalityLogService personalityLogService)
    {
        _saveDataManager = saveDataManager;
        _gameStatsService = gameStatsService;
        _personalityLogService = personalityLogService;
        
        // 起動時に自動でセーブデータをロード
        var loadedData = _saveDataManager.LoadGameData();
        LoadState(loadedData);
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
        var result = _results.TryGetValue(id, out var value) ? value : "";
        Debug.Log($"[GameProgressService] 結果取得: ID '{id}' → '{result}' (総Results数: {_results.Count})");
        return result;
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
        var saveData = _gameStatsService.CurrentSaveData;
        
        // 進行状態をsaveDataに同期
        saveData.UpdateGameProgress(_currentStep, _results);
        
        // 実際にファイルに保存
        _saveDataManager.SaveGameData(saveData);
        
        Debug.Log("[GameProgressService] 統合セーブ完了");
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