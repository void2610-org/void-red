using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

public static class GoogleSpreadSheetService
{
    private static readonly string[] _scopes = new [] { SheetsService.Scope.SpreadsheetsReadonly };

    public static async UniTask<IList<IList<object>>> GetSheet(string sheetId, string sheetRange = "Sheet!A:Z")
    {
        var credential = GoogleAuthService.GetCredential(_scopes);
        if (credential == null) return null;

        var sheetService = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential
        });

        // sheetRangeで指定されたセルのデータを取得
        var result = await sheetService.Spreadsheets.Values.Get(sheetId, sheetRange).ExecuteAsync();
        return result.Values;
    }
}