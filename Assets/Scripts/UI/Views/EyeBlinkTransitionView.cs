using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// 瞬きエフェクト風のトランジションView
/// 現段階はシンプルな黒フェード、後で瞬き形状に拡張可能
/// </summary>
[RequireComponent(typeof(Image))]
public class EyeBlinkTransitionView : MonoBehaviour
{
    [Header("アニメーション設定")]
    [SerializeField] private float closeDuration = 0.3f;
    [SerializeField] private float holdDuration = 0.2f;
    [SerializeField] private float openDuration = 0.3f;

    private Image _overlayImage;
    private MotionHandle _currentHandle;

    /// <summary>
    /// 完全なトランジション（閉じる→待機→開く）
    /// </summary>
    public async UniTask PlayTransitionAsync()
    {
        await PlayCloseAsync();
        await UniTask.Delay((int)(holdDuration * 1000));
        await PlayOpenAsync();
    }

    /// <summary>
    /// 閉じるアニメーション（黒へフェードイン）
    /// </summary>
    public async UniTask PlayCloseAsync()
    {
        _currentHandle.TryCancel();

        gameObject.SetActive(true);

        _currentHandle = _overlayImage.ColorTo(new Color(0f, 0f, 0f, 1f), closeDuration, Ease.InQuart);

        await _currentHandle.ToUniTask();
    }

    /// <summary>
    /// 開くアニメーション（透明へフェードアウト）
    /// </summary>
    public async UniTask PlayOpenAsync()
    {
        _currentHandle.TryCancel();

        _currentHandle = _overlayImage.ColorTo(new Color(0f, 0f, 0f, 0f), openDuration, Ease.OutQuart);

        await _currentHandle.ToUniTask();

        gameObject.SetActive(false);
    }

    private void Awake()
    {
        _overlayImage = GetComponent<Image>();

        // 初期状態は非表示
        _overlayImage.color = new Color(0f, 0f, 0f, 0f);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _currentHandle.TryCancel();
    }
}
