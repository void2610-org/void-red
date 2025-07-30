using System;
using UnityEngine;

/// <summary>
/// セーブデータの管理を行うサービスクラス
/// </summary>
public class SaveDataManager
{
    private const string SAVE_DATA_KEY = "player_save_data";
    private readonly PersonalityLogService _personalityLogService;
    
    /// <summary>
    /// コンストラクタ（依存性注入）
    /// </summary>
    public SaveDataManager(PersonalityLogService personalityLogService)
    {
        _personalityLogService = personalityLogService;
    }

    /// <summary>
    /// PlayerSaveDataをファイルに保存
    /// </summary>
    /// <param name="saveData">保存するデータ</param>
    /// <returns>保存が成功したかどうか</returns>
    private bool SavePlayerData(PlayerSaveData saveData)
    {
        var json = JsonUtility.ToJson(saveData, true);
        var success = DataPersistence.SaveData(SAVE_DATA_KEY, json);
        
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
    /// すべてのデータを統合して保存
    /// </summary>
    /// <param name="playerData">プレイヤーセーブデータ</param>
    /// <returns>すべての保存が成功したかどうか</returns>
    public bool SaveAllData(PlayerSaveData playerData)
    {
        var playerSaveSuccess = SavePlayerData(playerData);
        var personalityLogSuccess = _personalityLogService.SavePersonalityLog();
        
        if (playerSaveSuccess && personalityLogSuccess)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// すべてのセーブファイルを削除（デバッグ用）
    /// </summary>
    /// <returns>すべての削除が成功したかどうか</returns>
    public bool DeleteAllSaveFiles()
    {
        var playerDataDeleted = DataPersistence.DeleteData(SAVE_DATA_KEY);
        var personalityLogDeleted = _personalityLogService.DeletePersonalityLog();
        
        return playerDataDeleted && personalityLogDeleted;
    }
    
    /// <summary>
    /// すべてのセーブファイルを削除し、データを再読み込み（デバッグ用）
    /// </summary>
    /// <returns>削除が成功したかどうか</returns>
    public bool DeleteAllSaveFilesAndReload()
    {
        var success = DeleteAllSaveFiles();
        if (success)
        {
            _personalityLogService.ReloadPersonalityLog();
        }
        return success;
    }
}