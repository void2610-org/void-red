using R3;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 人格ログを開くボタンのViewコンポーネント
/// </summary>
[RequireComponent(typeof(Button))]
public class PersonalityLogButtonView : MonoBehaviour
{
    public Observable<Unit> OnButtonClicked { get; private set; }
    private Button _logButton;

    private void Awake()
    {
        _logButton = this.GetComponent<Button>();
        OnButtonClicked = _logButton.OnClickAsObservable();
    }
}
