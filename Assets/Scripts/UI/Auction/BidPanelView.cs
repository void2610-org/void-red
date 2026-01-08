using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;

// 入札パネルView
// カード選択時に表示される入札額調整UI
public class BidPanelView : MonoBehaviour
{
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI remainingResourceText;

    public Observable<Unit> OnIncrease => increaseButton.OnClickAsObservable();
    public Observable<Unit> OnDecrease => decreaseButton.OnClickAsObservable();
    public Observable<Unit> OnConfirm => confirmButton.OnClickAsObservable();

    public void UpdateRemainingResource(int remaining)
    {
        remainingResourceText.text = $"残り: {remaining}";
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    private void Awake()
    {
        // 初期状態で非表示
        Hide();
    }
}
