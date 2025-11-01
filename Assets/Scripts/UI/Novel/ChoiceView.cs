using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using LitMotion;
using R3;
using Void2610.UnityTemplate;

/// <summary>
/// 選択肢表示を担当するViewクラス
/// 質問文と選択肢ボタンを表示し、ユーザーの選択を受け取る
/// </summary>
public class ChoiceView : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private CanvasGroup choicePanelCanvasGroup;
    [SerializeField] private Image questionBackground;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private Button choiceButtonPrefab;

    private readonly List<Button> _choiceButtons = new();
    private readonly Subject<int> _choiceSelectedSubject = new();
    
    /// <summary>
    /// 選択肢を表示して選択を待つ
    /// </summary>
    /// <param name="choiceData">選択肢データ</param>
    /// <returns>選択されたインデックス</returns>
    public async UniTask<int> ShowChoice(ChoiceData choiceData)
    {
        questionText.text = choiceData.Question;
        ClearChoiceButtons();
        CreateChoiceButtons(choiceData.Options);
        
        await ShowPanel();
        var selectedIndex = await _choiceSelectedSubject.FirstAsync();
        await HidePanel();

        return selectedIndex;
    }
    
    /// <summary>
    /// 選択肢ボタンを動的生成
    /// </summary>
    private void CreateChoiceButtons(List<string> options)
    {
        for (var i = 0; i < options.Count; i++)
        {
            var button = Instantiate(choiceButtonPrefab, buttonContainer);
            button.gameObject.SetActive(true);
            
            // ボタンテキストを設定
            var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = options[i];
            
            // ボタンクリックイベントを設定
            var choiceIndex = i; // ラムダでキャプチャするためのローカル変数
            button.OnClickAsObservable()
                .Subscribe(_ => OnChoiceButtonClicked(choiceIndex))
                .AddTo(button);
            
            _choiceButtons.Add(button);
        }
    }
    
    /// <summary>
    /// 既存の選択肢ボタンを削除
    /// </summary>
    private void ClearChoiceButtons()
    {
        foreach (var button in _choiceButtons)
            Destroy(button.gameObject);
        _choiceButtons.Clear();
    }
    
    /// <summary>
    /// パネルを表示状態にする
    /// </summary>
    private async UniTask ShowPanel()
    {
        await choicePanelCanvasGroup.FadeIn(0.5f).ToUniTask();
        choicePanelCanvasGroup.interactable = true;
        choicePanelCanvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// パネルを非表示状態にする
    /// </summary>
    private async UniTask HidePanel()
    {
        choicePanelCanvasGroup.interactable = false;
        choicePanelCanvasGroup.blocksRaycasts = false;
        await choicePanelCanvasGroup.FadeOut(0.5f).ToUniTask();
        
        // ボタンをクリア
        ClearChoiceButtons();
    }
    
    /// <summary>
    /// 選択肢ボタンがクリックされた時の処理
    /// </summary>
    private void OnChoiceButtonClicked(int choiceIndex)
    {
        if (!choicePanelCanvasGroup.interactable) return;

        _choiceSelectedSubject.OnNext(choiceIndex);
    }

    private void Awake()
    {
        // ChoicePanelの初期状態を設定
        choicePanelCanvasGroup.alpha = 0f;
        choicePanelCanvasGroup.interactable = false;
        choicePanelCanvasGroup.blocksRaycasts = false;
        
        questionBackground.color = Color.white;
        questionText.text = "";
    }
    
    private void OnDestroy()
    {
        _choiceSelectedSubject?.Dispose();
        ClearChoiceButtons();
    }
}