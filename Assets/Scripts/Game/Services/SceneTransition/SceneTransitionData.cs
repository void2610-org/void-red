using System;
using UnityEngine;

/// <summary>
/// シーン遷移時に受け渡すデータの基底クラス
/// 各シーン固有のデータはこのクラスを継承して実装する
/// </summary>
[Serializable]
public abstract class SceneTransitionData
{
    /// <summary>
    /// 遷移先のシーンタイプ
    /// 継承クラスで固有の値を返す
    /// </summary>
    public abstract SceneType TargetScene { get; }
    
    /// <summary>
    /// 戻り先のシーンタイプ
    /// データ処理完了後に戻るシーンを指定する
    /// </summary>
    public SceneType ReturnScene { get; set; } = SceneType.Home;
    
    /// <summary>
    /// デバッグ用の文字列表現を取得
    /// </summary>
    /// <returns>データ情報の文字列</returns>
    public virtual string GetDebugInfo()
    {
        return $"Target: {TargetScene}, Return: {ReturnScene}";
    }
}

/// <summary>
/// バトルシーン遷移用のデータクラス
/// </summary>
[Serializable]
public class BattleTransitionData : SceneTransitionData
{
    /// <summary>
    /// 遷移先はバトルシーンで固定
    /// </summary>
    public override SceneType TargetScene => SceneType.Battle;
    
    /// <summary>対戦する敵のデータ</summary>
    public EnemyData TargetEnemy { get; set; }
    
    /// <summary>
    /// デフォルトコンストラクタ
    /// </summary>
    public BattleTransitionData()
    {
    }
    
    /// <summary>
    /// 敵データを指定するコンストラクタ
    /// </summary>
    /// <param name="enemy">対戦相手の敵データ</param>
    /// <param name="returnScene">戻り先シーン</param>
    public BattleTransitionData(EnemyData enemy, SceneType returnScene = SceneType.Home)
    {
        TargetEnemy = enemy;
        ReturnScene = returnScene;
    }
    
    /// <summary>
    /// デバッグ情報を取得
    /// </summary>
    /// <returns>バトルデータの詳細情報</returns>
    public override string GetDebugInfo()
    {
        var enemyName = TargetEnemy ? TargetEnemy.EnemyName : "None";
        return $"{base.GetDebugInfo()}, Enemy: {enemyName}";
    }
}

/// <summary>
/// ノベルシーン遷移用のデータクラス
/// シナリオIDのみの最小構成
/// </summary>
[Serializable]
public class NovelTransitionData : SceneTransitionData
{
    /// <summary>
    /// 遷移先はノベルシーンで固定
    /// </summary>
    public override SceneType TargetScene => SceneType.Novel;
    
    /// <summary>実行するシナリオのID</summary>
    public string ScenarioId { get; set; } = "";
    
    /// <summary>
    /// デフォルトコンストラクタ
    /// </summary>
    public NovelTransitionData()
    {
    }
    
    /// <summary>
    /// シナリオIDを指定するコンストラクタ
    /// </summary>
    /// <param name="scenarioId">シナリオID</param>
    /// <param name="returnScene">戻り先シーン</param>
    public NovelTransitionData(string scenarioId, SceneType returnScene = SceneType.Home)
    {
        ScenarioId = scenarioId;
        ReturnScene = returnScene;
    }
    
    /// <summary>
    /// デバッグ情報を取得
    /// </summary>
    /// <returns>ノベルデータの詳細情報</returns>
    public override string GetDebugInfo()
    {
        return $"{base.GetDebugInfo()}, Scenario: {ScenarioId}";
    }
}

