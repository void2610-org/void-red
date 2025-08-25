/// <summary>
/// ストーリー進行における分岐ノードを表す抽象基底クラス
/// </summary>
public abstract class StoryNode
{
}

/// <summary>
/// バトルイベントを表すノード
/// </summary>
public class BattleNode : StoryNode
{
    /// <summary>
    /// 敵キャラクターのID
    /// </summary>
    public string EnemyId { get; }
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="enemyId">敵ID</param>
    public BattleNode(string enemyId)
    {
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
    }
}