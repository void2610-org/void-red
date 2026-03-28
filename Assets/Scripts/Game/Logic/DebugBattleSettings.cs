#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// デバッグ用バトル設定（エディタ専用）
/// メニュー: Debug/Battle/ からオークションスキップのON/OFFを切り替え
/// </summary>
public static class DebugBattleSettings
{
    /// <summary>オークションをスキップしてバトルから開始するか</summary>
    public static bool SkipAuction
    {
        get => EditorPrefs.GetBool(SKIP_AUCTION_KEY, false);
        set => EditorPrefs.SetBool(SKIP_AUCTION_KEY, value);
    }

    /// <summary>デバッグ時のプレイヤースキル</summary>
    public static EmotionType PlayerSkill
    {
        get => (EmotionType)EditorPrefs.GetInt(PLAYER_SKILL_KEY, (int)EmotionType.Joy);
        set => EditorPrefs.SetInt(PLAYER_SKILL_KEY, (int)value);
    }

    /// <summary>デバッグ時の勝利条件</summary>
    public static VictoryCondition VictoryCondition
    {
        get => (VictoryCondition)EditorPrefs.GetInt(VICTORY_CONDITION_KEY, (int)VictoryCondition.HigherWins);
        set => EditorPrefs.SetInt(VICTORY_CONDITION_KEY, (int)value);
    }
    private const string SKIP_AUCTION_KEY = "DebugBattleSettings.SkipAuction";
    private const string PLAYER_SKILL_KEY = "DebugBattleSettings.PlayerSkill";
    private const string VICTORY_CONDITION_KEY = "DebugBattleSettings.VictoryCondition";

    [MenuItem("Debug/Battle/スキル設定/Anger（条件反転）")]
    private static void SetSkillAnger()
    {
        SetSkill(EmotionType.Anger);
    }

    [MenuItem("Debug/Battle/スキル設定/Surprise（ランダム変更）")]
    private static void SetSkillSurprise()
    {
        SetSkill(EmotionType.Surprise);
    }

    [MenuItem("Debug/Battle/オークションスキップ OFF", true)]
    private static bool DisableSkipAuctionValidate()
    {
        return SkipAuction;
    }

    [MenuItem("Debug/Battle/スキル設定/Trust（カード再使用）")]
    private static void SetSkillTrust()
    {
        SetSkill(EmotionType.Trust);
    }

    [MenuItem("Debug/Battle/スキル設定/Sadness（数字を3に）")]
    private static void SetSkillSadness()
    {
        SetSkill(EmotionType.Sadness);
    }

    [MenuItem("Debug/Battle/オークションスキップ ON", true)]
    private static bool EnableSkipAuctionValidate()
    {
        return !SkipAuction;
    }

    [MenuItem("Debug/Battle/スキル設定/Joy（2倍）")]
    private static void SetSkillJoy()
    {
        SetSkill(EmotionType.Joy);
    }

    [MenuItem("Debug/Battle/スキル設定/Anticipation（残り全ランダム）")]
    private static void SetSkillAnticipation()
    {
        SetSkill(EmotionType.Anticipation);
    }

    [MenuItem("Debug/Battle/スキル設定/Fear（数字入れ替え）")]
    private static void SetSkillFear()
    {
        SetSkill(EmotionType.Fear);
    }

    [MenuItem("Debug/Battle/スキル設定/Disgust（半減）")]
    private static void SetSkillDisgust()
    {
        SetSkill(EmotionType.Disgust);
    }

    [MenuItem("Debug/Battle/オークションスキップ ON")]
    private static void EnableSkipAuction()
    {
        SkipAuction = true;
        Debug.Log("[Debug] オークションスキップ: ON");
    }

    [MenuItem("Debug/Battle/オークションスキップ OFF")]
    private static void DisableSkipAuction()
    {
        SkipAuction = false;
        Debug.Log("[Debug] オークションスキップ: OFF");
    }

    [MenuItem("Debug/Battle/勝利条件/HigherWins（大きい数字が勝ち）")]
    private static void SetVictoryHigher()
    {
        VictoryCondition = VictoryCondition.HigherWins;
        Debug.Log("[Debug] 勝利条件: HigherWins");
    }

    [MenuItem("Debug/Battle/勝利条件/LowerWins（小さい数字が勝ち）")]
    private static void SetVictoryLower()
    {
        VictoryCondition = VictoryCondition.LowerWins;
        Debug.Log("[Debug] 勝利条件: LowerWins");
    }

    private static void SetSkill(EmotionType skill)
    {
        PlayerSkill = skill;
        Debug.Log($"[Debug] プレイヤースキル: {skill}");
    }
}
#endif
