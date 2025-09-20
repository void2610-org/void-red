using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;
using ExcelDataReader;
using System.Data;

/// <summary>
/// StreamingAssetsからExcelファイルを読み込んでDialogDataに変換するローダー
/// </summary>
public class ExcelDialogLoader
{
    private const string EXCEL_FILE_NAME = "Dialog.xlsx";
    
    /// <summary>
    /// 指定されたシート名からダイアログデータを読み込み
    /// </summary>
    /// <param name="sheetName">読み込むシート名（シナリオID）</param>
    /// <returns>ダイアログデータのリスト</returns>
    public async UniTask<List<DialogData>> LoadDialogDataAsync(string sheetName)
    {
        var filePath = Path.Combine(Application.streamingAssetsPath, EXCEL_FILE_NAME);
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"[ExcelDialogLoader] Excelファイルが見つかりません: {filePath}");
            return null;
        }
        
        try
        {
            // ファイルをバイト配列として読み込み（WebGLでも動作）
            var fileBytes = await LoadFileAsync(filePath);
            
            // Excelデータを解析
            var dialogData = await ParseExcelDataAsync(fileBytes, sheetName);
            
            Debug.Log($"[ExcelDialogLoader] シート '{sheetName}' から {dialogData?.Count ?? 0} 件のダイアログを読み込みました");
            return dialogData;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ExcelDialogLoader] Excelファイルの読み込みでエラーが発生: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// ファイルを非同期で読み込み
    /// </summary>
    private async UniTask<byte[]> LoadFileAsync(string filePath)
    {
        // WebGL対応: UnityWebRequestを使用
        #if UNITY_WEBGL && !UNITY_EDITOR
        using var request = UnityEngine.Networking.UnityWebRequest.Get(filePath);
        await request.SendWebRequest();
        
        if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            throw new System.Exception($"ファイル読み込みエラー: {request.error}");
        }
        
        return request.downloadHandler.data;
        #else
        // エディター・スタンドアロン: File.ReadAllBytesを非同期実行
        return await UniTask.RunOnThreadPool(() => File.ReadAllBytes(filePath));
        #endif
    }
    
    /// <summary>
    /// Excelデータを解析してDialogDataリストに変換
    /// </summary>
    private async UniTask<List<DialogData>> ParseExcelDataAsync(byte[] fileBytes, string sheetName)
    {
        return await UniTask.RunOnThreadPool(() =>
        {
            using var stream = new MemoryStream(fileBytes);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            
            var dataSet = reader.AsDataSet();
            var dataTable = dataSet.Tables[sheetName];
            
            if (dataTable == null)
            {
                Debug.LogError($"[ExcelDialogLoader] シート '{sheetName}' が見つかりません");
                return null;
            }
            
            return ConvertDataTableToDialogData(dataTable);
        });
    }
    
    /// <summary>
    /// DataTableをDialogDataリストに変換
    /// </summary>
    private List<DialogData> ConvertDataTableToDialogData(DataTable dataTable)
    {
        var dialogList = new List<DialogData>();
        
        // ヘッダー行をスキップして処理
        for (var i = 1; i < dataTable.Rows.Count; i++)
        {
            var row = dataTable.Rows[i];
            
            // 空行をスキップ
            if (IsEmptyRow(row)) continue;
            
            var dialogData = CreateDialogDataFromRow(row);
            if (dialogData != null) dialogList.Add(dialogData);
        }
        
        return dialogList;
    }
    
    /// <summary>
    /// DataRowからDialogDataを作成
    /// </summary>
    private DialogData CreateDialogDataFromRow(DataRow row)
    {
        // 基本情報（必須）
        var speakerName = GetStringValue(row, 0);
        var dialogText = GetStringValue(row, 1);
        
        // セリフが空の場合はスキップ
        if (string.IsNullOrEmpty(dialogText)) return null;
        
        // 動的パラメータを解析（3列目以降）
        var parameters = ParseDynamicParameters(row, 2);
        
        return new DialogData(speakerName, dialogText, parameters);
    }
    
    /// <summary>
    /// 動的パラメータを解析
    /// </summary>
    private Dictionary<DialogParameterType, object> ParseDynamicParameters(DataRow row, int startIndex)
    {
        var parameters = new Dictionary<DialogParameterType, object>();
        
        for (var i = startIndex; i < row.Table.Columns.Count - 1; i += 2)
        {
            var parameterTypeString = GetStringValue(row, i);
            var parameterValueString = GetStringValue(row, i + 1);
            
            if (string.IsNullOrEmpty(parameterTypeString)) continue;
            
            if (DialogParameterTypeExtensions.TryParseParameterType(parameterTypeString, out var parameterType))
            {
                var convertedValue = parameterType.ConvertValue(parameterValueString);
                parameters[parameterType] = convertedValue;
            }
            else
            {
                Debug.LogWarning($"[ExcelDialogLoader] 不明なパラメータタイプ: {parameterTypeString}");
            }
        }
        
        return parameters;
    }
    
    /// <summary>
    /// 行が空かどうかをチェック
    /// </summary>
    private bool IsEmptyRow(DataRow row)
    {
        for (var i = 0; i < row.Table.Columns.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(row[i]?.ToString()))
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// 安全に文字列値を取得
    /// </summary>
    private string GetStringValue(DataRow row, int columnIndex)
    {
        return columnIndex < row.Table.Columns.Count ? row[columnIndex]?.ToString() ?? "" : "";
    }
}