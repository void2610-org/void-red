using System;
using System.IO;
using UnityEngine;

/// <summary>
/// セーブデータの管理を行うサービスクラス
/// </summary>
public class SaveDataManager
{
    private const string SAVE_FILE_NAME = "player_save_data.json";
    private readonly string _saveFilePath;
    
    public SaveDataManager()
    {
        _saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    }
    
    /// <summary>
    /// PlayerSaveDataをファイルに保存
    /// </summary>
    /// <param name="saveData">保存するデータ</param>
    /// <returns>保存が成功したかどうか</returns>
    public bool SavePlayerData(PlayerSaveData saveData)
    {
        try
        {
            var json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(_saveFilePath, json);
            
            Debug.Log($"プレイヤーデータを保存しました: {_saveFilePath}");
            Debug.Log($"保存内容: {saveData.GetStatsString()}");
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"セーブデータの保存に失敗しました: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// ファイルからPlayerSaveDataを読み込み
    /// </summary>
    /// <returns>読み込んだデータ（存在しない場合や失敗時は新規データ）</returns>
    public PlayerSaveData LoadPlayerData()
    {
        try
        {
            if (!File.Exists(_saveFilePath))
            {
                Debug.Log("セーブファイルが存在しません。新規データを作成します。");
                return new PlayerSaveData();
            }
            
            var json = File.ReadAllText(_saveFilePath);
            var saveData = JsonUtility.FromJson<PlayerSaveData>(json);
            
            if (saveData == null)
            {
                Debug.LogWarning("セーブデータの読み込みに失敗しました。新規データを作成します。");
                return new PlayerSaveData();
            }
            
            Debug.Log($"プレイヤーデータを読み込みました: {saveData.GetStatsString()}");
            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"セーブデータの読み込みに失敗しました: {e.Message}");
            Debug.Log("新規データを作成します。");
            return new PlayerSaveData();
        }
    }
    
    /// セーブファイルが存在するかチェック
    /// </summary>
    /// <returns>セーブファイルの存在有無</returns>
    public bool SaveFileExists()
    {
        return File.Exists(_saveFilePath);
    }
    
    /// <summary>
    /// セーブファイルを削除（デバッグ用）
    /// </summary>
    /// <returns>削除が成功したかどうか</returns>
    public bool DeleteSaveFile()
    {
        try
        {
            if (File.Exists(_saveFilePath))
            {
                File.Delete(_saveFilePath);
                Debug.Log("セーブファイルを削除しました。");
                return true;
            }
            
            Debug.Log("削除対象のセーブファイルが存在しません。");
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"セーブファイルの削除に失敗しました: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// セーブファイルのパスを取得（デバッグ用）
    /// </summary>
    public string SaveFilePath => _saveFilePath;
}