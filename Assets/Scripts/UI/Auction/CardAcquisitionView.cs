using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

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

    private readonly Subject<Unit> _onNextButtonClicked = new();
    private readonly List<GameObject> _instantiatedItems = new();

    /// <summary>
    /// 獲得カードを一括表示し、nextボタンで進行
    /// </summary>
    public async UniTask ShowCardsAsync(IEnumerable<CardData> cards)
    {
        Show();
        ClearInstantiatedItems();

        foreach (var cardData in cards)
        {
            // カードをcardContainerに生成
            var cardView = Instantiate(cardPrefab, cardContainer);
            cardView.Initialize(cardData);
            cardView.SetInteractable(false);
            _instantiatedItems.Add(cardView.gameObject);

            // テキストをtextContainerに生成
            var textItem = Instantiate(cardTextPrefab, textContainer);
            textItem.Initialize(cardData);
            _instantiatedItems.Add(textItem.gameObject);
        }

        await _onNextButtonClicked.FirstAsync();
        Hide();
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        ClearInstantiatedItems();
    }

    private void Show()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void ClearInstantiatedItems()
    {
        foreach (var item in _instantiatedItems)
            Destroy(item);
        _instantiatedItems.Clear();
    }

    private void Awake()
    {
        nextButton.OnClickAsObservable()
            .Subscribe(_ => _onNextButtonClicked.OnNext(Unit.Default))
            .AddTo(this);
    }

    private void OnDestroy()
    {
        _onNextButtonClicked.Dispose();
    }
}
