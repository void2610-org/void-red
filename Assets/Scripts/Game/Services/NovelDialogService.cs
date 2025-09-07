using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Googleスプレッドシートからノベルダイアログデータを取得・管理するサービス
/// シナリオIDごとにシートを分けて管理
/// </summary>
public class NovelDialogService
{
    private const string SPREADSHEET_ID = "1cPwaMiTwriP5eGhxYqIYvdpjJ81xsnwDdBtULMlP82I";
    
    // シナリオIDとダイアログデータのキャッシュ
    private readonly Dictionary<string, List<DialogData>> _dialogCache = new();
    
    /// <summary>
    /// シナリオIDに対応するダイアログデータを取得
    /// </summary>
    /// <param name="scenarioId">シナリオID（シート名として使用）</param>
    /// <returns>ダイアログデータのリスト</returns>
    public async UniTask<List<DialogData>> GetDialogDataAsync(string scenarioId)
    {
        // キャッシュに存在する場合は返却
        if (_dialogCache.TryGetValue(scenarioId, out var cachedData))
        {
            return new List<DialogData>(cachedData);
        }
        
        try
        {
            // スプレッドシートからデータを取得
            var sheetData = await GoogleSpreadSheetService.GetSheet(SPREADSHEET_ID, scenarioId);
            
            if (sheetData == null || sheetData.Count <= 1)
            {
                return new List<DialogData>();
            }
            
            // スプレッドシートデータをDialogDataに変換
            var dialogList = ConvertSheetToDialogData(sheetData);
            
            // キャッシュに保存
            _dialogCache[scenarioId] = new List<DialogData>(dialogList);
            
            return dialogList;
        }
        catch (System.Exception ex)
        {
            return new List<DialogData>();
        }
    }
    
    /// <summary>
    /// スプレッドシートデータをDialogDataのリストに変換
    /// 列構成: SpeakerName | DialogText | CharacterImageName | SEClipName | CustomCharSpeed
    /// </summary>
    private List<DialogData> ConvertSheetToDialogData(IList<IList<object>> sheetData)
    {
        var dialogList = new List<DialogData>();
        
        // ヘッダー行をスキップ（1行目）
        for (int i = 1; i < sheetData.Count; i++)
        {
            var row = sheetData[i];
            
            // 空行をスキップ
            if (row.Count == 0 || IsEmptyRow(row))
                continue;
            
            try
            {
                var dialogData = CreateDialogDataFromRow(row);
                if (dialogData != null)
                {
                    dialogList.Add(dialogData);
                }
            }
            catch (System.Exception ex)
            {
                // エラーが発生した行はスキップ
            }
        }
        
        return dialogList;
    }
    
    /// <summary>
    /// スプレッドシートの1行からDialogDataを作成
    /// </summary>
    private DialogData CreateDialogDataFromRow(IList<object> row)
    {
        if (row.Count < 2)
        {
            return null;
        }
        
        // 基本情報（必須）
        var speakerName = GetStringValue(row, 0);
        var dialogText = GetStringValue(row, 1);
        
        // セリフが空の場合はスキップ
        if (string.IsNullOrEmpty(dialogText))
            return null;
        
        // オプション情報
        var characterImageName = GetStringValue(row, 2);
        var seClipName = GetStringValue(row, 3);
        var customCharSpeed = GetFloatValue(row, 4, -1f);
        
        // DialogDataを作成
        var dialogData = new DialogData(speakerName, dialogText, characterImageName, seClipName);
        
        // カスタム文字速度を設定
        if (customCharSpeed > 0)
        {
            dialogData.SetCustomCharSpeed(customCharSpeed);
        }
        
        return dialogData;
    }
    
    /// <summary>
    /// 行が空かどうかをチェック
    /// </summary>
    private bool IsEmptyRow(IList<object> row)
    {
        foreach (var cell in row)
        {
            if (!string.IsNullOrWhiteSpace(cell?.ToString()))
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// 安全に文字列値を取得
    /// </summary>
    private string GetStringValue(IList<object> row, int index)
    {
        return index < row.Count ? row[index]?.ToString() ?? "" : "";
    }
    
    /// <summary>
    /// 安全にfloat値を取得
    /// </summary>
    private float GetFloatValue(IList<object> row, int index, float defaultValue)
    {
        if (index >= row.Count) return defaultValue;
        
        if (float.TryParse(row[index]?.ToString(), out var result))
            return result;
        
        return defaultValue;
    }
    
    /// <summary>
    /// キャッシュをクリア
    /// </summary>
    public void ClearCache()
    {
        _dialogCache.Clear();
    }
}
