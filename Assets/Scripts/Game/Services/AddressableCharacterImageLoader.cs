using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace VoidRed.Game.Services
{
    /// <summary>
    /// Addressables を使用してキャラクター画像を動的に読み込むサービス
    /// Addressables専用（Resourcesフォールバックなし）
    /// </summary>
    public class AddressableCharacterImageLoader
    {
        private readonly Dictionary<string, Sprite> _spriteCache = new();
        
        /// <summary>
        /// キャラクター画像を非同期で読み込み（Addressables専用）
        /// </summary>
        /// <param name="characterImageName">画像名（例: "alv_normal", "noah_smile"）</param>
        /// <returns>読み込まれたSprite、失敗時はnull</returns>
        public async UniTask<Sprite> LoadCharacterImageAsync(string characterImageName)
        {
            if (string.IsNullOrEmpty(characterImageName))
            {
                return null;
            }

            // キャッシュから確認
            if (_spriteCache.TryGetValue(characterImageName, out var cachedSprite))
            {
                return cachedSprite;
            }

            try
            {
                Debug.Log($"[AddressableCharacterImageLoader] 画像を読み込み中: {characterImageName}");
                
                // Addressables で非同期読み込み（キー名は画像名そのまま）
                var handle = Addressables.LoadAssetAsync<Sprite>(characterImageName);
                var sprite = await handle.ToUniTask();
                
                if (sprite != null)
                {
                    _spriteCache[characterImageName] = sprite;
                    Debug.Log($"[AddressableCharacterImageLoader] 画像読み込み成功: {characterImageName}");
                }
                else
                {
                    Debug.LogWarning($"[AddressableCharacterImageLoader] 画像が見つかりません: {characterImageName}");
                }
                
                return sprite;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AddressableCharacterImageLoader] 画像読み込みエラー ({characterImageName}): {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// キャッシュをクリア
        /// </summary>
        public void ClearCache()
        {
            _spriteCache.Clear();
        }

        /// <summary>
        /// 特定の画像をキャッシュから削除
        /// </summary>
        public void RemoveFromCache(string characterImageName)
        {
            _spriteCache.Remove(characterImageName);
        }
    }
}
