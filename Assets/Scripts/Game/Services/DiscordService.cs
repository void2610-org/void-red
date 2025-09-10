using UnityEngine;
using System.Linq;
using Discord.Sdk;
using VContainer.Unity;

public class DiscordService : IStartable
{
    private const ulong CLIENT_ID = 1415132179377160262;
    
    private readonly Client _client;
    private string _codeVerifier = "";
    
    public DiscordService()
    {
        _client = new Client();

        // Modifying LoggingSeverity will show you more or less logging information
        _client.AddLogCallback(OnLog, LoggingSeverity.Error);
        _client.SetStatusChangedCallback(OnStatusChanged);
    }
    
    private void StartOAuthFlow() {
        var authorizationVerifier = _client.CreateAuthorizationCodeVerifier();
        _codeVerifier = authorizationVerifier.Verifier();
        
        var args = new AuthorizationArgs();
        args.SetClientId(CLIENT_ID);
        args.SetScopes(Client.GetDefaultPresenceScopes());
        args.SetCodeChallenge(authorizationVerifier.Challenge());
        _client.Authorize(args, OnAuthorizeResult);
    }

    private void OnAuthorizeResult(ClientResult result, string code, string redirectUri) {
        Debug.Log($"Authorization result: [{result.Error()}] [{code}] [{redirectUri}]");
        if (!result.Successful()) return;
        GetTokenFromCode(code, redirectUri);
    }

    private void GetTokenFromCode(string code, string redirectUri) {
        _client.GetToken(CLIENT_ID,
            code,
            _codeVerifier,
            redirectUri,
            (result, token, refreshToken, tokenType, expiresIn, scope) => {
                if (token != "") OnReceivedToken(token);
                else OnRetrieveTokenFailed();
            });
    }

    
    private void OnReceivedToken(string token) {
        Debug.Log("Token received: " + token);
        _client.UpdateToken(AuthorizationTokenType.Bearer, token, (ClientResult result) => { _client.Connect(); });
    }

    private void OnRetrieveTokenFailed() =>  Debug.LogError("Failed to retrieve token");

    private void OnLog(string message, LoggingSeverity severity)
    {
        Debug.Log($"Log: {severity} - {message}");
    }
    
    private void ClientReady()
    {
        var activity = new Activity();
        activity.SetType(ActivityTypes.Playing);
        activity.SetState("バトル中");
        activity.SetDetails("対戦相手: アルヴ");
        
        _client.UpdateRichPresence(activity, result => {
            if (result.Successful()) {
                Debug.Log("Rich presence updated!");
            } else {
                Debug.LogError("Failed to update rich presence");
            }
        });
    }

    private void OnStatusChanged(Client.Status status, Client.Error error, int errorCode)
    {
        Debug.Log($"Status changed: {status}");
        if(error != Client.Error.None) Debug.LogError($"Error: {error}, code: {errorCode}");

        if (status == Client.Status.Ready) ClientReady();
    }
 
    public void Start()
    {
        StartOAuthFlow();
    }
}