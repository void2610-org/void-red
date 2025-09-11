#if !UNITY_WEBGL || UNITY_EDITOR
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
        _activity.SetName("VOID RED");
        _activity.SetType(ActivityTypes.Playing);
        _activity.SetDetailsUrl("https://void-red.void2610.dev/");
    }
    
    public void SetSceneState(SceneType sceneType)
    {
        switch (sceneType)
        {
            case SceneType.Title:
                _activity.SetDetails("タイトル");
                _activity.SetState("ゲーム開始を待っています");
                break;
            case SceneType.Home:
                _activity.SetDetails("ホーム画面");
                _activity.SetState("メニューを見ています");
                break;
            case SceneType.Battle:
                _activity.SetDetails("バトル中");
                _activity.SetState("対戦準備中");
                break;
            case SceneType.Novel:
                _activity.SetDetails("ストーリー閲覧中");
                _activity.SetState("物語を読んでいます");
                break;
        }
        
        UpdateRichPresence();
    }
    
    public void SetState(string prefix, string details)
    {
        _activity.SetState($"{prefix}: {details}");
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
#endif