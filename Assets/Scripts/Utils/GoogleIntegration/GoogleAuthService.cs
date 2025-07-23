using System.IO;
using Cysharp.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using UnityEngine;
using UnityEngine.Networking;

public static class GoogleAuthService
{
    private static string KeyPath => Path.Combine(Application.streamingAssetsPath, "void-red-c7ec6e87a6c6.json");

    private static ICredential _credential;

    public static async UniTask<ICredential> GetCredentialAsync(string[] scopes)
    {
        if (_credential != null) return _credential;

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL環境ではUnityWebRequestを使用
        var request = UnityWebRequest.Get(KeyPath);
        await request.SendWebRequest();
        
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"GoogleAuthService: 認証ファイルの読み込みに失敗しました: {request.error}");
            return null;
        }
        
        var jsonContent = request.downloadHandler.text;
        using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent)))
        {
            _credential = GoogleCredential.FromStream(stream)
                .CreateScoped(scopes).UnderlyingCredential;
        }
#else
        // その他の環境では従来通りFileStreamを使用
        using (var stream = new FileStream(KeyPath, FileMode.Open, FileAccess.Read))
        {
            _credential = GoogleCredential.FromStream(stream)
                .CreateScoped(scopes).UnderlyingCredential;
        }
#endif

        return _credential;
    }
}