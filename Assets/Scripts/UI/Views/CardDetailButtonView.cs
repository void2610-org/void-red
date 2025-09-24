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

    public Observable<Unit> DetailButtonClicked => _detailButtonClicked;
    public void Show() => detailButton.gameObject.SetActive(true);
    public void Hide() => detailButton.gameObject.SetActive(false);
    
    private void OnDetailButtonClicked() => _detailButtonClicked.OnNext(Unit.Default);
    private readonly Subject<Unit> _detailButtonClicked = new();
    
    private void Awake()
    {
        detailButton.onClick.AddListener(OnDetailButtonClicked);
    }
    
    private void Start()
    {
        Hide(); // 初期状態では非表示
    }
    
    private void OnDestroy()
    {
        _detailButtonClicked?.Dispose();
        detailButton.onClick.RemoveListener(OnDetailButtonClicked);
    }
}