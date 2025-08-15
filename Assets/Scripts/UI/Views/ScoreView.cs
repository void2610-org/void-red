using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using LitMotion;

/// <summary>
/// スコア表示を担当するViewクラス（プレイヤーと敵のスコアを同時表示）
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ScoreView : MonoBehaviour
{
    [Header("敵側要素（上側）")]
    [SerializeField] private Image enemyEnvelopeImage;
    [SerializeField] private Image enemyScoreBackground;
    [SerializeField] private TextMeshProUGUI enemyScoreText;
    [SerializeField] private Transform enemyContainer;
    
    [Header("プレイヤー側要素（下側）")]
    [SerializeField] private Image playerEnvelopeImage;
    [SerializeField] private Image playerScoreBackground;
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private Transform playerContainer;
    
    private const float SLIDE_IN_DURATION = 0.5f;
    private const float FADE_OUT_DURATION = 0.3f;
    private const float SLIDE_DISTANCE = 300f;
    
    private CanvasGroup _canvasGroup;
    private Vector3 _enemyOriginalPosition;
    private Vector3 _playerOriginalPosition;
    
    private void Awake()
    {
        // CanvasGroupを取得または追加
        _canvasGroup = GetComponent<CanvasGroup>();
        
        // 初期位置を保存
        if (enemyContainer) _enemyOriginalPosition = enemyContainer.localPosition;
        if (playerContainer) _playerOriginalPosition = playerContainer.localPosition;
        
        // 初期状態は非表示
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
    
    /// <summary>
    /// プレイヤーと敵のスコアを同時に表示
    /// </summary>
    public async UniTask ShowScores(float playerScore, float enemyScore)
    {
        playerScoreText.text = playerScore.ToString("F2");
        enemyScoreText.text = enemyScore.ToString("F2");
        SetupInitialPositions();
        
        // 表示開始
        await LMotion.Create(0f, 1f, FADE_OUT_DURATION)
            .WithEase(Ease.InQuad)
            .Bind(alpha => _canvasGroup.alpha = alpha)
            .AddTo(gameObject); 
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        
        await SlideInFromRight();
        await UniTask.Delay(500);
        await SlideInFromLeft();
        
        // 指定時間待機
        await UniTask.Delay(3000);
        
        // 全体フェードアウト（背景とコンテンツを同時に）
        await LMotion.Create(1f, 0f, FADE_OUT_DURATION)
            .WithEase(Ease.InQuad)
            .Bind(alpha => _canvasGroup.alpha = alpha)
            .AddTo(gameObject)
            .ToUniTask();
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
    
    /// <summary>
    /// 初期位置を設定
    /// </summary>
    private void SetupInitialPositions()
    {
        if (enemyContainer)
        {
            var pos = _enemyOriginalPosition;
            pos.x -= SLIDE_DISTANCE;
            enemyContainer.localPosition = pos;
        }
        
        if (playerContainer)
        {
            var pos = _playerOriginalPosition;
            pos.x += SLIDE_DISTANCE;
            playerContainer.localPosition = pos;
        }
    }
    
    /// <summary>
    /// 敵側を左からスライドイン
    /// </summary>
    private async UniTask SlideInFromLeft()
    {
        if (!enemyContainer) return;
        
        var startPos = enemyContainer.localPosition;
        await LMotion.Create(startPos, _enemyOriginalPosition, SLIDE_IN_DURATION)
            .WithEase(Ease.OutQuart)
            .Bind(pos => enemyContainer.localPosition = pos)
            .AddTo(gameObject)
            .ToUniTask();
    }
    
    /// <summary>
    /// プレイヤー側を右からスライドイン
    /// </summary>
    private async UniTask SlideInFromRight()
    {
        if (!playerContainer) return;
        
        var startPos = playerContainer.localPosition;
        await LMotion.Create(startPos, _playerOriginalPosition, SLIDE_IN_DURATION)
            .WithEase(Ease.OutQuart)
            .Bind(pos => playerContainer.localPosition = pos)
            .AddTo(gameObject)
            .ToUniTask();
    }
}