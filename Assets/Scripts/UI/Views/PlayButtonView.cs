using UnityEngine;
using UnityEngine.UI;
using R3;

/// <summary>
/// プレイボタン管理を担当するViewクラス
/// </summary>
public class PlayButtonView : MonoBehaviour
{
    [SerializeField] private Button playButton;

    public Observable<Unit> PlayButtonClicked => playButton.OnClickAsObservable();

    /// <summary>
    /// ボタンの有効/無効を設定
    /// </summary>
    public void SetInteractable(bool interactable) => playButton.interactable = interactable;

    /// <summary>
    /// ボタンクリックをシミュレート（InputSystemアクション用）
    /// </summary>
    public void SimulateClick() => playButton.onClick.Invoke();

    private void Awake()
    {
        SetInteractable(false); // 初期状態では無効
    }
}