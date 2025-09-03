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
    
    public Observable<Unit> OnButtonClicked => _onButtonClicked;
    
    private void Awake()
    {
        _logButton = this.GetComponent<Button>();
        _logButton.onClick.AddListener(() => _onButtonClicked.OnNext(Unit.Default));
    }
    
    private void OnDestroy()
    {
        _onButtonClicked?.Dispose();
    }
}