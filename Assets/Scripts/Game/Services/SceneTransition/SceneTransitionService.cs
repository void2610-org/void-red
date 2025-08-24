using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移とデータ受け渡しを管理するサービスクラス
/// VContainerでSingletonとして登録し、複数シーン間でデータを共有する
/// </summary>
public class SceneTransitionService
{
    /// <summary>遷移待機中のデータ</summary>
    private SceneTransitionData _pendingTransitionData;
    
    /// <summary>現在のシーンタイプ</summary>
    private SceneType _currentSceneType;
    
    /// <summary>デバッグログ出力フラグ</summary>
    private readonly bool _enableDebugLog = true;
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public SceneTransitionService()
    {
        // 現在のシーンタイプを初期化
        _currentSceneType = SceneUtility.GetCurrentSceneType();
        
        if (_enableDebugLog)
        {
            Debug.Log($"[SceneTransitionService] 初期化完了。現在のシーン: {_currentSceneType}");
        }
    }
    
    /// <summary>
    /// データを準備してシーンに遷移
    /// </summary>
    /// <typeparam name="T">遷移データの型</typeparam>
    /// <param name="data">遷移時に渡すデータ</param>
    /// <returns>遷移完了のUniTask</returns>
    public async UniTask TransitionToScene<T>(T data) where T : SceneTransitionData
    {
        if (data == null)
        {
            Debug.LogError("[SceneTransitionService] 遷移データがnullです");
            return;
        }
        
        PrepareTransition(data);
        await TransitionToScene(data.TargetScene);
    }
    
    /// <summary>
    /// 指定したシーンタイプに遷移
    /// </summary>
    /// <param name="targetScene">遷移先シーンタイプ</param>
    /// <returns>遷移完了のUniTask</returns>
    public async UniTask TransitionToScene(SceneType targetScene)
    {
        // バリデーション
        if (!targetScene.IsValid())
        {
            Debug.LogError($"[SceneTransitionService] 無効なSceneType: {targetScene}");
            return;
        }
        
        // 現在のシーンと同じ場合はスキップ
        if (_currentSceneType == targetScene)
        {
            Debug.LogWarning($"[SceneTransitionService] 既に {targetScene} にいるため遷移をスキップします");
            return;
        }
        
        var sceneName = targetScene.ToSceneName();
        
        if (_enableDebugLog)
        {
            var dataInfo = _pendingTransitionData?.GetDebugInfo() ?? "No Data";
            Debug.Log($"[SceneTransitionService] {_currentSceneType} → {targetScene} 遷移開始 (Data: {dataInfo})");
        }
        
        await TransitionToSceneInternal(sceneName);
        _currentSceneType = targetScene;
        
        if (_enableDebugLog)
        {
            Debug.Log($"[SceneTransitionService] {targetScene} 遷移完了");
        }
    }
    
    /// <summary>
    /// 遷移データを準備
    /// </summary>
    /// <typeparam name="T">遷移データの型</typeparam>
    /// <param name="data">遷移データ</param>
    public void PrepareTransition<T>(T data) where T : SceneTransitionData
    {
        _pendingTransitionData = data;
        
        if (_enableDebugLog)
        {
            Debug.Log($"[SceneTransitionService] 遷移データ準備: {data.GetDebugInfo()}");
        }
    }
    
    /// <summary>
    /// 遷移データを取得
    /// </summary>
    /// <typeparam name="T">遷移データの型</typeparam>
    /// <returns>遷移データ（存在しない場合はnull）</returns>
    public T GetTransitionData<T>() where T : SceneTransitionData
    {
        if (_pendingTransitionData is T data)
        {
            if (_enableDebugLog)
            {
                Debug.Log($"[SceneTransitionService] 遷移データ取得: {data.GetDebugInfo()}");
            }
            return data;
        }
        
        if (_pendingTransitionData != null)
        {
            Debug.LogWarning($"[SceneTransitionService] 遷移データの型が一致しません。期待: {typeof(T).Name}, 実際: {_pendingTransitionData.GetType().Name}");
        }
        else
        {
            Debug.LogWarning($"[SceneTransitionService] 遷移データが存在しません（型: {typeof(T).Name}）");
        }
        
        return null;
    }
    
    /// <summary>
    /// 遷移データをクリア
    /// </summary>
    public void ClearTransitionData()
    {
        if (_enableDebugLog && _pendingTransitionData != null)
        {
            Debug.Log($"[SceneTransitionService] 遷移データクリア: {_pendingTransitionData.GetDebugInfo()}");
        }
        
        _pendingTransitionData = null;
    }
    
    /// <summary>
    /// 内部的なシーン遷移処理
    /// </summary>
    /// <param name="sceneName">遷移先シーン名</param>
    /// <returns>遷移完了のUniTask</returns>
    private async UniTask TransitionToSceneInternal(string sceneName)
    {
        try
        {
            // LoadSceneAsyncを使用して非同期でシーンを読み込み
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            
            // 読み込み完了まで待機
            await UniTask.WaitUntil(() => asyncOperation.isDone);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SceneTransitionService] シーン遷移でエラーが発生しました: {sceneName}, Error: {ex.Message}");
            throw;
        }
    }
}