using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using Game.PersonalityLog;

/// <summary>
/// 人格ログを表示するViewコンポーネント
/// </summary>
public class PersonalityLogView : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private GameObject logPanel;
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button closeButton;
    
    private PersonalityLogService _personalityLogService;
    
    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(PersonalityLogService personalityLogService)
    {
        _personalityLogService = personalityLogService;
        
        // 閉じるボタンのイベント設定
        if (closeButton)
        {
            closeButton.onClick.AddListener(HideLog);
        }
        
        // 初期状態では非表示
        HideLog();
    }
    
    /// <summary>
    /// ログを表示
    /// </summary>
    public void ShowLog()
    {
        if (!logPanel) return;
        
        logPanel.SetActive(true);
        UpdateLogDisplay();
    }
    
    /// <summary>
    /// ログを非表示
    /// </summary>
    public void HideLog()
    {
        if (!logPanel) return;
        
        logPanel.SetActive(false);
    }
    
    /// <summary>
    /// ログ表示を更新
    /// </summary>
    private void UpdateLogDisplay()
    {
        if (!logText || _personalityLogService == null) return;
        
        var logData = _personalityLogService.GetLogData();
        var displayText = FormatLogData(logData);
        
        logText.text = displayText;
        
        // スクロールを一番上に移動
        if (scrollRect)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
    
    /// <summary>
    /// ログデータをテキスト形式に変換
    /// </summary>
    private string FormatLogData(PersonalityLogData logData)
    {
        if (logData?.chapters == null || logData.chapters.Count == 0)
        {
            return "まだバトル履歴がありません。";
        }
        
        var sb = new StringBuilder();
        sb.AppendLine("=== 人格ログ ===\n");
        
        for (int chapterIndex = 0; chapterIndex < logData.chapters.Count; chapterIndex++)
        {
            var chapter = logData.chapters[chapterIndex];
            sb.AppendLine($"【Chapter {chapterIndex + 1}: {chapter.enemyData?.EnemyName ?? "不明な敵"}】");
            
            if (chapter.turns == null || chapter.turns.Count == 0)
            {
                sb.AppendLine("  ターンデータなし");
                sb.AppendLine();
                continue;
            }
            
            for (int turnIndex = 0; turnIndex < chapter.turns.Count; turnIndex++)
            {
                var turn = chapter.turns[turnIndex];
                sb.AppendLine($"  ターン {turnIndex + 1}:");
                
                // プレイヤームーブ
                if (turn.playerMove != null)
                {
                    sb.AppendLine($"    プレイヤー: {turn.playerMove.card?.CardName ?? "なし"} " +
                                 $"({turn.playerMove.playStyle.ToJapaneseString()}) " +
                                 $"ベット:{turn.playerMove.mentalBet} 精神力:{turn.playerMove.currentMentalPower}");
                }
                
                // 敵ムーブ
                if (turn.enemyMove != null)
                {
                    sb.AppendLine($"    敵: {turn.enemyMove.card?.CardName ?? "なし"} " +
                                 $"({turn.enemyMove.playStyle.ToJapaneseString()}) " +
                                 $"ベット:{turn.enemyMove.mentalBet} 精神力:{turn.enemyMove.currentMentalPower}");
                }
                
                // イベント
                if (turn.events != null && turn.events.Count > 0)
                {
                    sb.AppendLine("    イベント:");
                    foreach (var turnEvent in turn.events)
                    {
                        sb.AppendLine($"      - {FormatEvent(turnEvent)}");
                    }
                }
                
                sb.AppendLine();
            }
            
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// イベントをテキスト形式に変換
    /// </summary>
    private string FormatEvent(TurnEvent turnEvent)
    {
        switch (turnEvent)
        {
            case ResonanceEvent resonance:
                return $"共鳴: {resonance.resonanceCard?.CardName ?? "不明なカード"}";
                
            case CardEvolutionEvent evolution:
                return $"進化 ({evolution.actorId}): {evolution.fromCard?.CardName ?? "不明"} → {evolution.toCard?.CardName ?? "不明"}";
                
            case CardCollapseEvent collapse:
                return $"崩壊 ({collapse.actorId}): {collapse.collapseCard?.CardName ?? "不明なカード"}";
                
            default:
                return $"不明なイベント: {turnEvent?.GetType().Name ?? "null"}";
        }
    }
}