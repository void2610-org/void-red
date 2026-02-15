using System.Collections.Generic;
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
    [SerializeField] private StaggeredSlideInGroup choiceStagger;

    public Observable<int> OnChoiceSelected => _onChoiceSelected;

    private readonly Subject<int> _onChoiceSelected = new();
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
        choiceStagger.Play();
    }

    public void Hide()
    {
        choiceStagger.Cancel();
        _disposables.Dispose();
        _disposables = new CompositeDisposable();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
        _onChoiceSelected.Dispose();
    }
}
