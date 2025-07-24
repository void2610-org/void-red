using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Googleスプレッドシートからカードのナレーション内容を管理するサービス
/// </summary>
public class CardNarrationService
{
    // スプレッドシートID（環境変数や設定ファイルから読み込むことを推奨）
    private const string SPREADSHEET_ID = "1Yj-f13peW3dxumUhcSdPNRZ5if5ogz9aXM2cB19bSgY";
    private const string SHEET_NAME = "main";
    private const string LOCAL_CSV_FILENAME = "CardNarrationData.csv";
    
    // カードIDをキーとしたナレーションデータのキャッシュ
    private readonly Dictionary<string, CardNarrationData> _narrationCache = new();
    private bool _isInitialized = false;

    /// <summary>
    /// カラムマッピング情報
    /// </summary>
    private struct ColumnMapping
    {
        public readonly int ColumnIndex;
        public readonly NarrationType NarrationType;
        public readonly PlayStyle PlayStyle;

        public ColumnMapping(int columnIndex, NarrationType narrationType, PlayStyle playStyle)
        {
            ColumnIndex = columnIndex;
            NarrationType = narrationType;
            PlayStyle = playStyle;
        }
    }

    // カラムマッピング定義
    private static readonly ColumnMapping[] _columnMappings = new[]
    {
        new ColumnMapping(1, NarrationType.PrePlay, PlayStyle.Hesitation),
        new ColumnMapping(2, NarrationType.PrePlay, PlayStyle.Impulse),
        new ColumnMapping(3, NarrationType.PrePlay, PlayStyle.Conviction),
        new ColumnMapping(4, NarrationType.PostBattleWin, PlayStyle.Hesitation),
        new ColumnMapping(5, NarrationType.PostBattleWin, PlayStyle.Impulse),
        new ColumnMapping(6, NarrationType.PostBattleWin, PlayStyle.Conviction),
        new ColumnMapping(7, NarrationType.PostBattleLose, PlayStyle.Hesitation),
        new ColumnMapping(8, NarrationType.PostBattleLose, PlayStyle.Impulse),
        new ColumnMapping(9, NarrationType.PostBattleLose, PlayStyle.Conviction),
        new ColumnMapping(10, NarrationType.PostBattleWinEnemy, PlayStyle.Hesitation),
        new ColumnMapping(11, NarrationType.PostBattleWinEnemy, PlayStyle.Impulse),
        new ColumnMapping(12, NarrationType.PostBattleWinEnemy, PlayStyle.Conviction),
        new ColumnMapping(13, NarrationType.PostBattleLoseEnemy, PlayStyle.Hesitation),
        new ColumnMapping(14, NarrationType.PostBattleLoseEnemy, PlayStyle.Impulse),
        new ColumnMapping(15, NarrationType.PostBattleLoseEnemy, PlayStyle.Conviction)
    };

    /// <summary>
    /// カラムデータからCardNarrationDataを作成する共通関数
    /// </summary>
    private static CardNarrationData CreateNarrationDataFromColumns(string[] columns)
    {
        var narrationData = new CardNarrationData();
        
        foreach (var mapping in _columnMappings)
        {
            if (columns.Length > mapping.ColumnIndex && 
                !string.IsNullOrEmpty(columns[mapping.ColumnIndex]?.Trim()))
            {
                narrationData.SetNarration(mapping.NarrationType, mapping.PlayStyle, columns[mapping.ColumnIndex].Trim());
            }
        }
        
        return narrationData;
    }

    /// <summary>
    /// サービスの初期化（スプレッドシートからデータを読み込む）
    /// </summary>
    public async UniTask InitializeAsync()
    {
        if (_isInitialized) return;
        
#if !UNITY_WEBGL || UNITY_EDITOR
        // WebGL以外の環境でのみGoogle Sheets APIを試す
        try
        {
            var data = await GoogleSpreadSheetService.GetSheet(SPREADSHEET_ID, SHEET_NAME);
            if (data != null && data.Count > 0)
            {
                ParseSpreadsheetData(data);
                _isInitialized = true;
                Debug.Log($"CardNarrationService: スプレッドシートから{_narrationCache.Count}件読み込み");
                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"CardNarrationService: Google Sheets API失敗 - {e.Message}");
        }
#endif
        
        // WebGL環境、またはGoogle Sheets APIが失敗した場合、ローカルCSVファイルを読み込む
        await LoadFromLocalCsv();
        _isInitialized = true;
    }

    /// <summary>
    /// スプレッドシートのデータをパース
    /// 想定形式: CardID | PrePlay_Hesitation | PrePlay_Impulse | PrePlay_Conviction | PostBattle_Hesitation | PostBattle_Impulse | PostBattle_Conviction | PostBattleEnemy_Hesitation | PostBattleEnemy_Impulse | PostBattleEnemy_Conviction
    /// </summary>
    private void ParseSpreadsheetData(IList<IList<object>> data)
    {
        try
        {
            // ヘッダー行をスキップ
            if (data.Count <= 1) return;

            for (var i = 1; i < data.Count; i++)
            {
                var row = data[i];
                if (row == null || row.Count < 2) continue;

                var cardId = row[0]?.ToString();
                if (string.IsNullOrEmpty(cardId)) continue;

                // スプレッドシートの行データをstring配列に変換
                var columns = new string[row.Count];
                for (var j = 0; j < row.Count; j++)
                {
                    columns[j] = row[j]?.ToString() ?? string.Empty;
                }

                var narrationData = CreateNarrationDataFromColumns(columns);
                _narrationCache[cardId] = narrationData;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CardNarrationService: データパースエラー - {e.Message}");
        }
    }

    /// <summary>
    /// カードのナレーションを取得
    /// </summary>
    private string GetNarration(string cardId, NarrationType type, PlayStyle playStyle)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("CardNarrationService: サービスが初期化されていません");
            return string.Empty;
        }

        if (_narrationCache.TryGetValue(cardId, out var narrationData))
        {
            return narrationData.GetNarration(type, playStyle);
        }

        return string.Empty;
    }

    /// <summary>
    /// カードのナレーションを取得（CardDataから）
    /// </summary>
    public string GetNarration(CardData cardData, NarrationType type, PlayStyle playStyle)
    {
        if (!cardData) return string.Empty;
        
        // スプレッドシートから取得
        return GetNarration(cardData.CardId, type, playStyle);
    }

    /// <summary>
    /// ローカルCSVファイルからデータを読み込む
    /// </summary>
    private async UniTask LoadFromLocalCsv()
    {
        try
        {
            var csvPath = Path.Combine(Application.streamingAssetsPath, LOCAL_CSV_FILENAME);
            
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL環境ではUnityWebRequestを使用
            var request = UnityWebRequest.Get(csvPath);
            await request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"CardNarrationService: CSVファイル未検出: {LOCAL_CSV_FILENAME}");
                return;
            }
            
            var csvText = request.downloadHandler.text;
#else
            // その他の環境では従来通りFileStreamを使用
            if (!File.Exists(csvPath))
            {
                Debug.LogWarning($"CardNarrationService: CSVファイル未検出: {LOCAL_CSV_FILENAME}");
                return;
            }
            
            var csvText = await File.ReadAllTextAsync(csvPath);
#endif
            
            ParseCsvData(csvText);
            Debug.Log($"CardNarrationService: CSVから{_narrationCache.Count}件読み込み");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CardNarrationService: CSV読み込みエラー - {e.Message}");
        }
    }
    
    /// <summary>
    /// CSVデータをパース
    /// </summary>
    private void ParseCsvData(string csvText)
    {
        try
        {
            var lines = csvText.Split('\n');
            if (lines.Length <= 1) return;
            
            // ヘッダー行をスキップ
            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                
                var columns = line.Split(',');
                if (columns.Length < 2) continue;
                
                var cardId = columns[0].Trim();
                if (string.IsNullOrEmpty(cardId)) continue;
                
                var narrationData = CreateNarrationDataFromColumns(columns);
                _narrationCache[cardId] = narrationData;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CardNarrationService: CSVパースエラー - {e.Message}");
        }
    }

    /// <summary>
    /// データをリロード
    /// </summary>
    public async UniTask ReloadAsync()
    {
        _isInitialized = false;
        _narrationCache.Clear();
        await InitializeAsync();
    }
}