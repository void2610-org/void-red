using System;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

/// <summary>
/// HelpViewとAllHelpDataの橋渡しを行うPresenterクラス
/// </summary>
public class HelpPresenter : IStartable, IDisposable
{
    private HelpView _helpView;
    private HelpButtonView _helpButtonView;
    private readonly AllHelpData _allHelpData;
    private readonly InputActionsProvider _inputActionsProvider;
    private readonly CompositeDisposable _disposables = new();

    private int _currentIndex;

    public HelpPresenter(AllHelpData allHelpData, InputActionsProvider inputActionsProvider)
    {
        _allHelpData = allHelpData;
        _inputActionsProvider = inputActionsProvider;
    }

    public void Start()
    {
        // タイトルではヘルプ無し
        if (SceneManager.GetActiveScene().name == "TitleScene") return;
        
        // ビューの取得
        _helpView = UnityEngine.Object.FindFirstObjectByType<HelpView>();
        _helpButtonView = UnityEngine.Object.FindFirstObjectByType<HelpButtonView>();

        // Helpアクションの購読（トグル処理）
        _inputActionsProvider.UI.Help.OnPerformedAsObservable()
            .Subscribe(_ => ToggleHelp())
            .AddTo(_disposables);

        // ヘルプボタンのイベント購読
        _helpButtonView.OnButtonClicked
            .Subscribe(_ => ShowHelp())
            .AddTo(_disposables);

        // イベント購読
        _helpView.OnPreviousClicked
            .Subscribe(_ => ShowPreviousHelp())
            .AddTo(_disposables);

        _helpView.OnNextClicked
            .Subscribe(_ => ShowNextHelp())
            .AddTo(_disposables);
    }

    /// <summary>
    /// ヘルプ画面の表示/非表示を切り替え
    /// </summary>
    private void ToggleHelp()
    {
        if (_helpView.IsShowing)
            _helpView.Hide();
        else
            ShowHelp();
    }

    /// <summary>
    /// ヘルプ画面を表示
    /// </summary>
    private void ShowHelp()
    {
        _currentIndex = 0;
        UpdateHelpDisplay();
        _helpView.Show();
    }

    /// <summary>
    /// 前のヘルプを表示
    /// </summary>
    private void ShowPreviousHelp()
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            UpdateHelpDisplay();
        }
    }

    /// <summary>
    /// 次のヘルプを表示
    /// </summary>
    private void ShowNextHelp()
    {
        if (_currentIndex < _allHelpData.Count - 1)
        {
            _currentIndex++;
            UpdateHelpDisplay();
        }
    }

    /// <summary>
    /// ヘルプ表示を更新
    /// </summary>
    private void UpdateHelpDisplay()
    {
        var helpData = _allHelpData.GetHelpByIndex(_currentIndex);
        if (!helpData) return;
        
        _helpView.DisplayHelp(helpData, _currentIndex, _allHelpData.Count);
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }
}
