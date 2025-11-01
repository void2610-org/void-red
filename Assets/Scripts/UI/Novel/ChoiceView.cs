using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
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
    private bool _isWaitingForChoice;
    private int _selectedChoiceIndex = -1;
    
    private void Awake()
    {
        // ChoicePanelの初期状態を設定
        choicePanelCanvasGroup.alpha = 0f;
        choicePanelCanvasGroup.interactable = false;
        choicePanelCanvasGroup.blocksRaycasts = false;
        
        questionBackground.color = Color.white;
        questionText.text = "";
        
        // プレハブボタンは非表示にする
        choiceButtonPrefab.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 選択肢を表示して選択を待つ
    /// </summary>
    /// <param name="choiceData">選択肢データ</param>
    /// <returns>選択されたインデックス</returns>
    public async UniTask<int> ShowChoice(ChoiceData choiceData)
    {
        // UI要素を設定
        SetupUIElements(choiceData);
        
        // 表示状態にする
        ShowPanel();
        
        // ユーザーの選択待ち
        var selectedIndex = await WaitForUserChoice();
        
        // 非表示にする
        HidePanel();
        
        return selectedIndex;
    }
    
    /// <summary>
    /// UI要素を設定
    /// </summary>
    private void SetupUIElements(ChoiceData choiceData)
    {
        questionText.text = choiceData.Question;
        
        ClearChoiceButtons();
        CreateChoiceButtons(choiceData.Options);
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
    private void ShowPanel()
    {
        choicePanelCanvasGroup.alpha = 1f;
        choicePanelCanvasGroup.interactable = true;
        choicePanelCanvasGroup.blocksRaycasts = true;
    }
    
    /// <summary>
    /// ユーザーの選択を待つ
    /// </summary>
    private async UniTask<int> WaitForUserChoice()
    {
        _isWaitingForChoice = true;
        _selectedChoiceIndex = -1;
        
        // 選択されるまで待機
        await UniTask.WaitUntil(() => !_isWaitingForChoice);
        
        return _selectedChoiceIndex;
    }
    
    /// <summary>
    /// パネルを非表示状態にする
    /// </summary>
    private void HidePanel()
    {
        choicePanelCanvasGroup.alpha = 0f;
        choicePanelCanvasGroup.interactable = false;
        choicePanelCanvasGroup.blocksRaycasts = false;
        
        // ボタンをクリア
        ClearChoiceButtons();
    }
    
    /// <summary>
    /// 選択肢ボタンがクリックされた時の処理
    /// </summary>
    private void OnChoiceButtonClicked(int choiceIndex)
    {
        if (!choicePanelCanvasGroup.interactable || !_isWaitingForChoice) 
            return;
        
        _selectedChoiceIndex = choiceIndex;
        _isWaitingForChoice = false;
    }
    
    private void OnDestroy()
    {
        ClearChoiceButtons();
    }
}