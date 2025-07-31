using UnityEngine;
using UnityEngine.UI;
using R3;

/// <summary>
/// 人格ログを開くボタンのViewコンポーネント
/// </summary>
[RequireComponent(typeof(Button))]
public class PersonalityLogButtonView : MonoBehaviour
{
    private Button _logButton;
    
    private readonly Subject<Unit> _onButtonClicked = new();
    
    /// <summary>
    /// ボタンクリックイベント
    /// </summary>
    public Observable<Unit> OnButtonClicked => _onButtonClicked;
    
    /// <summary>
    /// 初期化
    /// </summary>
    private void Awake()
    {
        _logButton = this.GetComponent<Button>();
        _logButton.onClick.AddListener(() => _onButtonClicked.OnNext(Unit.Default));
    }
    
    /// <summary>
    /// ボタンを表示
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// ボタンを非表示
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// ボタンの有効/無効を設定
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        _logButton.interactable = interactable;
    }
    
    /// <summary>
    /// リソース解放
    /// </summary>
    private void OnDestroy()
    {
        _onButtonClicked?.Dispose();
    }
}