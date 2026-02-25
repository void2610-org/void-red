using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// バトル場のカードスロット表示
/// カードの数字・感情アイコン・裏表を管理する
/// </summary>
public class BattleCardSlotView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject cardFront;
    [SerializeField] private GameObject cardBack;
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private Image emotionIcon;
    [SerializeField] private Image selectionHighlight;

    /// <summary>クリックイベント</summary>
    public Observable<Unit> OnClicked => button.OnClickAsObservable();

    private BattleCardModel _card;

    /// <summary>
    /// カード情報で初期化（表面表示）
    /// </summary>
    public void Initialize(BattleCardModel card, bool showNumber = true)
    {
        _card = card;

        if (cardImage && card.Card.Data.CardImage)
            cardImage.sprite = card.Card.Data.CardImage;

        if (numberText)
            numberText.text = showNumber ? card.Number.ToString() : "";

        if (emotionIcon)
            emotionIcon.color = card.Emotion.GetColor();

        ShowFront();
        SetSelected(false);
    }

    /// <summary>数字を更新する（スキル効果後）</summary>
    public void UpdateNumber(int number)
    {
        if (numberText)
            numberText.text = number.ToString();
    }

    /// <summary>表面を表示</summary>
    public void ShowFront()
    {
        if (cardFront) cardFront.SetActive(true);
        if (cardBack) cardBack.SetActive(false);
    }

    /// <summary>裏面を表示（伏せ状態）</summary>
    public void ShowBack()
    {
        if (cardFront) cardFront.SetActive(false);
        if (cardBack) cardBack.SetActive(true);
    }

    /// <summary>選択状態を設定</summary>
    public void SetSelected(bool selected)
    {
        if (selectionHighlight)
            selectionHighlight.gameObject.SetActive(selected);
    }

    /// <summary>インタラクション可能かを設定</summary>
    public void SetInteractable(bool interactable)
    {
        if (button) button.interactable = interactable;
    }
}
