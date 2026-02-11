using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LitMotion;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// 獲得カード一覧を表示するサブView
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class CardAcquisitionView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button nextButton;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private Transform textContainer;
    [SerializeField] private AcquiredCardTextView cardTextPrefab;

    [Header("アニメーション設定")]
    [SerializeField] private float initialDelay = 0.3f;
    [SerializeField] private float staggerDelay = 0.05f;
    [SerializeField] private float animDuration = 0.2f;
    [SerializeField] private float cardSlideOffset = -50f;
    [SerializeField] private float textSlideOffset = 50f;

    private readonly Subject<Unit> _onNextButtonClicked = new();
    private readonly List<GameObject> _instantiatedItems = new();
    private readonly List<MotionHandle> _animHandles = new();

    /// <summary>
    /// 獲得カードを一括表示し、nextボタンで進行
    /// </summary>
    public async UniTask ShowCardsAsync(IEnumerable<CardData> cards)
    {
        await DisplayCardsAsync(cards);
        await _onNextButtonClicked.FirstAsync();
        Hide();
    }

    public async UniTask DisplayCardsAsync(IEnumerable<CardData> cards)
    {
        Show();
        ClearInstantiatedItems();

        foreach (var cardData in cards)
        {
            // カードをcardContainerに生成（アニメーション前は非表示）
            var cardView = Instantiate(cardPrefab, cardContainer);
            cardView.Initialize(cardData);
            cardView.SetInteractable(false);
            cardView.gameObject.GetOrAddComponent<CanvasGroup>().alpha = 0f;
            _instantiatedItems.Add(cardView.gameObject);

            // テキストをtextContainerに生成（アニメーション前は非表示）
            var textItem = Instantiate(cardTextPrefab, textContainer);
            textItem.Initialize(cardData);
            textItem.gameObject.GetOrAddComponent<CanvasGroup>().alpha = 0f;
            _instantiatedItems.Add(textItem.gameObject);
        }

        await UniTask.Delay(System.TimeSpan.FromSeconds(initialDelay));
        PlayEnterAnimation();
    }

    public async UniTask WaitForNextAndHideAsync()
    {
        await _onNextButtonClicked.FirstAsync();
        Hide();
    }

    public void Hide()
    {
        _animHandles.CancelAll();
        canvasGroup.Hide();
        ClearInstantiatedItems();
    }

    private void Show()
    {
        canvasGroup.Show();
    }

    /// <summary>
    /// カードとテキストの順次スライド+フェードインアニメーション
    /// </summary>
    private void PlayEnterAnimation()
    {
        _animHandles.CancelAll();
        Canvas.ForceUpdateCanvases();

        // カード列: 下からスライドイン
        var cardTargets = Enumerable.Range(0, cardContainer.childCount)
            .Select(i => cardContainer.GetChild(i))
            .Select(c => ((RectTransform)c, c.gameObject.GetOrAddComponent<CanvasGroup>()))
            .ToList();
        cardTargets.StaggeredSlideIn(new Vector2(0, cardSlideOffset), animDuration, staggerDelay, _animHandles);

        // テキスト列: 右からスライドイン
        var textTargets = Enumerable.Range(0, textContainer.childCount)
            .Select(i => textContainer.GetChild(i))
            .Select(c => ((RectTransform)c, c.gameObject.GetOrAddComponent<CanvasGroup>()))
            .ToList();
        textTargets.StaggeredSlideIn(new Vector2(textSlideOffset, 0), animDuration, staggerDelay, _animHandles);
    }

    private void ClearInstantiatedItems()
    {
        foreach (var item in _instantiatedItems)
            Destroy(item);
        _instantiatedItems.Clear();
    }

    private void Awake()
    {
        canvasGroup.Hide();

        nextButton.OnClickAsObservable()
            .Subscribe(_ => _onNextButtonClicked.OnNext(Unit.Default))
            .AddTo(this);
    }

    private void OnDestroy()
    {
        _animHandles.CancelAll();
        _onNextButtonClicked.Dispose();
    }
}
