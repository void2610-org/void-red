using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

/// <summary>
/// 人格ログを表示するViewコンポーネント
/// </summary>
public class PersonalityLogView : BaseWindowView
{
    [Header("UIコンポーネント")]
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private ScrollRect scrollRect;
    
    private GameProgressService _gameProgressService;
    private RectTransform _scrollContent;
    
    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(GameProgressService gameProgressService)
    {
        _scrollContent = scrollRect.content;
        _gameProgressService = gameProgressService;
    }

    public override void Show()
    {
        UpdateLogDisplay();
        base.Show();
    }

    /// <summary>
    /// ログ表示を更新（UniTask版）
    /// </summary>
    private void UpdateLogDisplay()
    {
        // var logData = _gameProgressService.GetPersonalityLogData();
        // var displayText = FormatLogData(logData);
        
        // テキストを設定
        // logText.text = displayText;
        
        // レイアウトを段階的に更新
        logText.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(logText.rectTransform);
        
        var textHeight = logText.preferredHeight;
        var currentSize = _scrollContent.sizeDelta;
        _scrollContent.sizeDelta = new Vector2(currentSize.x, Mathf.Max(textHeight, 100f));
        
        Canvas.ForceUpdateCanvases();
        
        // スクロールを一番上に移動
        scrollRect.verticalNormalizedPosition = 1f;
    }
}