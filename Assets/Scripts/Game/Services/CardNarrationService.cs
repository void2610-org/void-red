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

                var narrationData = new CardNarrationData();
                
                // プレイ前語り (列1-3)
                if (row.Count > 1 && !string.IsNullOrEmpty(row[1]?.ToString()))
                    narrationData.SetNarration(NarrationType.PrePlay, PlayStyle.Hesitation, row[1].ToString());
                if (row.Count > 2 && !string.IsNullOrEmpty(row[2]?.ToString()))
                    narrationData.SetNarration(NarrationType.PrePlay, PlayStyle.Impulse, row[2].ToString());
                if (row.Count > 3 && !string.IsNullOrEmpty(row[3]?.ToString()))
                    narrationData.SetNarration(NarrationType.PrePlay, PlayStyle.Conviction, row[3].ToString());
                
                // 勝負後語り (列4-6)
                if (row.Count > 4 && !string.IsNullOrEmpty(row[4]?.ToString()))
                    narrationData.SetNarration(NarrationType.PostBattle, PlayStyle.Hesitation, row[4].ToString());
                if (row.Count > 5 && !string.IsNullOrEmpty(row[5]?.ToString()))
                    narrationData.SetNarration(NarrationType.PostBattle, PlayStyle.Impulse, row[5].ToString());
                if (row.Count > 6 && !string.IsNullOrEmpty(row[6]?.ToString()))
                    narrationData.SetNarration(NarrationType.PostBattle, PlayStyle.Conviction, row[6].ToString());
                
                // 勝負後語り（敵） (列7-9)
                if (row.Count > 7 && !string.IsNullOrEmpty(row[7]?.ToString()))
                    narrationData.SetNarration(NarrationType.PostBattleEnemy, PlayStyle.Hesitation, row[7].ToString());
                if (row.Count > 8 && !string.IsNullOrEmpty(row[8]?.ToString()))
                    narrationData.SetNarration(NarrationType.PostBattleEnemy, PlayStyle.Impulse, row[8].ToString());
                if (row.Count > 9 && !string.IsNullOrEmpty(row[9]?.ToString()))
                    narrationData.SetNarration(NarrationType.PostBattleEnemy, PlayStyle.Conviction, row[9].ToString());

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
            
            var csvText = File.ReadAllText(csvPath);
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
                
                var narrationData = new CardNarrationData();
                
                // プレイ前語り (列1-3)
                if (columns.Length > 1 && !string.IsNullOrEmpty(columns[1].Trim()))
                    narrationData.SetNarration(NarrationType.PrePlay, PlayStyle.Hesitation, columns[1].Trim());
                if (columns.Length > 2 && !string.IsNullOrEmpty(columns[2].Trim()))
                    narrationData.SetNarration(NarrationType.PrePlay, PlayStyle.Impulse, columns[2].Trim());
                if (columns.Length > 3 && !string.IsNullOrEmpty(columns[3].Trim()))
                    narrationData.SetNarration(NarrationType.PrePlay, PlayStyle.Conviction, columns[3].Trim());
                
                // 勝負後語り (列4-6)
                if (columns.Length > 4 && !string.IsNullOrEmpty(columns[4].Trim()))
                    narrationData.SetNarration(NarrationType.PostBattle, PlayStyle.Hesitation, columns[4].Trim());
                if (columns.Length > 5 && !string.IsNullOrEmpty(columns[5].Trim()))
                    narrationData.SetNarration(NarrationType.PostBattle, PlayStyle.Impulse, columns[5].Trim());
                if (columns.Length > 6 && !string.IsNullOrEmpty(columns[6].Trim()))
                    narrationData.SetNarration(NarrationType.PostBattle, PlayStyle.Conviction, columns[6].Trim());
                
                // 勝負後語り（敵） (列7-9)
                if (columns.Length > 7 && !string.IsNullOrEmpty(columns[7].Trim()))
                    narrationData.SetNarration(NarrationType.PostBattleEnemy, PlayStyle.Hesitation, columns[7].Trim());
                if (columns.Length > 8 && !string.IsNullOrEmpty(columns[8].Trim()))
                    narrationData.SetNarration(NarrationType.PostBattleEnemy, PlayStyle.Impulse, columns[8].Trim());
                if (columns.Length > 9 && !string.IsNullOrEmpty(columns[9].Trim()))
                    narrationData.SetNarration(NarrationType.PostBattleEnemy, PlayStyle.Conviction, columns[9].Trim());
                
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