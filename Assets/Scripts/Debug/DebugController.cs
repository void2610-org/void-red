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
    
    private readonly ReactiveProperty<bool> _fastModeProperty = new();
    private readonly ReactiveProperty<float> _timeScaleProperty = new();
    
    private StatsTrackerService _statsTrackerService;
    private SaveDataManager _saveDataManager;
    
    [Inject]
    public void Construct(StatsTrackerService statsTrackerService, SaveDataManager saveDataManager)
    {
        _statsTrackerService = statsTrackerService;
        _saveDataManager = saveDataManager;
    }
    
    private void Awake()
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
        
        // 新しいセーブデータで始めるフラグがtrueの場合、セーブファイルを削除
        if (startWithFreshData && _saveDataManager != null)
            _saveDataManager.DeleteSaveFile();
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
        if (!Application.isEditor || !showSaveInfo || _statsTrackerService == null || _saveDataManager == null) return;
        
        GUILayout.BeginArea(new Rect(10, 100, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("=== セーブデータ情報 ===");
        
        // セーブファイル存在確認
        var saveExists = _saveDataManager.SaveFileExists();
        GUILayout.Label($"セーブファイル: {(saveExists ? "存在" : "なし")}");
        
        // フラグ状態表示
        GUILayout.Label($"新データ開始: {(startWithFreshData ? "ON" : "OFF")}");
        
        // プレイヤー統計情報表示
        var playerData = _statsTrackerService.PlayerSaveData;
        GUILayout.Label($"統計: {playerData.GetStatsString()}");
        
        if (GUILayout.Button("セーブファイル削除"))
        {
            var success = _saveDataManager.DeleteSaveFile();
            Debug.Log($"セーブファイル削除: {(success ? "成功" : "失敗")}");
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}