using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using Game.PersonalityLog;
using Cysharp.Threading.Tasks;
using R3;

/// <summary>
/// ポーズ画面を表示するViewコンポーネント
/// </summary>
public class PauseView : BaseWindowView
{
    [Header("ポーズ固有UIコンポーネント")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button titleButton;

    public Observable<Unit> OnTitleButtonClicked { get; private set; }
    public Observable<Unit> OnResumeButtonClicked { get; private set; }

    public override void Show()
    {
        Time.timeScale = 0;
        base.Show();
        // 一番前面に移動
        this.transform.SetAsLastSibling();
    }

    public override void Hide()
    {
        Time.timeScale = 1;
        base.Hide();
    }

    protected override void Awake()
    {
        closeButton = resumeButton;
        base.Awake();

        OnTitleButtonClicked = titleButton.OnClickAsObservable();
        OnResumeButtonClicked = resumeButton.OnClickAsObservable();
    }
}