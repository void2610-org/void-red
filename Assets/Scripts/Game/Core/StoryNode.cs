/// <summary>
/// ストーリー進行における分岐ノードを表す抽象基底クラス
/// </summary>
public abstract class StoryNode
{
    public string NodeId { get; protected set; }
    public bool ReturnToHome { get; protected set; } = true;
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
    /// <param name="returnToHome">ホームに戻るかどうか</param>
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
    /// <param name="returnToHome">ホームに戻るかどうか</param>
    public NovelNode(string scenarioId, bool returnToHome = true)
    {
        NodeId = scenarioId;
        ScenarioId = scenarioId;
        ReturnToHome = returnToHome;
    }
}

/// <summary>
/// ゲーム終了を表すノード
/// </summary>
public class EndingNode : StoryNode
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public EndingNode()
    {
        NodeId = "ending";
    }
}