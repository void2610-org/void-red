using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Addressablesを使用してキャラクター画像を動的に読み込むサービス
/// </summary>
public class AddressableCharacterImageLoader
{
    // 読み込み済み画像のキャッシュ
    private readonly Dictionary<string, Sprite> _loadedSprites = new();
    
    // AsyncOperationHandleのキャッシュ（メモリリーク防止）
    private readonly Dictionary<string, AsyncOperationHandle<Sprite>> _handles = new();
    
    /// <summary>
    /// キャラクター画像を非同期で読み込む
    /// </summary>
    /// <param name="imageName">画像名（Addressableキー）</param>
    /// <returns>読み込まれたSprite（失敗時はnull）</returns>
    public async UniTask<Sprite> LoadCharacterImageAsync(string imageName)
    {
        if (string.IsNullOrEmpty(imageName))
        {
            return null;
        }
        
        // キャッシュに存在する場合は返却
        if (_loadedSprites.TryGetValue(imageName, out var cachedSprite))
        {
            return cachedSprite;
        }
        
        // Addressablesから読み込み
        var handle = Addressables.LoadAssetAsync<Sprite>(imageName);
        _handles[imageName] = handle;
        
        var sprite = await handle.ToUniTask();
        
        // キャッシュに保存
        if (sprite)
        {
            _loadedSprites[imageName] = sprite;
        }
        
        return sprite;
    }
    
    /// <summary>
    /// 指定した画像をキャッシュから削除し、メモリを解放
    /// </summary>
    /// <param name="imageName">解放する画像名</param>
    public void UnloadCharacterImage(string imageName)
    {
        if (string.IsNullOrEmpty(imageName))
            return;
        
        // キャッシュから削除
        _loadedSprites.Remove(imageName);
        
        // Addressableハンドルを解放
        if (_handles.TryGetValue(imageName, out var handle))
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
            _handles.Remove(imageName);
        }
    }
    
    /// <summary>
    /// 全ての読み込み済み画像をキャッシュから削除し、メモリを解放
    /// </summary>
    public void UnloadAllCharacterImages()
    {
        // 全てのハンドルを解放
        foreach (var kvp in _handles)
        {
            if (kvp.Value.IsValid())
            {
                Addressables.Release(kvp.Value);
            }
        }
        
        _handles.Clear();
        _loadedSprites.Clear();
    }
    
    /// <summary>
    /// 指定した画像がキャッシュに存在するかチェック
    /// </summary>
    /// <param name="imageName">確認する画像名</param>
    /// <returns>キャッシュに存在するかどうか</returns>
    public bool IsImageCached(string imageName)
    {
        return !string.IsNullOrEmpty(imageName) && _loadedSprites.ContainsKey(imageName);
    }
}
