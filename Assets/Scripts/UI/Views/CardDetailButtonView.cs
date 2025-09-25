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
    public void Show() => detailButton.gameObject.SetActive(true);
    public void Hide() => detailButton.gameObject.SetActive(false);
    
    private void Awake()
    {
        Hide(); // 初期状態では非表示
    }
    
    // OnDestroy is no longer needed since there are no listeners or subjects to clean up
}