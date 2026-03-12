using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
    [SerializeField] private StaggeredSlideInGroup buttonStagger;
    [SerializeField] private List<Button> choiceButtons;
    [SerializeField] private List<TextMeshProUGUI> choiceLabels;

    private CompositeDisposable _disposables = new();
    private UniTaskCompletionSource<int> _selectionCompletionSource;

    /// <summary>
    /// 選択肢が押されるまで待機する
    /// </summary>
    /// <returns>押された選択肢の番号</returns>
    public UniTask<int> WaitForSelectionAsync() => _selectionCompletionSource.Task;

    public void Setup(List<string> labels)
    {
        _disposables.Dispose();
        _disposables = new CompositeDisposable();
        _selectionCompletionSource = new UniTaskCompletionSource<int>();

        for (var i = 0; i < choiceButtons.Count; i++)
        {
            if (i < labels.Count)
            {
                var index = i;
                choiceLabels[i].text = labels[i];
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].OnClickAsObservable()
                    .Subscribe(_ => _selectionCompletionSource.TrySetResult(index))
                    .AddTo(_disposables);
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }

        gameObject.SetActive(true);
        buttonStagger.Play();
    }

    public void Hide()
    {
        buttonStagger.Cancel();
        _disposables.Dispose();
        _disposables = new CompositeDisposable();
        _selectionCompletionSource?.TrySetCanceled();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
        _selectionCompletionSource?.TrySetCanceled();
    }
}
