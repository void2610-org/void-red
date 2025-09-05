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
        
        private readonly Dictionary<string, List<DialogData>> _dialogCache = new();
        private readonly HashSet<string> _loadedScenarios = new();
        private bool _isInitialized = false;

        public async UniTask InitializeAsync()
        {
            if (_isInitialized) return;
            _isInitialized = true;
        }

        /// <summary>
        /// 指定されたシナリオIDの台詞リストを取得
        /// </summary>
        public async UniTask<List<DialogData>> GetDialogsByScenarioIdAsync(string scenarioId)
        {
            if (!_isInitialized) return new List<DialogData>();

            if (_dialogCache.TryGetValue(scenarioId, out var cachedDialogs))
            {
                return cachedDialogs;
            }

            await LoadScenarioFromSpreadsheet(scenarioId);
            return _dialogCache.TryGetValue(scenarioId, out var dialogs) ? dialogs : new List<DialogData>();
        }

        private async UniTask LoadScenarioFromSpreadsheet(string scenarioId)
        {
            if (_loadedScenarios.Contains(scenarioId)) return;

            try
            {
                var data = await GoogleSpreadSheetService.GetSheet(SPREADSHEET_ID, scenarioId);
                
                if (data != null && data.Count > 0)
                {
                    ParseSingleScenarioData(scenarioId, data);
                    _loadedScenarios.Add(scenarioId);
                }
            }
            catch (System.Exception ex)
            {
                // エラー時はキャッシュにないシナリオとして扱う
            }
        }

        private void ParseSingleScenarioData(string scenarioId, IList<IList<object>> data)
        {
            var dialogs = new List<DialogData>();
            
            // ヘッダー行をスキップして2行目から処理
            for (int i = 1; i < data.Count; i++)
            {
                var row = data[i];
                if (row.Count < 3) continue; // 最低限、話者名・台詞・キャラクター画像名が必要

                var speakerName = row.Count > 0 ? row[0].ToString() : "";
                var dialogText = row.Count > 1 ? row[1].ToString() : "";
                var characterImageName = row.Count > 2 ? row[2].ToString() : "";
                var seClipName = row.Count > 3 ? row[3].ToString() : "";

                // 空行をスキップ
                if (string.IsNullOrEmpty(speakerName) && string.IsNullOrEmpty(dialogText))
                    continue;

                // SE設定
                bool hasSe = !string.IsNullOrEmpty(seClipName);
                bool playSeOnStart = row.Count > 4 ? ParseBool(row[4].ToString()) : true;

                // 文字送り速度設定
                bool useDefaultCharSpeed = row.Count > 5 ? ParseBool(row[5].ToString()) : true;
                float customCharSpeed = row.Count > 6 ? ParseFloat(row[6].ToString(), 0.05f) : 0.05f;

                // 自動進行設定
                bool autoAdvance = false; // デフォルトはfalse（手動進行）

                var dialog = DialogData.CreateFromSpreadsheetData(
                    speakerName,
                    dialogText,
                    characterImageName,
                    seClipName,
                    playSeOnStart,
                    useDefaultCharSpeed ? -1f : customCharSpeed,
                    autoAdvance
                );

                dialogs.Add(dialog);
            }

            _dialogCache[scenarioId] = dialogs;
        }

        public IEnumerable<string> GetAvailableScenarioIds()
        {
            return _dialogCache.Keys;
        }

        private static bool ParseBool(string value)
        {
            return value.ToLower() == "true" || value == "1";
        }

        private static float ParseFloat(string value, float defaultValue)
        {
            return float.TryParse(value, out var result) ? result : defaultValue;
        }

        public void ClearCache()
        {
            _dialogCache.Clear();
            _loadedScenarios.Clear();
        }
    }
}
