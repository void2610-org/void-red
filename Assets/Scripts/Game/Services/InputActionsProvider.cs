using System;
using VContainer.Unity;

/// <summary>
/// InputSystem_Actionsのシングルトンプロバイダー
/// VContainerでSingletonとして管理され、アプリケーション全体で共有される
/// </summary>
public class InputActionsProvider : IDisposable
{
    private readonly InputSystem_Actions _inputActions;

    /// <summary>
    /// Battle アクションマップへのアクセス
    /// </summary>
    public InputSystem_Actions.BattleActions Battle => _inputActions.Battle;

    /// <summary>
    /// UI アクションマップへのアクセス
    /// </summary>
    public InputSystem_Actions.UIActions UI => _inputActions.UI;

    /// <summary>
    /// Novel アクションマップへのアクセス
    /// </summary>
    public InputSystem_Actions.NovelActions Novel => _inputActions.Novel;

    public InputActionsProvider()
    {
        // InputSystem_Actionsのインスタンスを作成
        _inputActions = new InputSystem_Actions();
    }

    public void Dispose()
    {
        // InputActionsのクリーンアップ
        _inputActions?.Dispose();
    }
}
