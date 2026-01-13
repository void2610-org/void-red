using System.Collections.Generic;

/// <summary>
/// プレイヤー進行データを保持するクラス
/// 閲覧済みカードを管理
/// </summary>
public class PlayerProgressData
{
    /// <summary>
    /// 閲覧済みカードID
    /// </summary>
    public HashSet<string> ViewedCardIds { get; set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public PlayerProgressData()
    {
        ViewedCardIds = new HashSet<string>();
    }

    /// <summary>
    /// カード閲覧を記録
    /// </summary>
    /// <param name="cardId">カードID</param>
    public void RecordCardView(string cardId)
    {
        if (!string.IsNullOrEmpty(cardId))
        {
            ViewedCardIds.Add(cardId);
        }
    }

    /// <summary>
    /// リセット
    /// </summary>
    public void Reset()
    {
        ViewedCardIds.Clear();
    }
}
