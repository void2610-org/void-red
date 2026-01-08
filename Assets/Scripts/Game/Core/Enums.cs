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
}