using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// 敵の表示を担当するViewクラス
/// カード属性に応じてSpriteを切り替える
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class EnemyView : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private Image enemyImage;
    [SerializeField] private Image enemyImageBack;

    [Header("アニメーション設定")]
    [SerializeField] private Color imageColor = Color.white;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float crossFadeDuration = 0.4f;

    private EnemyData _enemyData;
    private CanvasGroup _canvasGroup;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Initialize(EnemyData enemyData)
    {
        _enemyData = enemyData;

        // デフォルトスプライトを設定
        enemyImage.sprite = _enemyData.DefaultSprite;
        enemyImage.color = imageColor;

        // 背面画像を透明に初期化
        enemyImageBack.color = new Color(1, 1, 1, 0);

        // 初期状態では非表示
        gameObject.SetActive(false);

        // アルヴだけサイズを変える
        if (enemyData.EnemyId == "alv")
        {
            enemyImage.transform.localPosition = new Vector3(0f, 0f, 0f);
            enemyImage.transform.localScale = Vector3.one;
            enemyImageBack.transform.localPosition = new Vector3(0f, 0f, 0f);
            enemyImageBack.transform.localScale = Vector3.one;
        }
        else
        {
            enemyImage.transform.localPosition = new Vector3(0f, -300f, 0f);
            enemyImage.transform.localScale = Vector3.one * 1.4f;
            enemyImageBack.transform.localPosition = new Vector3(0f, -300f, 0f);
            enemyImageBack.transform.localScale = Vector3.one * 1.4f;
        }
    }

    /// <summary>
    /// 指定された属性に応じて敵のSpriteを更新
    /// </summary>
    public async UniTask UpdateSpriteForAttribute(CardAttribute attribute)
    {
        if (!_enemyData) return;

        var newSprite = _enemyData.GetSpriteForAttribute(attribute);
        if (!newSprite) return;

        // スプライト切り替えアニメーション
        await PlaySpriteChangeAnimation(newSprite);
    }

    /// <summary>
    /// デフォルトスプライトに戻す（カード未選択時）
    /// </summary>
    public async UniTask ResetToDefaultSprite()
    {
        if (!_enemyData || !_enemyData.DefaultSprite) return;

        // デフォルトスプライトに切り替え
        await PlaySpriteChangeAnimation(_enemyData.DefaultSprite);
    }

    /// <summary>
    /// 2枚の画像を使った真のクロスフェードアニメーション
    /// </summary>
    private async UniTask PlaySpriteChangeAnimation(Sprite newSprite)
    {
        if (!enemyImage || !enemyImageBack) return;

        // 現在のスプライトと同じ場合はアニメーションをスキップ
        if (enemyImage.sprite == newSprite) return;

        // 背面画像に新しいスプライトを設定
        enemyImageBack.sprite = newSprite;
        enemyImageBack.color = new Color(1, 1, 1, 0);

        // 同時進行のクロスフェード
        var fadeOutFront = enemyImage.FadeOut(crossFadeDuration, Ease.InOutQuad);

        var fadeInBack = enemyImageBack.FadeIn(crossFadeDuration, Ease.InOutQuad);

        // 両方のアニメーションを同時実行
        await UniTask.WhenAll(
            fadeOutFront.AddTo(gameObject).ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy()),
            fadeInBack.AddTo(gameObject).ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy())
        );

        // アニメーション完了後、前面と背面を入れ替え
        (enemyImage.sprite, enemyImageBack.sprite) = (enemyImageBack.sprite, enemyImage.sprite);

        enemyImage.color = imageColor;
        enemyImageBack.color = new Color(1, 1, 1, 0);
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
    }
}
