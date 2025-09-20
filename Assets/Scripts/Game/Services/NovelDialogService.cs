using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// ノベルダイアログデータを取得・管理するサービス
/// スプレッドシートまたはローカルExcelファイルから読み込み
/// </summary>
public class NovelDialogService
{
    private const string SPREADSHEET_ID = "1cPwaMiTwriP5eGhxYqIYvdpjJ81xsnwDdBtULMlP82I";
    private readonly ExcelDialogLoader _excelDialogLoader;
    
    /// <summary>
    /// Excel/スプレッドシートの切り替えフラグ（外部から設定可能）
    /// </summary>
    public bool UseLocalExcel { get; set; } = true; // trueでローカルExcel、falseでスプレッドシート
    
    /// <summary>
    /// コンストラクタ（依存性注入対応）
    /// </summary>
    /// <param name="excelDialogLoader">ExcelDialogLoader（VContainerから注入）</param>
    public NovelDialogService(ExcelDialogLoader excelDialogLoader)
    {
        _excelDialogLoader = excelDialogLoader;
    }
    
    /// <summary>
    /// シナリオIDに対応するダイアログデータを取得
    /// </summary>
    /// <param name="scenarioId">シナリオID（シート名として使用）</param>
    /// <returns>ダイアログデータのリスト</returns>
    public async UniTask<List<DialogData>> GetDialogDataAsync(string scenarioId)
    {
        if (UseLocalExcel) // プロパティを使用
        {
            return await GetDialogDataFromExcel(scenarioId);
        }
        else
        {
            return await GetDialogDataFromSpreadsheet(scenarioId);
        }
    }
    
    /// <summary>
    /// ローカルExcelファイルからダイアログデータを取得
    /// </summary>
    private async UniTask<List<DialogData>> GetDialogDataFromExcel(string scenarioId)
    {
        var dialogData = await _excelDialogLoader.LoadDialogDataAsync(scenarioId);
        
        if (dialogData == null)
        {
            Debug.LogError($"[NovelDialogService] ローカルExcelからシナリオ '{scenarioId}' のデータを取得できませんでした");
            return null;
        }
        
        return dialogData;
    }
    
    /// <summary>
    /// Googleスプレッドシートからダイアログデータを取得
    /// </summary>
    private async UniTask<List<DialogData>> GetDialogDataFromSpreadsheet(string scenarioId)
    {
        // スプレッドシートからデータを取得
        var sheetData = await GoogleSpreadSheetService.GetSheet(SPREADSHEET_ID, scenarioId);
        
        if (sheetData == null)
        {
            Debug.LogError($"[NovelDialogService] スプレッドシートからシナリオ '{scenarioId}' のデータを取得できませんでした");
            return null;
        }
        
        if (sheetData.Count <= 1)
        {
            Debug.LogError($"[NovelDialogService] シナリオ '{scenarioId}' にデータが存在しません（ヘッダー行のみまたは空）");
            return null;
        }
        
        // スプレッドシートデータをDialogDataに変換
        return ConvertSheetToDialogData(sheetData);
    }
    
    /// <summary>
    /// スプレッドシートデータをDialogDataのリストに変換
    /// 列構成: SpeakerName | DialogText | パラメータタイプ | パラメータ値 | パラメータタイプ | パラメータ値 ...
    /// </summary>
    private List<DialogData> ConvertSheetToDialogData(IList<IList<object>> sheetData)
    {
        var dialogList = new List<DialogData>();
        
        // ヘッダー行をスキップ（1行目）
        for (var i = 1; i < sheetData.Count; i++)
        {
            var row = sheetData[i];
            
            // 空行をスキップ
            if (row.Count == 0 || IsEmptyRow(row))
                continue;
            
            var dialogData = CreateDialogDataFromRow(row);
            if (dialogData != null) dialogList.Add(dialogData);
        }
        
        return dialogList;
    }
    
    /// <summary>
    /// スプレッドシートの1行からDialogDataを作成
    /// 基本情報（SpeakerName, DialogText）の後、動的パラメータを解析
    /// </summary>
    private DialogData CreateDialogDataFromRow(IList<object> row)
    {
        if (row.Count < 2) return null;
        
        // 基本情報（必須）
        var speakerName = GetStringValue(row, 0);
        var dialogText = GetStringValue(row, 1);
        
        // セリフが空の場合はスキップ
        if (string.IsNullOrEmpty(dialogText)) return null;
        
        // 動的パラメータを解析（3列目以降）
        var parameters = ParseDynamicParameters(row, 2);
        
        // DialogDataを作成
        return new DialogData(speakerName, dialogText, parameters);
    }
    
    /// <summary>
    /// 動的パラメータを解析してDictionaryを作成
    /// パラメータタイプとパラメータ値のペアを処理
    /// </summary>
    /// <param name="row">スプレッドシート行データ</param>
    /// <param name="startIndex">パラメータ解析開始インデックス</param>
    /// <returns>パラメータのDictionary</returns>
    private Dictionary<DialogParameterType, object> ParseDynamicParameters(IList<object> row, int startIndex)
    {
        var parameters = new Dictionary<DialogParameterType, object>();
        
        // パラメータタイプとパラメータ値のペアを解析
        for (var i = startIndex; i < row.Count - 1; i += 2)
        {
            var parameterTypeString = GetStringValue(row, i);
            var parameterValueString = GetStringValue(row, i + 1);
            
            // 空のパラメータタイプはスキップ
            if (string.IsNullOrEmpty(parameterTypeString)) continue;
            
            // パラメータタイプを変換
            if (DialogParameterTypeExtensions.TryParseParameterType(parameterTypeString, out var parameterType))
            {
                // 値を適切な型に変換
                var convertedValue = parameterType.ConvertValue(parameterValueString);
                parameters[parameterType] = convertedValue;
            }
            else
            {
                Debug.LogWarning($"[NovelDialogService] 不明なパラメータタイプ: {parameterTypeString}");
            }
        }
        
        return parameters;
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
    
}
