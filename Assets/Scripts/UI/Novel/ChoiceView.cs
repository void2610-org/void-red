using System.Collections.Generic;
using System.Linq;
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
public class ChoiceView : BaseWindowView
{
    [Header("UI要素")]
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

        Show(); // Show()内でGetPreferredNavigationTarget()が呼ばれ、最初の選択肢が選択される
        await UniTask.Delay(500);

        var selectedIndex = await _choiceSelectedSubject.FirstAsync();
        Hide();

        return selectedIndex;
    }

    /// <summary>
    /// 優先ナビゲーション対象を返す（最初の選択肢ボタン）
    /// </summary>
    protected override GameObject GetPreferredNavigationTarget()
    {
        return _choiceButtons.Count > 0 ? _choiceButtons[0].gameObject : null;
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
        
        //ナビゲーションを設定
        _choiceButtons.Select(b => b as Selectable).ToList().SetNavigation(isHorizontal: false, wrapAround: true);
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
    /// 選択肢ボタンがクリックされた時の処理
    /// </summary>
    private void OnChoiceButtonClicked(int choiceIndex)
    {
        if (!IsShowing) return;

        _choiceSelectedSubject.OnNext(choiceIndex);
    }

    protected override void Awake()
    {
        base.Awake();

        questionBackground.color = Color.white;
        questionText.text = "";
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _choiceSelectedSubject?.Dispose();
        ClearChoiceButtons();
    }
}