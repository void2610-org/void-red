using UnityEngine;
using UnityEngine.UI;
using R3;

/// <summary>
/// ポーズボタンのViewコンポーネント
/// </summary>
[RequireComponent(typeof(Button))]
public class PauseButtonView : MonoBehaviour
{
    public Observable<Unit> OnButtonClicked { get; private set; }

    private void Awake()
    {
        var button = GetComponent<Button>();
        OnButtonClicked = button.OnClickAsObservable();
    }
}