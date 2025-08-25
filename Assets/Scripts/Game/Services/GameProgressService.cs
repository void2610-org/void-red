using UnityEngine;

/// <summary>
/// ゲーム全体の進行度を管理し、次のイベントを決定するサービス
/// ストーリーの分岐ロジックを内部に含む
/// </summary>
public class GameProgressService
{
    /// <summary>
    /// 現在のストーリーステップ（簡易版）
    /// </summary>
    private int _currentStep = 0;
    
    /// <summary>
    /// コンストラクタ（依存性注入）
    /// </summary>
    public GameProgressService()
    {
        Debug.Log("[GameProgressService] 初期化完了（簡易版）");
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
    /// 次に発生するストーリーノードを決定
    /// </summary>
    /// <returns>次のストーリーノード</returns>
    public StoryNode GetNextNode()
    {
        Debug.Log($"[GameProgressService] 次のノード取得: Step {_currentStep}");
        
        // 固定進行ロジック（テスト用の簡単な実装）
        return _currentStep switch
        {
            // 導入ノベル
            0 => new NovelNode("intro_001"),
            // 最初のバトル
            1 => new BattleNode("enemy_001"),
            // 最初のバトル後ノベル
            2 => new NovelNode("first_victory"),
            // 2番目のバトル
            3 => new BattleNode("enemy_002"),
            // 2番目のバトル後ノベル
            4 => new NovelNode("second_victory"),
            // 最終バトル
            5 => new BattleNode("enemy_003"),
            // エンディング
            6 => new NovelNode("perfect_ending"),
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
    /// ストーリーをリセット
    /// </summary>
    public void ResetStory()
    {
        _currentStep = 0;
        Debug.Log("[GameProgressService] ストーリーリセット");
    }
}