using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// ダイアログ背景画像を管理するView
/// 画像切り替え時は一旦黒にフェードアウトしてから新しい画像をフェードイン
/// </summary>
[RequireComponent(typeof(Image))]
public class DialogBackgroundView : MonoBehaviour
{
    private Image _backgroundImage;
    [SerializeField] private float fadeDuration = 0.5f;

    /// <summary>
    /// 背景画像を設定（黒経由でフェード）
    /// </summary>
    public async UniTask SetBackground(Sprite sprite)
    {
        if (sprite == null) return;

        // 現在の画像と同じ場合はスキップ
        if (_backgroundImage.sprite == sprite) return;

        // 一旦黒にフェードアウト
        await _backgroundImage.FadeOut(fadeDuration, Ease.InOutQuad)
            .AddTo(gameObject)
            .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

        // スプライトを切り替え
        _backgroundImage.sprite = sprite;

        // 新しい画像をフェードイン
        await _backgroundImage.FadeIn(fadeDuration, Ease.InOutQuad)
            .AddTo(gameObject)
            .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
    }

    /// <summary>
    /// 即座に背景画像を設定（フェードなし）
    /// </summary>
    public void SetBackgroundImmediate(Sprite sprite)
    {
        _backgroundImage.sprite = sprite;
        _backgroundImage.color = sprite ? Color.white : Color.clear;
    }

    private void Awake()
    {
        _backgroundImage = GetComponent<Image>();
    }
}
