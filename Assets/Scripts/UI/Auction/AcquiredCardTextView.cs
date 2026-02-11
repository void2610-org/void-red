using TMPro;
using UnityEngine;

/// <summary>
/// 獲得カード一覧の個別アイテム表示
/// </summary>
public class AcquiredCardTextView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cardNameText;

    public void Initialize(CardData cardData) => cardNameText.text = cardData.CardName;
}
