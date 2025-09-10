using System;
using UnityEngine;
using Discord.Sdk;

public class DiscordService : IDisposable
{
    private const ulong CLIENT_ID = 1415132179377160262;
    
    private readonly Client _client;
    private readonly DiscordRichPresenceService _richPresenceService;
    
    public void SetSceneState(SceneType sceneType) => _richPresenceService.SetSceneState(sceneType);
    public void SetState(string prefix, string details) => _richPresenceService.SetState(prefix, details);
    
    public DiscordService()
    {
        _client = new Client();
        _client.SetApplicationId(CLIENT_ID);
        _richPresenceService = new DiscordRichPresenceService(_client);
        
        // 初期のリッチプレゼンス設定
        _richPresenceService.SetSceneState(SceneType.Title);
    }
    
    public void Dispose()
    {
        _client?.Dispose();
    }
}