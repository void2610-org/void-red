/// <summary>
/// ストーリー進行における分岐ノードを表す抽象基底クラス
/// </summary>
public abstract class StoryNode
{
    public string NodeId { get; protected set; }
    
    /// <summary>
    /// このノード終了後にホームに戻るかどうか
    /// true: ホームに戻る、false: 次のノードへ直接遷移
    /// </summary>
    public bool ReturnToHome { get; set; } = true;
}

/// <summary>
/// バトルイベントを表すノード
/// </summary>
public class BattleNode : StoryNode
{
    public string EnemyId { get; }
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="enemyId">敵ID</param>
    /// <param name="returnToHome">このノード終了後にホームに戻るか（デフォルト: true）</param>
    public BattleNode(string enemyId, bool returnToHome = true)
    {
        NodeId = enemyId;
        EnemyId = enemyId;
        ReturnToHome = returnToHome;
    }
}

/// <summary>
/// ノベルパートを表すノード
/// </summary>
public class NovelNode : StoryNode
{
    /// <summary>
    /// シナリオID
    /// </summary>
    public string ScenarioId { get; }
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="scenarioId">シナリオID</param>
    /// <param name="returnToHome">このノード終了後にホームに戻るか（デフォルト: true）</param>
    public NovelNode(string scenarioId, bool returnToHome = true)
    {
        NodeId = scenarioId;
        ScenarioId = scenarioId;
        ReturnToHome = returnToHome;
    }
}