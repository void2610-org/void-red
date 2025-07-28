using System;
using UnityEngine;

/// <summary>
/// セーブデータの管理を行うサービスクラス
/// </summary>
public class SaveDataManager
{
    private const string SAVE_DATA_KEY = "player_save_data";

    /// <summary>
    /// PlayerSaveDataをファイルに保存
    /// </summary>
    /// <param name="saveData">保存するデータ</param>
    /// <returns>保存が成功したかどうか</returns>
    public bool SavePlayerData(PlayerSaveData saveData)
    {
        var json = JsonUtility.ToJson(saveData, true);
        var success = DataPersistence.SaveData(SAVE_DATA_KEY, json);
        
        if (success)
        {
            Debug.Log($"プレイヤーデータを保存しました: {saveData.GetStatsString()}");
        }
        
        return success;
    }
    
    /// <summary>
    /// ファイルからPlayerSaveDataを読み込み
    /// </summary>
    /// <returns>読み込んだデータ（存在しない場合や失敗時は新規データ）</returns>
    public PlayerSaveData LoadPlayerData()
    {
        var json = DataPersistence.LoadData(SAVE_DATA_KEY);
        
        if (string.IsNullOrEmpty(json))
        {
            return new PlayerSaveData();
        }
        
        var saveData = JsonUtility.FromJson<PlayerSaveData>(json);
        return saveData ?? new PlayerSaveData();
    }
    
    /// <summary>
    /// セーブファイルが存在するかチェック
    /// </summary>
    /// <returns>セーブファイルの存在有無</returns>
    public bool IsSaveFileExists()
    {
        return DataPersistence.DataExists(SAVE_DATA_KEY);
    }
    
    /// <summary>
    /// セーブファイルを削除（デバッグ用）
    /// </summary>
    /// <returns>削除が成功したかどうか</returns>
    public bool DeleteSaveFile()
    {
        return DataPersistence.DeleteData(SAVE_DATA_KEY);
    }
}