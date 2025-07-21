using System.Collections.Generic;

/// <summary>
/// カードのナレーションタイプ
/// </summary>
public enum NarrationType
{
    PrePlay,        // プレイ前語り
    PostBattle,     // 勝負後語り
    PostBattleEnemy // 勝負後語り（敵）
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