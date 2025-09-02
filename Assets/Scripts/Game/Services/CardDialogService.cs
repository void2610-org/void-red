using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;

namespace VoidRed.Game.Services
{
    /// <summary>
    /// カード関連の台詞データをスプレッドシートから読み込み、管理するサービス
    /// GoogleSpreadSheetServiceを使用してデータを取得する
    /// </summary>
    public class CardDialogService
    {
        private const string SPREADSHEET_ID = "1cPwaMiTwriP5eGhxYqIYvdpjJ81xsnwDdBtULMlP82I"; // TODO: 実際のスプレッドシートIDに変更
        private const string SHEET_NAME = "Dialog"; // シート名
        
        private readonly Dictionary<string, List<DialogData>> _dialogCache = new();
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
                    Debug.Log($"CardDialogService: スプレッドシートから{_dialogCache.Sum(x => x.Value.Count)}件の台詞を読み込み");
                    return;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"CardDialogService: Google Sheets API失敗 - {e.Message}");
            }
#endif
            
            // WebGL環境、またはGoogle Sheets APIが失敗した場合、ローカルデータまたはダミーデータを使用
            LoadFallbackData();
            _isInitialized = true;
        }

        /// <summary>
        /// スプレッドシートのデータをパース
        /// 想定形式: ScenarioID | SpeakerName | DialogText | CharacterImagePath | SEAudioPath | DisplaySpeed
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
                    if (row == null || row.Count < 3) continue; // 最低限ScenarioID, SpeakerName, DialogTextが必要

                    var scenarioId = row[0]?.ToString();
                    if (string.IsNullOrEmpty(scenarioId)) continue;

                    var speakerName = row[1]?.ToString() ?? "";
                    var dialogText = row[2]?.ToString() ?? "";
                    
                    if (string.IsNullOrEmpty(dialogText)) continue;

                    // オプション項目
                    var characterImagePath = row.Count > 3 ? row[3]?.ToString() : "";
                    var seAudioPath = row.Count > 4 ? row[4]?.ToString() : "";
                    var displaySpeed = row.Count > 5 ? ParseFloat(row[5]?.ToString(), 0.05f) : 0.05f;

                    var dialogData = DialogData.CreateFromSpreadsheetData(
                        speakerName, 
                        dialogText, 
                        characterImagePath, 
                        seAudioPath, 
                        true, // playSeOnStart
                        displaySpeed, // customCharSpeed
                        false // autoAdvance
                    );

                    // シナリオIDごとにグループ化
                    if (!_dialogCache.ContainsKey(scenarioId))
                    {
                        _dialogCache[scenarioId] = new List<DialogData>();
                    }
                    
                    _dialogCache[scenarioId].Add(dialogData);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CardDialogService: データパースエラー - {e.Message}");
            }
        }

        /// <summary>
        /// フォールバック用のダミーデータを読み込み
        /// </summary>
        private void LoadFallbackData()
        {
            Debug.Log("CardDialogService: フォールバックデータを使用");
            
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
        /// 指定されたシナリオIDの台詞リストを取得
        /// </summary>
        public List<DialogData> GetDialogsByScenarioId(string scenarioId)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("CardDialogService: サービスが初期化されていません");
                return new List<DialogData>();
            }

            return _dialogCache.TryGetValue(scenarioId, out var dialogs) ? dialogs : new List<DialogData>();
        }

        /// <summary>
        /// 利用可能なシナリオIDのリストを取得
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
            Debug.Log($"CardDialogService: 利用可能なシナリオID一覧:");
            foreach (var scenarioId in _dialogCache.Keys)
            {
                var count = _dialogCache[scenarioId].Count;
                Debug.Log($"  - {scenarioId} ({count}件の台詞)");
            }
        }
    }
}
