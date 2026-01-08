using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;

// 対話フェーズのUI
// 2つの選択肢ボタンを表示
public class DialoguePhaseView : MonoBehaviour
{
    [Header("選択肢1")]
    [SerializeField] private Button choice1Button;
    [SerializeField] private TextMeshProUGUI choice1Label;

    [Header("選択肢2")]
    [SerializeField] private Button choice2Button;
    [SerializeField] private TextMeshProUGUI choice2Label;

    public Observable<int> OnChoiceSelected => _onChoiceSelected;

    private readonly Subject<int> _onChoiceSelected = new();
    private CompositeDisposable _disposables = new();

    // 選択肢をセットアップ
    public void Setup(string label1, string label2)
    {
        _disposables.Dispose();
        _disposables = new CompositeDisposable();

        choice1Label.text = label1;
        choice2Label.text = label2;

        choice1Button.OnClickAsObservable()
            .Subscribe(_ => _onChoiceSelected.OnNext(0))
            .AddTo(_disposables);

        choice2Button.OnClickAsObservable()
            .Subscribe(_ => _onChoiceSelected.OnNext(1))
            .AddTo(_disposables);
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    private void OnDestroy()
    {
        _disposables.Dispose();
        _onChoiceSelected.Dispose();
    }
}
