using R3;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ヘルプボタンのViewコンポーネント
/// </summary>
[RequireComponent(typeof(Button))]
public class HelpButtonView : MonoBehaviour
{
    public Observable<Unit> OnButtonClicked { get; private set; }

    private void Awake()
    {
        var button = GetComponent<Button>();
        OnButtonClicked = button.OnClickAsObservable();
    }
}
