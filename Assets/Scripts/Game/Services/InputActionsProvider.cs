using System;
using VContainer.Unity;

/// <summary>
/// InputSystem_Actionsのシングルトンプロバイダー
/// VContainerでSingletonとして管理され、アプリケーション全体で共有される
/// </summary>
public class InputActionsProvider : IDisposable
{
    private InputSystem_Actions _inputActions;

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
        _inputActions.UI.Enable();
    }

    /// <summary>
    /// シーン遷移時に呼び出され、アクションマップを適切に切り替える
    /// </summary>
    public void OnSceneChanged(SceneType targetScene)
    {
        // 全てのアクションマップを無効化
        _inputActions.Battle.Disable();
        _inputActions.Novel.Disable();

        // UIは常に有効
        _inputActions.UI.Enable();

        // 遷移先シーンに応じてアクションマップを有効化
        switch (targetScene)
        {
            case SceneType.Battle:
                _inputActions.Battle.Enable();
                break;
            case SceneType.Novel:
                _inputActions.Novel.Enable();
                break;
        }
    }

    public void Dispose()
    {
        // InputActionsのクリーンアップ
        _inputActions?.Dispose();
    }
}
