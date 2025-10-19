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

    public Observable<Unit> OnTitleButtonClicked => _onTitleButtonClicked;
    public Observable<Unit> OnResumeButtonClicked => _onResumeButtonClicked;

    private readonly Subject<Unit> _onTitleButtonClicked = new();
    private readonly Subject<Unit> _onResumeButtonClicked = new();

    public override void Show()
    {
        Time.timeScale = 0;
        base.Show();
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

        resumeButton.onClick.AddListener(() => _onResumeButtonClicked.OnNext(Unit.Default));
        titleButton.onClick.AddListener(() => _onTitleButtonClicked.OnNext(Unit.Default));
    }
}