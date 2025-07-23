using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;

public static class GoogleDriveService
{
    private static readonly string[] _scopes = new [] { DriveService.Scope.DriveReadonly };
    private const string APPLICATION_NAME = "void-red";

    private class PageToken
    {
        public string Token { get; set; } = "";
    }

    public static async UniTask<List<File>> GetFolderList(string folderId)
    {
        var credential = await GoogleAuthService.GetCredentialAsync(_scopes);
        var driveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = APPLICATION_NAME
        });

        var query = $"('{folderId}' in parents) and (mimeType = 'application/vnd.google-apps.folder') and (trashed = false)";
        var pageToken = new PageToken();
        var files = new List<File>();
        files.AddRange(await RequestFiles(driveService, query, pageToken));
        while (!string.IsNullOrEmpty(pageToken.Token))
        {
            files.AddRange(await RequestFiles(driveService, query, pageToken));
        }

        return files;
    }

    public static async UniTask<List<File>> GetSpreadSheetList(string folderId)
    {
        var credential = await GoogleAuthService.GetCredentialAsync(_scopes);
        var driveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = APPLICATION_NAME
        });

        var query = $"('{folderId}' in parents) and (mimeType = 'application/vnd.google-apps.spreadsheet') and (trashed = false)";
        var pageToken = new PageToken();
        var files = new List<File>();
        files.AddRange(await RequestFiles(driveService, query, pageToken));
        while (!string.IsNullOrEmpty(pageToken.Token))
        {
            files.AddRange(await RequestFiles(driveService, query, pageToken));
        }

        return files;
	// 戻り値のFileオブジェクトの以下を利用するなどします
	// file.Id : スプレッドシートの場合スプレッドシートのID
	// file.Name : ドライブ上の表示名
    }

    private static async UniTask<IList<File>> RequestFiles(DriveService service, string query, PageToken pageToken)
    {
        var listRequest = service.Files.List();
        listRequest.PageSize = 200;
        listRequest.Fields = "nextPageToken, files(id, name)";
        listRequest.PageToken = pageToken.Token;
        listRequest.Q = query;

        var response = await listRequest.ExecuteAsync();
        if (response.Files != null && response.Files.Count > 0)
        {
            pageToken.Token = response.NextPageToken;
            return response.Files;
        }

        pageToken.Token = null;
        return new List<File>();
    }
}