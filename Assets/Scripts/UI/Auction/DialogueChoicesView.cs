using System.Collections.Generic;
using LitMotion;
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

        var targets = new List<(RectTransform, CanvasGroup)>();
        for (var i = 0; i < choiceButtons.Count; i++)
        {
            if (!choiceButtons[i].gameObject.activeSelf) continue;
            var rect = (RectTransform)choiceButtons[i].transform;
            rect.anchoredPosition = _buttonOriginalPositions[i];
            targets.Add((rect, _buttonCanvasGroups[i]));
        }

        targets.StaggeredSlideIn(new Vector2(slideOffset, 0), animDuration, staggerDelay, _animHandles, moveEase: Ease.OutBack);
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
