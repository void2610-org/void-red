using UnityEngine;
using Discord.Sdk;

public class DiscordRichPresenceService
{
    private readonly Client _client;
    private readonly Activity _activity;
    
    public DiscordRichPresenceService(Client client)
    {
        _client = client;
        _activity = new Activity();
        _activity.SetType(ActivityTypes.Playing);
    }
    
    public void SetSceneState(SceneType sceneType)
    {
        switch (sceneType)
        {
            case SceneType.Title:
                _activity.SetState("タイトル");
                _activity.SetDetails("ゲーム開始を待っています");
                break;
            case SceneType.Home:
                _activity.SetState("ホーム画面");
                _activity.SetDetails("メニューを見ています");
                break;
            case SceneType.Battle:
                _activity.SetState("バトル中");
                break;
            case SceneType.Novel:
                _activity.SetState("ストーリー閲覧中");
                break;
        }
        
        UpdateRichPresence();
    }
    
    public void SetDetails(string prefix, string details)
    {
        _activity.SetDetails($"{prefix}: {details}");
        UpdateRichPresence();
    }
    
    private void UpdateRichPresence()
    {
        _client.UpdateRichPresence(_activity, result => {
            if (!result.Successful()) 
                Debug.LogError("Failed to update rich presence");
        });
    }
}