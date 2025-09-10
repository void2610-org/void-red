using System;
using UnityEngine;
using Discord.Sdk;
using VContainer.Unity;

public class DiscordService : IStartable, IDisposable
{
    private readonly Client _client;
    private readonly DiscordAuthenticator _authenticator;
    private readonly DiscordRichPresenceService _richPresenceService;
    
    public void SetSceneState(SceneType sceneType) => _richPresenceService.SetSceneState(sceneType);
    public void SetDetails(string prefix, string details) => _richPresenceService.SetDetails(prefix, details);
    
    public DiscordService()
    {
        _client = new Client();
        _authenticator = new DiscordAuthenticator(_client);
        _richPresenceService = new DiscordRichPresenceService(_client);
        
        // ログのコールバック設定
        _client.AddLogCallback((m, s) => Debug.LogError($"Log: {s} - {m}"), LoggingSeverity.Error);
        _client.SetStatusChangedCallback(OnStatusChanged);
    }
    
    private void OnStatusChanged(Client.Status status, Client.Error error, int errorCode)
    {
        Debug.Log($"Status changed: {status}");
        if(error != Client.Error.None) Debug.LogError($"Error: {error}, code: {errorCode}");

        if (status == Client.Status.Ready) 
            _richPresenceService.SetSceneState(SceneType.Title);
    }
 
    public void Start()
    {
        _authenticator.StartOAuthFlow();
    }
    
    public void Dispose()
    {
        _client?.Dispose();
    }
}