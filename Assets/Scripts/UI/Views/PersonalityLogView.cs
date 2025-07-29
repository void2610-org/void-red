using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using Game.PersonalityLog;
using Cysharp.Threading.Tasks;

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
    private RectTransform _scrollContent;
    
    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(PersonalityLogService personalityLogService)
    {
        _scrollContent = scrollRect.content;
        _personalityLogService = personalityLogService;
        closeButton.onClick.AddListener(HideLog);
        
        HideLog();
    }
    
    /// <summary>
    /// ログを表示
    /// </summary>
    public void ShowLog()
    {
        logPanel.SetActive(true);
        UpdateLogDisplayAsync().Forget();
    }
    
    /// <summary>
    /// ログを非表示
    /// </summary>
    public void HideLog()
    {
        logPanel.SetActive(false);
    }
    
    /// <summary>
    /// ログ表示を更新（UniTask版）
    /// </summary>
    private async UniTaskVoid UpdateLogDisplayAsync()
    {
        var logData = _personalityLogService.GetLogData();
        var displayText = FormatLogData(logData);
        
        // テキストを設定
        logText.text = displayText;
        
        // 1フレーム待ってからレイアウト更新
        await UniTask.Yield();
        
        // レイアウトを段階的に更新
        await UpdateLayoutAsync();
        
        // スクロールを一番上に移動
        scrollRect.verticalNormalizedPosition = 1f;
    }
    
    /// <summary>
    /// レイアウトを更新
    /// </summary>
    private async UniTask UpdateLayoutAsync()
    {
        // Phase 1: TMProのテキスト情報を更新
        logText.ForceMeshUpdate();
        await UniTask.Yield();
        
        // Phase 2: 初回Canvas更新
        Canvas.ForceUpdateCanvases();
        await UniTask.Yield();
        
        // Phase 3: ContentSizeFitterのレイアウトを更新
        LayoutRebuilder.ForceRebuildLayoutImmediate(logText.rectTransform);
        await UniTask.Yield();
        
        // Phase 4: ScrollRectのContentサイズを調整
        var textHeight = logText.preferredHeight;
        var currentSize = _scrollContent.sizeDelta;
        _scrollContent.sizeDelta = new Vector2(currentSize.x, Mathf.Max(textHeight, 100f));
        await UniTask.Yield();
        
        // Phase 5: 最終Canvas更新でレイアウトを確定
        Canvas.ForceUpdateCanvases();
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
        
        for (var chapterIndex = 0; chapterIndex < logData.chapters.Count; chapterIndex++)
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