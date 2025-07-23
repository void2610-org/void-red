using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

public static class GoogleSpreadSheetService
{
    private static readonly string[] _scopes = new [] { SheetsService.Scope.SpreadsheetsReadonly };

    public static async UniTask<IList<IList<object>>> GetSheet(string sheetId, string sheetName)
    {
        var credential = await GoogleAuthService.GetCredentialAsync(_scopes);
        if (credential == null) return null;

        var sheetService = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential
        });

        // シート全体のデータを取得
        var range = $"{sheetName}";
        var result = await sheetService.Spreadsheets.Values.Get(sheetId, range).ExecuteAsync();
        return result.Values;
    }
}