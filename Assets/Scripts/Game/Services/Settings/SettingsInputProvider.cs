using System;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using Void2610.SettingsSystem;

/// <summary>
/// InputActionsProviderをラップしてISettingsInputProviderを実装
/// 設定画面の入力操作をSettingsSystemパッケージに提供
/// </summary>
public class SettingsInputProvider : ISettingsInputProvider, IDisposable
{
    public Observable<Unit> OnToggleSettings => _onToggleSettings;
    public Observable<float> OnNavigateHorizontal => _onNavigateHorizontal;

    private readonly Subject<Unit> _onToggleSettings = new();
    private readonly Subject<float> _onNavigateHorizontal = new();
    private readonly CompositeDisposable _disposables = new();
    private readonly InputAction _navigateAction;

    public SettingsInputProvider(InputActionsProvider inputActionsProvider)
    {
        _navigateAction = inputActionsProvider.UI.Navigate;

        // Pauseアクションを設定トグルに接続
        inputActionsProvider.UI.Pause.OnPerformedAsObservable()
            .Subscribe(_ => _onToggleSettings.OnNext(Unit.Default))
            .AddTo(_disposables);

        // NavigateアクションのX成分を水平ナビゲーションに接続
        inputActionsProvider.UI.Navigate.OnPerformedAsObservable()
            .Subscribe(_ =>
            {
                var value = _navigateAction.ReadValue<Vector2>();
                _onNavigateHorizontal.OnNext(value.x);
            })
            .AddTo(_disposables);
    }

    public void Dispose()
    {
        _disposables?.Dispose();
        _onToggleSettings?.Dispose();
        _onNavigateHorizontal?.Dispose();
    }
}
