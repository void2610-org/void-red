using System.Collections.Generic;
using LitMotion;
using LitMotion.Extensions;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// 対話フェーズの選択肢表示View
/// 4つのボタンを持ち、3択時は4つ目を非表示にする
/// </summary>
public class DialogueChoicesView : MonoBehaviour
{
    [SerializeField] private List<Button> choiceButtons;
    [SerializeField] private List<TextMeshProUGUI> choiceLabels;

    [Header("アニメーション設定")]
    [SerializeField] private float staggerDelay = 0.05f;
    [SerializeField] private float animDuration = 0.2f;
    [SerializeField] private float slideOffset = 50f;

    public Observable<int> OnChoiceSelected => _onChoiceSelected;

    private readonly Subject<int> _onChoiceSelected = new();
    private readonly List<MotionHandle> _animHandles = new();
    private readonly List<CanvasGroup> _buttonCanvasGroups = new();
    private readonly List<Vector2> _buttonOriginalPositions = new();
    private CompositeDisposable _disposables = new();

    public void Setup(List<string> labels)
    {
        _disposables.Dispose();
        _disposables = new CompositeDisposable();

        for (var i = 0; i < choiceButtons.Count; i++)
        {
            if (i < labels.Count)
            {
                var index = i;
                choiceLabels[i].text = labels[i];
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].OnClickAsObservable()
                    .Subscribe(_ => _onChoiceSelected.OnNext(index))
                    .AddTo(_disposables);
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }

        gameObject.SetActive(true);
        PlayEnterAnimation();
    }

    public void Hide()
    {
        _animHandles.CancelAll();
        _disposables.Dispose();
        _disposables = new CompositeDisposable();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// ボタンの順次スライド+フェードインアニメーション
    /// シーンに配置された位置めがけてアニメーションする
    /// </summary>
    private void PlayEnterAnimation()
    {
        _animHandles.CancelAll();

        var activeIndex = 0;
        for (var i = 0; i < choiceButtons.Count; i++)
        {
            if (!choiceButtons[i].gameObject.activeSelf) continue;

            var rect = (RectTransform)choiceButtons[i].transform;
            var canvasGroup = _buttonCanvasGroups[i];
            var originalPos = _buttonOriginalPositions[i];
            var delay = staggerDelay * activeIndex;

            // 開始位置を右にオフセット、透明度を0に
            var startPos = originalPos + new Vector2(slideOffset, 0);
            rect.anchoredPosition = startPos;
            canvasGroup.alpha = 0f;

            // ディレイ付きでスライド+フェードイン
            var moveHandle = LMotion.Create(startPos, originalPos, animDuration)
                .WithEase(Ease.OutCubic)
                .WithDelay(delay)
                .BindToAnchoredPosition(rect)
                .AddTo(rect.gameObject);
            _animHandles.Add(moveHandle);

            var fadeHandle = LMotion.Create(0f, 1f, animDuration)
                .WithEase(Ease.OutCubic)
                .WithDelay(delay)
                .BindToAlpha(canvasGroup)
                .AddTo(canvasGroup.gameObject);
            _animHandles.Add(fadeHandle);

            activeIndex++;
        }
    }

    private void Awake()
    {
        // 各ボタンのCanvasGroupとシーン配置位置を保存
        foreach (var button in choiceButtons)
        {
            _buttonCanvasGroups.Add(button.GetComponent<CanvasGroup>());
            var rect = (RectTransform)button.transform;
            _buttonOriginalPositions.Add(rect.anchoredPosition);
        }
    }

    private void OnDestroy()
    {
        _animHandles.CancelAll();
        _disposables.Dispose();
        _onChoiceSelected.Dispose();
    }
}
