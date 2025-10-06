/// <summary>
/// カードとテーマに付与されるキーワード
/// キーワードが一致するとスコアボーナスが得られる
/// </summary>
public enum KeywordType
{
    None,

    // 感情系
    Joy,      // 喜び
    Anger,    // 怒り
    Sadness,  // 悲しみ
    Fear,     // 恐怖
    Surprise, // 驚き
    Disgust,  // 嫌悪

    // テーマ系
    Love,     // 愛
    Death,    // 死
    Dream,    // 夢
    Memory,   // 記憶
    Hope,     // 希望
    Despair,  // 絶望

    // 概念系
    Freedom,  // 自由
    Fate,     // 運命
    Truth,    // 真実
    Lie,      // 嘘
    Time,     // 時間
    Eternity, // 永遠
}

/// <summary>
/// KeywordType の拡張メソッド
/// </summary>
public static class KeywordTypeExtensions
{
    /// <summary>
    /// キーワードの日本語名を取得
    /// </summary>
    /// <param name="keyword">キーワードタイプ</param>
    /// <returns>日本語名</returns>
    public static string GetJapaneseName(this KeywordType keyword)
    {
        return keyword switch
        {
            KeywordType.None => "なし",

            // 感情系
            KeywordType.Joy => "喜び",
            KeywordType.Anger => "怒り",
            KeywordType.Sadness => "悲しみ",
            KeywordType.Fear => "恐怖",
            KeywordType.Surprise => "驚き",
            KeywordType.Disgust => "嫌悪",

            // テーマ系
            KeywordType.Love => "愛",
            KeywordType.Death => "死",
            KeywordType.Dream => "夢",
            KeywordType.Memory => "記憶",
            KeywordType.Hope => "希望",
            KeywordType.Despair => "絶望",

            // 概念系
            KeywordType.Freedom => "自由",
            KeywordType.Fate => "運命",
            KeywordType.Truth => "真実",
            KeywordType.Lie => "嘘",
            KeywordType.Time => "時間",
            KeywordType.Eternity => "永遠",

            _ => keyword.ToString()
        };
    }
}
