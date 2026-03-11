using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// コイントス演出を管理するView
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class CoinFlipView : BasePhaseView
{
    [SerializeField] private Animator coinAnimator;
    [SerializeField] private Image coinImage;
    [SerializeField] private Sprite frontSprite;
    [SerializeField] private Sprite backSprite;
    [SerializeField] private TextMeshProUGUI resultText;

    private static readonly int _flipTrigger = Animator.StringToHash("Flip");

    public async UniTask PlayCoinFlipAsync(bool isPlayerFirst)
    {
        // 初期化
        resultText.alpha = 0f;
        resultText.text = isPlayerFirst ? "先攻" : "後攻";

        // 表示
        Show();
        await UniTask.Delay(200);

        // コインアニメーション（1.5倍速でスプライトをゼロ交点で切り替え）
        coinImage.sprite = frontSprite;
        coinAnimator.speed = 1.5f;
        coinAnimator.SetTrigger(_flipTrigger);
        await UniTask.Delay(180);   // scaleY ゼロ交点①（表→裏）
        coinImage.sprite = backSprite;
        await UniTask.Delay(140);   // scaleY ゼロ交点②（裏→表）
        coinImage.sprite = frontSprite;
        await UniTask.Delay(120);   // scaleY ゼロ交点③（表→裏）
        coinImage.sprite = backSprite;
        await UniTask.Delay(95);    // scaleY ゼロ交点④（裏→表）
        coinImage.sprite = isPlayerFirst ? frontSprite : backSprite;  // 先攻なら表、後攻なら裏
        await UniTask.Delay(400);   // アニメーション終了まで待機
        coinAnimator.speed = 1f;

        // 結果テキスト表示
        resultText.FadeIn(0.3f, Ease.OutQuart).ToUniTask().Forget();
        await UniTask.Delay(700);

        // 非表示
        Hide();
        await UniTask.Delay(300);
    }
}
