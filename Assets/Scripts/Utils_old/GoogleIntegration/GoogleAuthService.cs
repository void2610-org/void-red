using System.IO;
using Google.Apis.Auth.OAuth2;
using UnityEngine;

public static class GoogleAuthService
{
    private static string KeyPath => Path.Combine(Application.streamingAssetsPath, "void-red-c7ec6e87a6c6.json");

    private static ICredential _credential;

    public static ICredential GetCredential(string[] scopes)
    {
        if (_credential != null) return _credential;

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL環境ではGoogle APIが動作しないためnullを返す
        return null;
#else
        try
        {
            using (var stream = new FileStream(KeyPath, FileMode.Open, FileAccess.Read))
            {
                _credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(scopes).UnderlyingCredential;
            }
            return _credential;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"GoogleAuthService: 認証失敗 - {e.Message}");
            return null;
        }
#endif
    }
}