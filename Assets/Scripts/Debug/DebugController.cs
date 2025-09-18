using UnityEngine;
using R3;
using VContainer;

/// <summary>
/// デバッグ機能をまとめて管理するコンポーネント
/// </summary>
public class DebugController : MonoBehaviour
{
    [Header("実行速度")]
    [SerializeField] private bool enableFastMode = false;
    [SerializeField, Range(0.1f, 10f)] private float timeScale = 2f;
    
    [Header("セーブデータ")]
    [SerializeField] private bool showSaveInfo = false;
    [SerializeField] private bool startWithFreshData = false; // 毎回新しいセーブデータで始める
    
    [Header("Steam連携")]
    [SerializeField] private bool resetSteamStats = false; // Steamの実績・統計情報をリセットする
    
    private readonly ReactiveProperty<bool> _fastModeProperty = new();
    private readonly ReactiveProperty<float> _timeScaleProperty = new();
    
    private GameProgressService _gameProgressService;
    private SaveDataManager _saveDataManager;
    private SteamService _steamService;
    
    [Inject]
    public void Construct(GameProgressService gameProgressService, SaveDataManager saveDataManager, SteamService steamService)
    {
        _gameProgressService = gameProgressService;
        _saveDataManager = saveDataManager;
        _steamService = steamService;
        Init();
    }
    
    private void Init()
    {
        if (!Application.isEditor)
        {
            Destroy(this);
            return;
        }
        
        // 初期値設定
        _fastModeProperty.Value = enableFastMode;
        _timeScaleProperty.Value = timeScale;
        
        // 値の変更を監視してタイムスケールを適用
        _fastModeProperty.CombineLatest(_timeScaleProperty, (fastMode, scale) => fastMode ? scale : 1f)
            .Subscribe(scale => Time.timeScale = scale)
            .AddTo(this);
        
        if (startWithFreshData)
        {
            var success = _saveDataManager.DeleteSaveFile();
            if (success) Debug.Log("[Debug] セーブファイル削除完了");
        }
        
        if (resetSteamStats)
        {
            var success = _steamService.ResetAllStats();
            if (success) Debug.Log("[Debug] Steamの実績・統計情報をリセットしました");
            else Debug.LogWarning("[Debug] Steamの実績・統計情報のリセットに失敗しました");
        }
    }
    
    private void OnValidate()
    {
        // インスペクターでの変更を反映
        if (Application.isPlaying)
        {
            _fastModeProperty.Value = enableFastMode;
            _timeScaleProperty.Value = timeScale;
        }
    }
    
    private void OnGUI()
    {
        if (!Application.isEditor || !showSaveInfo || _gameProgressService == null || _saveDataManager == null) return;
        
        GUILayout.BeginArea(new Rect(10, 100, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("=== セーブデータ情報 ===");
        
        // セーブファイル存在確認
        var saveExists = _saveDataManager.SaveFileExists();
        GUILayout.Label($"セーブファイル: {(saveExists ? "存在" : "なし")}");
        
        // フラグ状態表示
        GUILayout.Label($"新データ開始: {(startWithFreshData ? "ON" : "OFF")}");
        
        // プレイヤー統計情報表示（現在のゲーム状態を表示）
        GUILayout.Label($"精神力: {_gameProgressService.GetPlayerMentalPower()}");
        
        if (GUILayout.Button("セーブファイル削除"))
        {
            var success = _saveDataManager.DeleteSaveFile();
            if (success) 
            {
                _gameProgressService.ResetToDefaultData();
            }
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}