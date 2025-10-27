using UnityEngine;
using UnityEngine.UI;
using R3;

/// <summary>
/// カード詳細ボタン管理を担当するViewクラス
/// プレイボタンの上に表示され、カード選択時のみ表示される
/// </summary>
public class CardDetailButtonView : MonoBehaviour
{
    [SerializeField] private Button detailButton;

    public Observable<Unit> DetailButtonClicked => detailButton.OnClickAsObservable();

    /// <summary>
    /// ボタンの有効/無効を設定
    /// </summary>
    public void SetInteractable(bool interactable) => detailButton.interactable = interactable;

    private void Awake()
    {
        SetInteractable(false); // 初期状態では無効
    }
}