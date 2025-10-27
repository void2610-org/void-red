using UnityEngine;

public enum BgmType
{
    Title, // タイトルBGM
    Home,  // ホームBGM
    Novel, // ノベルBGM
    Battle, // バトルBGM
}

/// <summary>
/// ゲームの状態を表すEnum
/// </summary>
public enum GameState
{
    ThemeAnnouncement,  // お題発表
    PlayerCardSelection, // プレイヤーカード選択
    EnemyCardSelection,  // 敵カード選択
    Evaluation,         // 評価
    ResultDisplay,      // 勝敗表示
    BattleEnd,         // バトル終了（3勝達成）
    GameOver           // ゲームオーバー
}

/// <summary>
/// カードの出し方を表すenum
/// </summary>
public enum PlayStyle
{
    Hesitation, // 迷い
    Impulse, // 衝動
    Conviction // 確信
}

/// <summary>
/// PlayStyleに関する拡張メソッド
/// </summary>
public static class PlayStyleExtensions
{
    /// <summary>
    /// PlayStyleを日本語の文字列に変換
    /// </summary>
    public static string ToJapaneseString(this PlayStyle playStyle)
    {
        return playStyle switch
        {
            PlayStyle.Hesitation => "迷い",
            PlayStyle.Impulse => "衝動",
            PlayStyle.Conviction => "確信",
            _ => "不明"
        };
    }
    
    /// <summary>
    /// PlayStyleの説明を取得
    /// </summary>
    public static string GetDescription(this PlayStyle playStyle)
    {
        return playStyle switch
        {
            PlayStyle.Hesitation => "慎重に、しかし迷いながら",
            PlayStyle.Impulse => "感情のまま、衝動的に",
            PlayStyle.Conviction => "強い信念を持って、確信的に",
            _ => ""
        };
    }
    
    /// <summary>
    /// PlayStyleのスコア倍率を取得
    /// </summary>
    public static float GetScoreMultiplier(this PlayStyle playStyle)
    {
        return playStyle switch
        {
            PlayStyle.Hesitation => 0.8f,  // 迷い：スコア低減、崩壊率低
            PlayStyle.Impulse => 1.0f,     // 衝動：標準
            PlayStyle.Conviction => 1.3f,  // 確信：スコア増加、崩壊率高
            _ => 1.0f
        };
    }
    
    /// <summary>
    /// PlayStyleの崩壊率倍率を取得
    /// </summary>
    public static float GetCollapseMultiplier(this PlayStyle playStyle)
    {
        return playStyle switch
        {
            PlayStyle.Hesitation => 0.5f,  // 迷い：崩壊率低
            PlayStyle.Impulse => 1.0f,     // 衝動：標準
            PlayStyle.Conviction => 1.8f,  // 確信：崩壊率高
            _ => 1.0f
        };
    }

    /// <summary>
    /// 相手のPlayStyleに対して有利かどうかを判定
    /// じゃんけんの三すくみ関係: Hesitation→Conviction→Impulse→Hesitation
    /// </summary>
    public static bool IsStrongAgainst(this PlayStyle playStyle, PlayStyle opponent)
    {
        return playStyle switch
        {
            PlayStyle.Conviction => opponent == PlayStyle.Hesitation,    // 確信 は 迷い に勝つ
            PlayStyle.Hesitation => opponent == PlayStyle.Impulse, // 迷い は 衝動 に勝つ
            PlayStyle.Impulse => opponent == PlayStyle.Conviction,  // 衝動 は 確信 に勝つ
            _ => false
        };
    }

    /// <summary>
    /// 相手のPlayStyleとの相性によるスコア倍率を取得
    /// </summary>
    /// <param name="playStyle">自分のPlayStyle</param>
    /// <param name="opponent">相手のPlayStyle</param>
    /// <returns>有利: 1.2倍, 不利: 0.9倍, 同じ/引き分け: 1.0倍</returns>
    public static float GetAdvantageMultiplier(this PlayStyle playStyle, PlayStyle opponent)
    {
        if (playStyle == opponent)
            return 1.0f;  // 同じPlayStyle: ボーナスなし

        if (playStyle.IsStrongAgainst(opponent))
            return 1.2f;  // 有利: +20%ボーナス

        return 0.9f;  // 不利: -10%ペナルティ
    }
}