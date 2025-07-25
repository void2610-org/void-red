using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

/// <summary>
/// セーブデータの管理を行うサービスクラス
/// </summary>
public class SaveDataManager
{
    private const string SAVE_FILE_NAME = "player_save_data.json";
    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

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
            
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL環境ではPlayerPrefsに保存
            PlayerPrefs.SetString("SaveData", json);
            PlayerPrefs.Save();
#else
            // その他の環境では従来通りファイルに保存
            File.WriteAllText(SaveFilePath, json);
#endif
            
            Debug.Log($"プレイヤーデータを保存しました: {saveData.GetStatsString()}");
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
            string json;
            
#if UNITY_WEBGL && !UNITY_EDITOR
            json = PlayerPrefs.GetString("SaveData", "");
            if (string.IsNullOrEmpty(json))return new PlayerSaveData();
#else
            if (!File.Exists(SaveFilePath)) return new PlayerSaveData();
            json = File.ReadAllText(SaveFilePath);
#endif
            
            var saveData = JsonUtility.FromJson<PlayerSaveData>(json);
            if (saveData == null) return new PlayerSaveData();
            
            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"セーブデータの読み込みに失敗しました: {e.Message}");
            Debug.Log("新規データを作成します。");
            return new PlayerSaveData();
        }
    }
    
    /// <summary>
    /// セーブファイルが存在するかチェック
    /// </summary>
    /// <returns>セーブファイルの存在有無</returns>
    public bool IsSaveFileExists()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return PlayerPrefs.HasKey("SaveData") && !string.IsNullOrEmpty(PlayerPrefs.GetString("SaveData", ""));
#else
        return File.Exists(SaveFilePath);
#endif
    }
    
    /// <summary>
    /// セーブファイルを削除（デバッグ用）
    /// </summary>
    /// <returns>削除が成功したかどうか</returns>
    public bool DeleteSaveFile()
    {
        try
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (PlayerPrefs.HasKey("SaveData"))
            {
                PlayerPrefs.DeleteKey("SaveData");
                PlayerPrefs.Save();
                return true;
            }
            
            return false;
#else
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                return true;
            }
            
            return false;
#endif
        }
        catch (Exception e)
        {
            Debug.LogError($"セーブファイルの削除に失敗しました: {e.Message}");
            return false;
        }
    }
}