using System;
using UnityEngine;

#if !UNITY_WEBGL || UNITY_EDITOR
using Discord.Sdk;
#endif

public class DiscordService : IDisposable
{
#if !UNITY_WEBGL || UNITY_EDITOR
    private const ulong CLIENT_ID = 1415132179377160262;
    
    private readonly Client _client;
    private readonly DiscordRichPresenceService _richPresenceService;
#endif
    
    public void SetSceneState(SceneType sceneType)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        _richPresenceService?.SetSceneState(sceneType);
#endif
    }
    
    public void SetState(string prefix, string details)
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        _richPresenceService?.SetState(prefix, details);
#endif
    }
    
    public DiscordService()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        _client = new Client();
        _client.SetApplicationId(CLIENT_ID);
        _richPresenceService = new DiscordRichPresenceService(_client);
        
        // 初期のリッチプレゼンス設定
        _richPresenceService.SetSceneState(SceneType.Title);
#endif
    }
    
    public void Dispose()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        _client?.Dispose();
#endif
    }
}