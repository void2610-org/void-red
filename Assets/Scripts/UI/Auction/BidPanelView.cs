using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 入札パネルView
// カード選択時に表示される入札額調整UI
public class BidPanelView : MonoBehaviour
{
    [Header("入札操作")]
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button confirmButton;

    public Observable<Unit> OnIncrease => increaseButton.OnClickAsObservable();
    public Observable<Unit> OnDecrease => decreaseButton.OnClickAsObservable();
    public Observable<Unit> OnConfirm => confirmButton.OnClickAsObservable();
}
