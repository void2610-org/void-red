using System;
using System.Collections.Generic;
using System.Linq;
using MackySoft.SerializeReferenceExtensions;
using R3;
using UnityEngine;
using VContainer;
using Void2610.UnityTemplate;

/// <summary>
/// ゲーム設定の管理を行うサービスクラス
/// VContainerでシングルトンとして注入される
/// 
/// 使用例:
/// var setting = new SliderSetting("SE音量", "効果音の音量", 0.8f, 0f, 1f);
/// setting.OnValueChanged.Subscribe(_ => SeManager.Instance.SeVolume = setting.CurrentValue).AddTo(_disposables);
/// settingsManager.AddSetting(setting);
/// </summary>
[System.Serializable]
public class SettingsManager
{
    [SerializeReference, SubclassSelector]
    private List<ISettingBase> settings = new();
    
    private readonly Subject<string> _onSettingChanged = new();
    private readonly CompositeDisposable _disposables = new();
    
    private SaveDataManager _saveDataManager;
    
    /// <summary>
    /// 設定値が変更された時のイベント（設定名を通知）
    /// </summary>
    public Observable<string> OnSettingChanged => _onSettingChanged;
    
    /// <summary>
    /// 全ての設定項目の読み取り専用リスト
    /// </summary>
    public IReadOnlyList<ISettingBase> Settings => settings.AsReadOnly();
    
    public SettingsManager(SaveDataManager saveDataManager)
    {
        _saveDataManager = saveDataManager;
        
        // 既存の設定がない場合は初期設定を作成
        if (settings.Count == 0) InitializeDefaultSettings();
        
        // 各設定の値変更イベントを監視
        SubscribeToSettingChanges();
        // セーブデータから設定を読み込み（初期適用はスキップ）
        LoadSettings(applyValues: false);
    }
    
    /// <summary>
    /// デフォルト設定を初期化
    /// OnValueChanged.Subscribe でシステムに直接反映するパターン
    /// </summary>
    private void InitializeDefaultSettings()
    {
        // TODO: BgmManagerを移植
        // BGM音量設定
        var bgmSetting = new SliderSetting("BGM音量", "バックグラウンドミュージックの音量", 0.8f, 0f, 1f);
        bgmSetting.OnValueChanged.Subscribe(v => Debug.Log($"BGM音量を {v} に設定")).AddTo(_disposables);
        settings.Add(bgmSetting);
        
        // SE音量設定（SeManager初期化後に適用）
        var seSetting = new SliderSetting("SE音量", "効果音の音量", 0.8f, 0f, 1f);
        seSetting.OnValueChanged.Subscribe(v => {
            if (SeManager.Instance != null)
            {
                SeManager.Instance.SeVolume = v;
            }
            else
            {
                Debug.Log($"SeManager未初期化のため、SE音量設定を遅延: {v}");
            }
        }).AddTo(_disposables);
        settings.Add(seSetting);
        
        // フルスクリーン切り替え
        var fullscreenSetting = new EnumSetting("フルスクリーン", "フルスクリーン表示の切り替え", 
            new[] { "false", "true" }, Screen.fullScreen ? "true" : "false", new[] { "オフ", "オン" });
        fullscreenSetting.OnValueChanged.Subscribe(v => Screen.fullScreen = v == "true").AddTo(_disposables);
        settings.Add(fullscreenSetting);
        
        // セーブデータ削除ボタン
        var deleteDataSetting = new ButtonSetting(
            "セーブデータ削除", 
            "プレイヤーのセーブデータを完全に削除します", 
            "削除する", 
            true, 
            "本当にセーブデータを削除しますか？この操作は元に戻せません。"
        );
        deleteDataSetting.ButtonAction = () => _saveDataManager.DeleteSaveFile();
        settings.Add(deleteDataSetting);
    }
    
    /// <summary>
    /// 各設定の値変更イベントを監視（通知とセーブ処理）
    /// </summary>
    private void SubscribeToSettingChanges()
    {
        foreach (var setting in settings)
        {
            setting.OnSettingChanged
                .Subscribe(_ => {
                    _onSettingChanged.OnNext(setting.SettingName);
                    SaveSettings(); // 設定変更時に自動保存
                })
                .AddTo(_disposables);
        }
    }
    
    /// <summary>
    /// 設定名で設定項目を取得
    /// </summary>
    public T GetSetting<T>(string settingName) where T : class, ISettingBase
    {
        return settings.FirstOrDefault(s => s.SettingName == settingName) as T;
    }
    
    /// <summary>
    /// すべての設定をデフォルト値にリセット
    /// </summary>
    public void ResetAllSettings()
    {
        foreach (var setting in settings)
        {
            setting.ResetToDefault();
        }
        
        Debug.Log("すべての設定をデフォルト値にリセットしました");
    }
    
    /// <summary>
    /// 設定データをファイルに保存
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            var settingsData = new SettingsData();
            
            foreach (var setting in settings)
            {
                settingsData.settingValues[setting.SettingName] = setting.SerializeValue();
            }
            
            var json = JsonUtility.ToJson(settingsData, true);
            System.IO.File.WriteAllText(GetSettingsFilePath(), json);
            
            Debug.Log("設定データを保存しました");
        }
        catch (Exception e)
        {
            Debug.LogError($"設定データの保存に失敗しました: {e.Message}");
        }
    }
    
    /// <summary>
    /// ファイルから設定データを読み込み
    /// </summary>
    /// <param name="applyValues">読み込み後に設定値を適用するかどうか</param>
    public void LoadSettings(bool applyValues = true)
    {
        try
        {
            var filePath = GetSettingsFilePath();
            
            if (!System.IO.File.Exists(filePath))
            {
                Debug.Log("設定ファイルが存在しません。デフォルト設定を使用します。");
                if (applyValues) ApplyCurrentValues();
                return;
            }
            
            var json = System.IO.File.ReadAllText(filePath);
            var settingsData = JsonUtility.FromJson<SettingsData>(json);
            
            if (settingsData?.settingValues != null)
            {
                foreach (var setting in settings)
                {
                    if (settingsData.settingValues.TryGetValue(setting.SettingName, out var value))
                    {
                        setting.DeserializeValue(value);
                    }
                }
                
                Debug.Log("設定データを読み込みました");
            }
            
            if (applyValues) ApplyCurrentValues();
        }
        catch (Exception e)
        {
            Debug.LogError($"設定データの読み込みに失敗しました: {e.Message}");
            if (applyValues) ApplyCurrentValues();
        }
    }
    
    /// <summary>
    /// 現在の設定値を適用
    /// </summary>
    private void ApplyCurrentValues()
    {
        foreach (var setting in settings)
        {
            setting.ApplyCurrentValue();
        }
    }
    
    /// <summary>
    /// 外部システム初期化後に全設定を適用
    /// </summary>
    public void ApplyAllSettingsWhenReady()
    {
        // SE音量設定の適用
        var seSetting = GetSetting<SliderSetting>("SE音量");
        if (seSetting != null && SeManager.Instance != null)
        {
            SeManager.Instance.SeVolume = seSetting.CurrentValue;
            Debug.Log($"SeManager初期化後にSE音量を適用: {seSetting.CurrentValue}");
        }
        
        // 他の設定も安全に適用
        var fullscreenSetting = GetSetting<EnumSetting>("フルスクリーン");
        if (fullscreenSetting != null)
        {
            Screen.fullScreen = fullscreenSetting.CurrentValue == "true";
            Debug.Log($"フルスクリーン設定を適用: {fullscreenSetting.CurrentValue}");
        }
        
        // BGM設定は既にデバッグログ出力のみなので安全
        Debug.Log("全設定を外部システム初期化後に適用しました");
    }
    
    /// <summary>
    /// SeManagerが初期化された後にSE音量設定を適用（後方互換性のため残す）
    /// </summary>
    public void ApplySeVolumeWhenReady()
    {
        var seSetting = GetSetting<SliderSetting>("SE音量");
        if (seSetting != null && SeManager.Instance != null)
        {
            SeManager.Instance.SeVolume = seSetting.CurrentValue;
            Debug.Log($"SeManager初期化後にSE音量を適用: {seSetting.CurrentValue}");
        }
    }
    
    /// <summary>
    /// 設定ファイルのパスを取得
    /// </summary>
    private string GetSettingsFilePath()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "game_settings.json");
    }
    
    /// <summary>
    /// リソース解放
    /// </summary>
    public void Dispose()
    {
        _disposables?.Dispose();
    }
    
    /// <summary>
    /// 設定データのシリアライゼーション用クラス
    /// </summary>
    [System.Serializable]
    private class SettingsData
    {
        public SerializableDictionary<string, string> settingValues = new();
    }
}