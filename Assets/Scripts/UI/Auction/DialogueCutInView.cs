using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// 対話フェーズのカットイン演出を担当するViewクラス
/// キャラ画像・カットイン画像・セリフが右から左にスライドする
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class DialogueCutInView : MonoBehaviour
{
    [SerializeField] private Image standingImage;
    [SerializeField] private Image cutInImage;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private RectTransform rectTransform;

    [Header("プレイヤーカットイン画像")]
    [SerializeField] private Sprite playerStandingSprite;
    [SerializeField] private Sprite playerCutInSprite;

    [Header("アニメーション設定")]
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private float exitDuration = 0.3f;
    [SerializeField] private float slideOffset = 1200f;
    [SerializeField] private float intervalDuration = 0.3f;

    private CanvasGroup _canvasGroup;
    private float _initialX;
    private MotionHandle _slideHandle;

    /// <summary>
    /// プレイヤーのカットイン演出を再生する
    /// </summary>
    public UniTask PlayPlayerCutInAsync(string text) => PlayCutInAsync(playerStandingSprite, playerCutInSprite, text);

    /// <summary>
    /// カットイン演出を再生する
    /// </summary>
    public async UniTask PlayCutInAsync(Sprite standing, Sprite cutIn, string text)
    {
        _slideHandle.TryCancel();

        standingImage.sprite = standing;
        cutInImage.sprite = cutIn;
        dialogueText.text = $"「{text}」";

        _canvasGroup.alpha = 1f;
        // 右画面外からスライドイン
        rectTransform.anchoredPosition = new Vector2(_initialX + slideOffset, rectTransform.anchoredPosition.y);
        _slideHandle = rectTransform.MoveToX(_initialX, slideDuration, Ease.OutCubic);
        await _slideHandle.ToUniTask();

        await UniTask.Delay((int)(displayDuration * 1000));

        // 左画面外へスライドアウト
        _slideHandle = rectTransform.MoveToX(_initialX - slideOffset, exitDuration, Ease.InCubic);
        await _slideHandle.ToUniTask();
        _canvasGroup.alpha = 0f;

        rectTransform.anchoredPosition = new Vector2(_initialX, rectTransform.anchoredPosition.y);

        // 次のカットインとの間隔
        await UniTask.Delay((int)(intervalDuration * 1000));
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        _initialX = rectTransform.anchoredPosition.x;
    }

    private void OnDestroy()
    {
        _slideHandle.TryCancel();
    }
}
