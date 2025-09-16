using UnityEngine;
using UnityEngine.UI;
using R3;

/// <summary>
/// ポーズボタンのViewコンポーネント
/// </summary>
[RequireComponent(typeof(Button))]
public class PauseButtonView : MonoBehaviour
{
    public Observable<Unit> OnButtonClicked => _onButtonClicked;
    
    private Button _button;
    private readonly Subject<Unit> _onButtonClicked = new();
    
    private void Awake()
    {
        _button = this.GetComponent<Button>();
        _button.onClick.AddListener(() => _onButtonClicked.OnNext(Unit.Default));
    }
    
    private void OnDestroy()
    {
        _onButtonClicked?.Dispose();
    }
}