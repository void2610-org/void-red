using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
    [SerializeField] private AcquiredCardView cardPrefab;
    [SerializeField] private Transform textContainer;
    [SerializeField] private AcquiredCardTextView cardTextPrefab;
    [SerializeField] private StaggeredSlideInGroup cardStagger;
    [SerializeField] private StaggeredSlideInGroup textStagger;

    [Header("アニメーション設定")]
    [SerializeField] private float initialDelay = 0.3f;

    private readonly Subject<Unit> _onNextButtonClicked = new();
    private readonly List<GameObject> _instantiatedItems = new();

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
            // カードをcardContainerに生成
            var acquiredCard = Instantiate(cardPrefab, cardContainer);
            acquiredCard.Initialize(cardData);
            _instantiatedItems.Add(acquiredCard.gameObject);

            // テキストをtextContainerに生成（アニメーション前は非表示）
            var textItem = Instantiate(cardTextPrefab, textContainer);
            textItem.Initialize(cardData);
            textItem.gameObject.GetOrAddComponent<CanvasGroup>().alpha = 0f;
            _instantiatedItems.Add(textItem.gameObject);
        }

        await UniTask.Delay(System.TimeSpan.FromSeconds(initialDelay));
        cardStagger.Play();
        textStagger.Play();
    }

    public async UniTask WaitForNextAndHideAsync()
    {
        await _onNextButtonClicked.FirstAsync();
        Hide();
    }

    public void Hide()
    {
        cardStagger.Cancel();
        textStagger.Cancel();
        canvasGroup.Hide();
        ClearInstantiatedItems();
    }

    private void Show()
    {
        canvasGroup.Show();
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
        _onNextButtonClicked.Dispose();
    }
}
