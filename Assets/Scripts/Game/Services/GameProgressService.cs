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
    
    public GameProgressService()
    {
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
        // 結果辞書を使用したストーリー分岐ロジック
        return _currentStep switch
        {
            // 導入ノベル
            0 => new NovelNode("intro_001"),
            // 最初のバトル
            1 => new BattleNode("enemy_001"),
            // 最初のバトル結果に応じた分岐
            2 => GetResult("1") == "win" 
                ? new NovelNode("first_victory") 
                : new NovelNode("first_defeat"),
            // 2番目のバトル
            3 => new BattleNode("enemy_002"),
            // 2番目のバトル結果に応じた分岐
            4 => GetResult("3") == "win"
                ? new NovelNode("second_victory")
                : new NovelNode("second_defeat"),
            // 最終バトル
            5 => new BattleNode("enemy_003"),
            // ゲーム終了
            _ => new EndingNode()
        };
    }
    
    /// <summary>
    /// ストーリーを次のステップに進行
    /// </summary>
    public void AdvanceStory()
    {
        _currentStep++;
        Debug.Log($"[GameProgressService] ストーリー進行: Step {_currentStep}");
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
    public void RecordCurrentBattleResult(bool isPlayerWin)
    {
        _results[_currentStep.ToString()] = isPlayerWin ? "win" : "lose";
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