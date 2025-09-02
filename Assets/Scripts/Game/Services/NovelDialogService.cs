using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;

namespace VoidRed.Game.Services
{
    /// <summary>
    /// ノベル台詞データをスプレッドシートから読み込み、管理するサービス
    /// GoogleSpreadSheetServiceを使用してデータを取得する
    /// </summary>
    public class NovelDialogService
    {
        private const string SPREADSHEET_ID = "1cPwaMiTwriP5eGhxYqIYvdpjJ81xsnwDdBtULMlP82I";
        // シート名は動的にシナリオIDを使用するため、SHEET_NAME定数は削除
        
        private readonly Dictionary<string, List<DialogData>> _dialogCache = new();
        private readonly HashSet<string> _loadedScenarios = new(); // 読み込み済みシナリオを追跡
        private bool _isInitialized = false;

        /// <summary>
        /// サービスの初期化（基本設定のみ）
        /// 実際のデータ読み込みは GetDialogsByScenarioId で遅延ロード
        /// </summary>
        public async UniTask InitializeAsync()
        {
            if (_isInitialized) return;
            
            Debug.Log($"[NovelDialogService] 初期化開始");
            Debug.Log($"[NovelDialogService] スプレッドシートID: {SPREADSHEET_ID}");
            
            // 認証状況をチェック
            var authSuccess = CheckAuthenticationStatus();
            
            // 認証に失敗した場合のみフォールバックデータを読み込み
            if (!authSuccess)
            {
                Debug.Log("[NovelDialogService] 認証失敗のため、フォールバックデータを使用");
                LoadFallbackData();
            }
            else
            {
                Debug.Log("[NovelDialogService] 認証成功、スプレッドシートからデータを取得します");
            }
            
            _isInitialized = true;
            
            Debug.Log("[NovelDialogService] サービスを初期化しました（遅延ロード方式）");
        }

        /// <summary>
        /// Google認証の状況をチェック
        /// </summary>
        private bool CheckAuthenticationStatus()
        {
            try
            {
                var scopes = new[] { "https://www.googleapis.com/auth/spreadsheets.readonly" };
                var credential = GoogleAuthService.GetCredential(scopes);
                
                if (credential != null)
                {
                    Debug.Log("[NovelDialogService] Google認証: 成功");
                    return true;
                }
                else
                {
                    Debug.LogWarning("[NovelDialogService] Google認証: 失敗 - 認証ファイルが見つからないか、権限が不足しています");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[NovelDialogService] 認証チェックエラー: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// フォールバック用のダミーデータを読み込み
        /// </summary>
        private void LoadFallbackData()
        {
            Debug.Log("NovelDialogService: フォールバックデータを使用");
            
            // プロローグ用フォールバックデータ
            var prologueDialogs = new List<DialogData>
            {
                DialogData.CreateFromSpreadsheetData("ナレーター", "物語の始まり...", "", "", true, 0.05f, false),
                DialogData.CreateFromSpreadsheetData("主人公", "新たな冒険が始まる！", "", "", true, 0.03f, false),
                DialogData.CreateFromSpreadsheetData("ナレーター", "これはオフライン用のデータです。", "", "", true, 0.04f, false)
            };
            _dialogCache["prologue"] = prologueDialogs;
            
            // エンディング用フォールバックデータ
            var endingDialogs = new List<DialogData>
            {
                DialogData.CreateFromSpreadsheetData("ナレーター", "長い冒険が終わりを迎える...", "", "", true, 0.05f, false),
                DialogData.CreateFromSpreadsheetData("主人公", "ありがとう、みんな！", "", "", true, 0.03f, false),
                DialogData.CreateFromSpreadsheetData("システム", "Game Clear!", "", "", true, 0.02f, false)
            };
            _dialogCache["ending"] = endingDialogs;
            
            // テスト用データ
            var testDialogs = new List<DialogData>
            {
                DialogData.CreateFromSpreadsheetData("ナレーター", "これはテスト用の台詞です。", "", "", true, 0.05f, false),
                DialogData.CreateFromSpreadsheetData("キャラクター1", "こんにちは！", "", "", true, 0.03f, false),
                DialogData.CreateFromSpreadsheetData("キャラクター2", "よろしくお願いします！", "", "", true, 0.04f, false)
            };
            _dialogCache["test_001"] = testDialogs;
        }

        /// <summary>
        /// 指定されたシナリオIDの台詞リストを取得（遅延ロード対応）
        /// </summary>
        public async UniTask<List<DialogData>> GetDialogsByScenarioIdAsync(string scenarioId)
        {
            Debug.Log($"[NovelDialogService] シナリオ '{scenarioId}' の取得を開始");
            
            if (!_isInitialized)
            {
                Debug.LogWarning("NovelDialogService: サービスが初期化されていません");
                return new List<DialogData>();
            }

            // 既にキャッシュされている場合はそれを返す
            if (_dialogCache.TryGetValue(scenarioId, out var cachedDialogs))
            {
                Debug.Log($"[NovelDialogService] シナリオ '{scenarioId}' をキャッシュから取得 ({cachedDialogs.Count}行)");
                return cachedDialogs;
            }

            // スプレッドシートから該当シナリオのシートを読み込み
            await LoadScenarioFromSpreadsheet(scenarioId);

            // 再度キャッシュから取得
            var result = _dialogCache.TryGetValue(scenarioId, out var dialogs) ? dialogs : new List<DialogData>();
            Debug.Log($"[NovelDialogService] シナリオ '{scenarioId}' 最終結果: {result.Count}行");
            return result;
        }

        /// <summary>
        /// 指定されたシナリオIDの台詞リストを取得（同期版・既存互換性）
        /// </summary>
        public List<DialogData> GetDialogsByScenarioId(string scenarioId)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("NovelDialogService: サービスが初期化されていません");
                return new List<DialogData>();
            }
            
            return _dialogCache.TryGetValue(scenarioId, out var dialogs) ? dialogs : new List<DialogData>();
        }

        /// <summary>
        /// スプレッドシートから特定のシナリオデータを読み込み
        /// </summary>
        private async UniTask LoadScenarioFromSpreadsheet(string scenarioId)
        {
            // 既に読み込み済みの場合はスキップ
            if (_loadedScenarios.Contains(scenarioId))
            {
                Debug.Log($"[NovelDialogService] シナリオ '{scenarioId}' は既にキャッシュ済み");
                return;
            }

#if !UNITY_WEBGL || UNITY_EDITOR
            try
            {
                Debug.Log($"[NovelDialogService] スプレッドシート読み込み開始");
                Debug.Log($"[NovelDialogService] - スプレッドシートID: {SPREADSHEET_ID}");
                Debug.Log($"[NovelDialogService] - シート名: {scenarioId}");
                
                // シナリオIDをシート名として使用
                var data = await GoogleSpreadSheetService.GetSheet(SPREADSHEET_ID, scenarioId);
                
                if (data != null && data.Count > 0)
                {
                    Debug.Log($"[NovelDialogService] スプレッドシートからデータ取得成功: {data.Count}行");
                    ParseSingleScenarioData(scenarioId, data);
                    _loadedScenarios.Add(scenarioId);
                    
                    var dialogCount = _dialogCache.TryGetValue(scenarioId, out var dialogs) ? dialogs.Count : 0;
                    Debug.Log($"[NovelDialogService] シナリオ '{scenarioId}' から {dialogCount} 件の台詞を読み込み");
                }
                else
                {
                    Debug.LogWarning($"[NovelDialogService] スプレッドシートが空またはアクセスできません");
                    Debug.LogWarning($"[NovelDialogService] - data is null: {data == null}");
                    Debug.LogWarning($"[NovelDialogService] - data.Count: {data?.Count ?? 0}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[NovelDialogService] スプレッドシート読み込みエラー: {e.GetType().Name}");
                Debug.LogError($"[NovelDialogService] エラーメッセージ: {e.Message}");
                if (e.InnerException != null)
                {
                    Debug.LogError($"[NovelDialogService] 内部エラー: {e.InnerException.Message}");
                }
            }
#else
            Debug.Log($"NovelDialogService: WebGL環境のため、シナリオ '{scenarioId}' はフォールバックデータを使用");
#endif
        }

        /// <summary>
        /// 単一シナリオのスプレッドシートデータをパース
        /// 想定形式: SpeakerName | DialogText | CharacterImagePath | SEAudioPath | DisplaySpeed
        /// </summary>
        private void ParseSingleScenarioData(string scenarioId, IList<IList<object>> data)
        {
            try
            {
                var dialogList = new List<DialogData>();

                // ヘッダー行をスキップ
                var startRow = data.Count > 0 && data[0] != null && data[0].Count > 0 && 
                              (data[0][0]?.ToString()?.Contains("Speaker") == true || 
                               data[0][0]?.ToString()?.Contains("話者") == true) ? 1 : 0;

                for (var i = startRow; i < data.Count; i++)
                {
                    var row = data[i];
                    if (row == null || row.Count < 2) continue;

                    var speakerName = row[0]?.ToString() ?? "";
                    var dialogText = row[1]?.ToString() ?? "";
                    
                    if (string.IsNullOrEmpty(dialogText)) continue;

                    // オプション項目
                    var characterImagePath = row.Count > 2 ? row[2]?.ToString() : "";
                    var seAudioPath = row.Count > 3 ? row[3]?.ToString() : "";
                    var displaySpeed = row.Count > 4 ? ParseFloat(row[4]?.ToString(), 0.05f) : 0.05f;

                    var dialogData = DialogData.CreateFromSpreadsheetData(
                        speakerName, 
                        dialogText, 
                        characterImagePath, 
                        seAudioPath, 
                        true, // playSeOnStart
                        displaySpeed, // customCharSpeed
                        false // autoAdvance
                    );

                    dialogList.Add(dialogData);
                }

                _dialogCache[scenarioId] = dialogList;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"NovelDialogService: シナリオ '{scenarioId}' のデータパースエラー - {e.Message}");
            }
        }
        /// </summary>
        public IEnumerable<string> GetAvailableScenarioIds()
        {
            return _dialogCache.Keys;
        }

        /// <summary>
        /// サービスが初期化済みかどうか
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// 文字列を浮動小数点数にパース（失敗時はデフォルト値を返す）
        /// </summary>
        private static float ParseFloat(string value, float defaultValue)
        {
            return float.TryParse(value, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// キャッシュをクリア（デバッグ用）
        /// </summary>
        public void ClearCache()
        {
            _dialogCache.Clear();
            _isInitialized = false;
        }

        /// <summary>
        /// デバッグ用：読み込まれたシナリオIDを全て表示
        /// </summary>
        public void LogAvailableScenarios()
        {
            Debug.Log($"[NovelDialogService] 利用可能なシナリオID一覧:");
            foreach (var scenarioId in _dialogCache.Keys)
            {
                var count = _dialogCache[scenarioId].Count;
                Debug.Log($"  - {scenarioId} ({count}件の台詞)");
            }
        }

        /// <summary>
        /// デバッグ用：スプレッドシート接続テスト
        /// </summary>
        public async UniTask TestSpreadsheetConnection()
        {
            Debug.Log("[NovelDialogService] スプレッドシート接続テスト開始");
            
            try
            {
                var data = await GoogleSpreadSheetService.GetSheet(SPREADSHEET_ID, "prologue");
                
                if (data != null && data.Count > 0)
                {
                    Debug.Log($"[NovelDialogService] 接続テスト成功! 取得行数: {data.Count}");
                    for (int i = 0; i < Mathf.Min(3, data.Count); i++)
                    {
                        var row = data[i];
                        var rowData = string.Join(" | ", row);
                        Debug.Log($"  行{i + 1}: {rowData}");
                    }
                }
                else
                {
                    Debug.LogWarning("[NovelDialogService] 接続テスト失敗: データが空またはnull");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[NovelDialogService] 接続テストエラー: {e.Message}");
            }
        }
    }
}
