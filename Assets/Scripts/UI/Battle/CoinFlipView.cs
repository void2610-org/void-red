using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UnityEngine;
using Void2610.UnityTemplate;

/// <summary>
/// コイントス演出を管理するView
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class CoinFlipView : BasePhaseView
{
    [SerializeField] private Animator coinAnimator;
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

        // コインアニメーション
        coinAnimator.SetTrigger(_flipTrigger);
        await UniTask.Delay(1400);

        // 結果テキスト表示
        resultText.FadeIn(0.3f, Ease.OutQuart);
        await UniTask.Delay(700);

        // 非表示
        Hide();
        await UniTask.Delay(300);
    }
}
