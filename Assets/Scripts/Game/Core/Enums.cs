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
    // 1. 出品者フェーズ
    ThemeAnnouncement,      // 記憶テーマ公開
    CardDistribution,       // カード配布（主4枚 + 相4枚）
    ValueRanking,           // 価値順位設定
    CardReveal,             // カード公開

    BiddingPhase,           // 感情リソースで入札
    DialoguePhase,          // 揺さぶり・入札変動
    AuctionResult,          // 入札結果の開示・落札者決定
    RewardPhase,            // 報酬ポイント算出・感情リソース獲得
    MemoryGrowth,           // 記憶テーマ構成・キャラクター表示
    BattleEnd,              // バトル終了
}


/// <summary>
/// 感情リソースの8属性（プルチックの感情の輪に基づく）
/// </summary>
public enum EmotionType
{
    Joy,         // 喜び
    Trust,       // 信頼
    Fear,        // 恐れ
    Surprise,    // 驚き
    Sadness,     // 悲しみ
    Disgust,     // 嫌悪
    Anger,       // 怒り
    Anticipation // 期待
}

// EmotionTypeの拡張メソッド
public static class EmotionTypeExtensions
{
    // 感情タイプに対応する色を取得
    public static UnityEngine.Color GetColor(this EmotionType emotion) => emotion switch
    {
        EmotionType.Joy => new UnityEngine.Color(1f, 0.85f, 0.2f),           // 黄色（喜び）
        EmotionType.Trust => new UnityEngine.Color(0.3f, 0.75f, 0.4f),       // 緑（信頼）
        EmotionType.Fear => new UnityEngine.Color(0.2f, 0.4f, 0.2f),         // 暗緑（恐れ）
        EmotionType.Surprise => new UnityEngine.Color(0.4f, 0.8f, 0.9f),     // シアン（驚き）
        EmotionType.Sadness => new UnityEngine.Color(0.3f, 0.45f, 0.75f),    // 青（悲しみ）
        EmotionType.Disgust => new UnityEngine.Color(0.5f, 0.3f, 0.6f),      // 紫（嫌悪）
        EmotionType.Anger => new UnityEngine.Color(0.9f, 0.25f, 0.25f),      // 赤（怒り）
        EmotionType.Anticipation => new UnityEngine.Color(0.95f, 0.5f, 0.2f),// オレンジ（期待）
        _ => UnityEngine.Color.white
    };

    // 感情タイプの日本語名を取得
    public static string ToJapaneseName(this EmotionType emotion) => emotion switch
    {
        EmotionType.Joy => "喜び",
        EmotionType.Trust => "信頼",
        EmotionType.Fear => "恐れ",
        EmotionType.Surprise => "驚き",
        EmotionType.Sadness => "悲しみ",
        EmotionType.Disgust => "嫌悪",
        EmotionType.Anger => "怒り",
        EmotionType.Anticipation => "期待",
        _ => "不明"
    };
}
