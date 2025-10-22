using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;

/// <summary>
/// Presenterの初期化完了を待つためのブリッジクラス
/// MonoBehaviourなのでFindAnyObjectByTypeで検索可能
/// ISceneInitializableをVContainerから注入して委譲する
/// </summary>
public class SceneInitializationBridge : MonoBehaviour
{
    [Inject] private ISceneInitializable _initializable;

    /// <summary>
    /// シーンの初期化完了を待つ
    /// </summary>
    public UniTask WaitForInitializationAsync()
    {
        return _initializable.WaitForInitializationAsync();
    }
}
