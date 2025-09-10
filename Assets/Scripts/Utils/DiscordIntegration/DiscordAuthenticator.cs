using UnityEngine;
using Discord.Sdk;
using System;

public class DiscordAuthenticator
{
    private const ulong CLIENT_ID = 1415132179377160262;
    
    private readonly Client _client;
    private string _codeVerifier = "";
    
    public DiscordAuthenticator(Client client)
    {
        _client = client;
    }
    
    public void StartOAuthFlow() 
    {
        var authorizationVerifier = _client.CreateAuthorizationCodeVerifier();
        _codeVerifier = authorizationVerifier.Verifier();
        
        var args = new AuthorizationArgs();
        args.SetClientId(CLIENT_ID);
        args.SetScopes(Client.GetDefaultPresenceScopes());
        args.SetCodeChallenge(authorizationVerifier.Challenge());
        _client.Authorize(args, OnAuthorizeResult);
    }

    private void OnAuthorizeResult(ClientResult result, string code, string redirectUri) 
    {
        if (!result.Successful()) return;
        
        GetTokenFromCode(code, redirectUri);
    }

    private void GetTokenFromCode(string code, string redirectUri) 
    {
        _client.GetToken(CLIENT_ID,
            code,
            _codeVerifier,
            redirectUri,
            (result, token, refreshToken, tokenType, expiresIn, scope) => {
                if (token != "") OnReceivedToken(token);
            });
    }
    
    private void OnReceivedToken(string token) 
    {
        _client.UpdateToken(AuthorizationTokenType.Bearer, token,
            _ => _client.Connect());
    }
}