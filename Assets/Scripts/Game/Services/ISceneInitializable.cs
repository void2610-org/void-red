using Cysharp.Threading.Tasks;

/// <summary>
/// シーンの非同期初期化を行うPresenterが実装するインターフェース
/// </summary>
public interface ISceneInitializable
{
    /// <summary>
    /// シーンの初期化完了を待つ
    /// </summary>
    UniTask WaitForInitializationAsync();
}
