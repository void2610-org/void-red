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
