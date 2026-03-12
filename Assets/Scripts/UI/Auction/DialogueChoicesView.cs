using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

/// <summary>
/// 対話フェーズの選択肢表示View
/// 4つのボタンを持つ対話選択View
/// </summary>
public class DialogueChoicesView : MonoBehaviour
{
    [SerializeField] private StaggeredSlideInGroup buttonStagger;
    [SerializeField] private List<Button> choiceButtons;

    private CompositeDisposable _disposables = new();
    private UniTaskCompletionSource<int> _selectionCompletionSource;

    /// <summary>
    /// 選択肢が押されるまで待機する
    /// </summary>
    /// <returns>押された選択肢の番号</returns>
    public UniTask<int> WaitForSelectionAsync() => _selectionCompletionSource.Task;

    public void Setup()
    {
        _disposables.Dispose();
        _disposables = new CompositeDisposable();
        _selectionCompletionSource = new UniTaskCompletionSource<int>();

        for (var i = 0; i < choiceButtons.Count; i++)
        {
            var index = i;
            choiceButtons[i].gameObject.SetActive(true);
            choiceButtons[i].OnClickAsObservable()
                .Subscribe(_ => _selectionCompletionSource.TrySetResult(index))
                .AddTo(_disposables);
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
