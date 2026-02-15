using UnityEngine;

/// <summary>
/// 獲得カード一覧の個別カード表示
/// </summary>
public class AcquiredCardView : MonoBehaviour
{
    [SerializeField] private CardView cardView;

    public void Initialize(CardData cardData)
    {
        cardView.Initialize(cardData);
        cardView.SetInteractable(false);
    }
}
