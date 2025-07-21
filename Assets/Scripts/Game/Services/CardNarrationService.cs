using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Googleスプレッドシートからカードのナレーション内容を管理するサービス
/// </summary>
public class CardNarrationService
{
    // スプレッドシートID（環境変数や設定ファイルから読み込むことを推奨）
    private const string SPREADSHEET_ID = "1Yj-f13peW3dxumUhcSdPNRZ5if5ogz9aXM2cB19bSgY";
    private const string SHEET_NAME = "main";
    
    // カードIDをキーとしたナレーションデータのキャッシュ
    private readonly Dictionary<string, CardNarrationData> _narrationCache = new();
    private bool _isInitialized = false;

    /// <summary>
    /// サービスの初期化（スプレッドシートからデータを読み込む）
    /// </summary>
    public async UniTask InitializeAsync()
    {
        if (_isInitialized) return;
        
        try
        {
            var data = await GoogleSpreadSheetService.GetSheet(SPREADSHEET_ID, SHEET_NAME);
            if (data == null || data.Count == 0)
            {
                Debug.LogWarning("CardNarrationService: スプレッドシートからデータを取得できませんでした");
                _isInitialized = true;
                return;
            }

            ParseSpreadsheetData(data);
            _isInitialized = true;
            Debug.Log($"CardNarrationService: {_narrationCache.Count}個のカードの語りを読み込みました");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CardNarrationService: 初期化中にエラーが発生しました: {e.Message}");
            _isInitialized = true;
        }
    }

    /// <summary>
    /// スプレッドシートのデータをパース
    /// 想定形式: CardID | PrePlay_Hesitation | PrePlay_Impulse | PrePlay_Conviction | PostBattle_Hesitation | PostBattle_Impulse | PostBattle_Conviction | PostBattleEnemy_Hesitation | PostBattleEnemy_Impulse | PostBattleEnemy_Conviction
    /// </summary>
    private void ParseSpreadsheetData(IList<IList<object>> data)
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
        
        // まずスプレッドシートから取得を試みる
        var spreadsheetNarration = GetNarration(cardData.CardId, type, playStyle);
        if (!string.IsNullOrEmpty(spreadsheetNarration))
        {
            return spreadsheetNarration;
        }
        
        // スプレッドシートに無い場合で、PostBattleの場合のみカードデータ自体の語り文を返す
        if (type == NarrationType.PostBattle)
        {
            return cardData.GetNarration(playStyle);
        }
        
        return string.Empty;
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