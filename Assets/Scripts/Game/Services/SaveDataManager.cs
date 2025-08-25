using System;
using UnityEngine;

/// <summary>
/// セーブデータの管理を行うサービスクラス
/// </summary>
public class SaveDataManager
{
    private const string SAVE_DATA_KEY = "game_save_data";
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public SaveDataManager()
    {
    }

    /// <summary>
    /// GameSaveDataをファイルに保存
    /// </summary>
    /// <param name="saveData">保存するデータ</param>
    /// <returns>保存が成功したかどうか</returns>
    public bool SaveGameData(GameSaveData saveData)
    {
        try
        {
            var json = JsonUtility.ToJson(saveData, true);
            var success = DataPersistence.SaveData(SAVE_DATA_KEY, json);
            
            if (success) Debug.Log($"[SaveDataManager] ゲームデータセーブ成功: {saveData.GetDebugInfo()}");
            else Debug.LogError("[SaveDataManager] ゲームデータセーブ失敗");
            
            return success;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveDataManager] セーブ中にエラーが発生: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// ファイルからGameSaveDataを読み込み
    /// </summary>
    /// <returns>読み込んだデータ（存在しない場合や失敗時は新規データ）</returns>
    public GameSaveData LoadGameData()
    {
        try
        {
            var json = DataPersistence.LoadData(SAVE_DATA_KEY);
            
            if (string.IsNullOrEmpty(json))
            {
                Debug.Log("[SaveDataManager] セーブファイルが存在しないため新規データを作成");
                return new GameSaveData();
            }
            
            var saveData = JsonUtility.FromJson<GameSaveData>(json);
            if (saveData != null)
            {
                Debug.Log($"[SaveDataManager] ゲームデータロード成功: {saveData.GetDebugInfo()}");
                return saveData;
            }
            else
            {
                Debug.LogWarning("[SaveDataManager] セーブデータの解析に失敗、新規データを作成");
                return new GameSaveData();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveDataManager] ロード中にエラーが発生: {ex.Message}");
            return new GameSaveData();
        }
    }
    
    /// <summary>
    /// セーブファイルが存在するかチェック
    /// </summary>
    /// <returns>セーブファイルの存在有無</returns>
    public bool SaveFileExists()
    {
        return DataPersistence.DataExists(SAVE_DATA_KEY);
    }
    
    /// <summary>
    /// セーブファイルを削除（デバッグ用）
    /// </summary>
    /// <returns>削除が成功したかどうか</returns>
    public bool DeleteSaveFile()
    {
        try
        {
            // 存在確認
            if (!SaveFileExists())
            {
                Debug.Log("[SaveDataManager] セーブファイルが存在しないため削除不要");
                return true; // 削除済みとみなす
            }
            
            var success = DataPersistence.DeleteData(SAVE_DATA_KEY);
            
            if (success) Debug.Log("[SaveDataManager] セーブファイル削除成功");
            else Debug.LogWarning("[SaveDataManager] セーブファイル削除に失敗しました");
            
            return success;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveDataManager] セーブファイル削除中にエラーが発生: {ex.Message}");
            return false;
        }
    }
}