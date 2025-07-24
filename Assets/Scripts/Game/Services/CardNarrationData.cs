using System.Collections.Generic;

/// <summary>
/// カードのナレーションタイプ
/// </summary>
public enum NarrationType
{
    PrePlay,        // プレイ前語り
    PostBattleWin,  // 勝負後語り（勝利）
    PostBattleLose, // 勝負後語り（敗北）
    PostBattleWinEnemy, // 勝負後語り（敵勝利）
    PostBattleLoseEnemy, // 勝負後語り（敵敗北）
}

/// <summary>
/// 単一のカードの全ナレーションデータを管理するクラス
/// </summary>
public class CardNarrationData
{
    // ナレーションタイプとPlayStyleをキーとしたナレーションデータ
    private readonly Dictionary<NarrationType, Dictionary<PlayStyle, string>> _narrations = new();

    /// <summary>
    /// ナレーションを設定
    /// </summary>
    public void SetNarration(NarrationType type, PlayStyle playStyle, string narration)
    {
        if (!_narrations.ContainsKey(type))
        {
            _narrations[type] = new Dictionary<PlayStyle, string>();
        }
        _narrations[type][playStyle] = narration;
    }

    /// <summary>
    /// ナレーションを取得
    /// </summary>
    public string GetNarration(NarrationType type, PlayStyle playStyle)
    {
        if (_narrations.TryGetValue(type, out var styleNarrations))
        {
            if (styleNarrations.TryGetValue(playStyle, out var narration))
            {
                return narration;
            }
        }
        return string.Empty;
    }
}