using Coffee.UIEffects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CardView と DeckCardView の共通基底クラス
/// カードの基本表示ロジックを提供
/// </summary>
public abstract class BaseCardView : MonoBehaviour
{
    // 抽象プロパティ：各サブクラスで実装
    protected abstract Image CardImage { get; }
    protected abstract TextMeshProUGUI CardNameText { get; }
    protected abstract Image CardFrame { get; }
    protected abstract UIEffect EdgeUIEffect { get; }

    // CardData取得メソッド（各サブクラスで実装）
    protected abstract CardData GetCardData();

    /// <summary>
    /// カードの基本表示を更新（画像、名前、バナー、フレーム）
    /// 状態に応じた色変更を適用
    /// </summary>
    protected void UpdateCardDisplay(CardDisplayState displayState)
    {
        var cardData = GetCardData();
        if (!cardData) return;

        // カード画像と名前を設定
        CardImage.sprite = cardData.CardImage;
        CardNameText.text = cardData.CardName;

        switch (displayState)
        {
            case CardDisplayState.Normal:
                // 通常表示
                CardImage.color = cardData.CardImage ? Color.white : Color.clear;
                CardFrame.color = cardData.Color;
                CardNameText.color = cardData.IsTextColorBlack ? Color.black : Color.white;
                break;

            case CardDisplayState.Veiled:
                // 隠されている表示：黒く暗い表示
                CardImage.color = cardData.CardImage ? new Color(0.2f, 0.2f, 0.2f, 1f) : Color.clear;
                CardFrame.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                CardNameText.color = Color.black;
                CardNameText.text = "???";
                break;

            case CardDisplayState.Collapsed:
                // 崩壊表示：グレーアウト, UIEffectを有効化
                CardImage.color = cardData.CardImage ? new Color(0.5f, 0.5f, 0.5f, 0.7f) : Color.clear;
                CardFrame.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
                CardNameText.color = cardData.IsTextColorBlack ? new Color(0.2f, 0.2f, 0.2f, 1f) : new Color(0.7f, 0.7f, 0.7f, 1f);
                break;

            case CardDisplayState.Backside:
                // 裏面表示：全て非表示（CardViewで使用）
                CardFrame.color = Color.clear;
                CardImage.color = Color.clear;
                CardNameText.text = string.Empty;
                break;
        }
    }
}
