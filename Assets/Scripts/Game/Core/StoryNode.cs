/// <summary>
/// ストーリー進行における分岐ノードを表す抽象基底クラス
/// </summary>
public abstract class StoryNode
{
    public string NodeId { get; protected set; }
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
    public BattleNode(string enemyId)
    {
        NodeId = enemyId;
        EnemyId = enemyId;
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
    public NovelNode(string scenarioId)
    {
        NodeId = scenarioId;
        ScenarioId = scenarioId;
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