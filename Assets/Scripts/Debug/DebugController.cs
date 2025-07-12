using UnityEngine;
using R3;

/// <summary>
/// デバッグ機能をまとめて管理するコンポーネント
/// </summary>
public class DebugController : MonoBehaviour
{
    [Header("実行速度")]
    [SerializeField] private bool enableFastMode = false;
    [SerializeField, Range(0.1f, 10f)] private float timeScale = 2f;
    
    private readonly ReactiveProperty<bool> _fastModeProperty = new();
    private readonly ReactiveProperty<float> _timeScaleProperty = new();
    
    private void Awake()
    {
        if(!Application.isEditor) return;
        
        // 初期値設定
        _fastModeProperty.Value = enableFastMode;
        _timeScaleProperty.Value = timeScale;
        
        // 値の変更を監視してタイムスケールを適用
        _fastModeProperty.CombineLatest(_timeScaleProperty, (fastMode, scale) => fastMode ? scale : 1f)
            .Subscribe(scale => Time.timeScale = scale)
            .AddTo(this);
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
}