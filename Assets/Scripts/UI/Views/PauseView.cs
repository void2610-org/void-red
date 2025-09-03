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
public class PauseView : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button titleButton;
    
    public Observable<Unit> OnTitleButtonClicked => _onTitleButtonClicked;
    
    private readonly Subject<Unit> _onTitleButtonClicked = new();
    
    public void Show()
    {
        Time.timeScale = 0;
        panel.SetActive(true);
    }

    private void Hide()
    {
        Time.timeScale = 1;
        panel.SetActive(false);
    }
    
    private void Awake()
    {
        resumeButton.onClick.AddListener(Hide);
        titleButton.onClick.AddListener(() => _onTitleButtonClicked.OnNext(Unit.Default));
        
        Hide();
    }
}