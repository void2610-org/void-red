using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Void2610.UnityTemplate;
using LitMotion;

[RequireComponent(typeof(Volume))]
public class VolumeController : SingletonMonoBehaviour<VolumeController>
{
    [SerializeField] private Vector2 filmGrainIntensityRange = new Vector2(0f, 1f);
    [SerializeField] private Vector2 chromaticAberrationIntensityRange = new Vector2(0f, 1f);
    [SerializeField] private float defaultDepthOfFieldFocusDistance = 50f;
    [SerializeField] private float dizzyEffectDuration = 2f;
    [SerializeField] private float dizzyMinDistance = 1f;
    [SerializeField] private float dizzyMaxDistance = 10f;

    private Volume _volume;
    private FilmGrain _filmGrain;
    private ChromaticAberration _chromaticAberration;
    private DepthOfField _depthOfField;
    private MotionHandle _dizzyMotionHandle;
    
    public void SetFilmGrainIntensity(float intensity)
    {
        _filmGrain.intensity.value = Mathf.Clamp(intensity, filmGrainIntensityRange.x, filmGrainIntensityRange.y);
    }
    
    public void SetChromaticAberrationIntensity(float intensity)
    {
        _chromaticAberration.intensity.value = Mathf.Clamp(intensity, chromaticAberrationIntensityRange.x, chromaticAberrationIntensityRange.y);
    }

    /// <summary>
    /// めまいエフェクトを開始（被写界深度の焦点距離を周期的に変化）
    /// </summary>
    public void StartDizzyEffect()
    {
        // 既存のモーションを停止
        if (_dizzyMotionHandle.IsActive()) _dizzyMotionHandle.Cancel();

        // 焦点距離を往復させるモーションを作成
        _dizzyMotionHandle = LMotion.Create(dizzyMinDistance, dizzyMaxDistance, dizzyEffectDuration)
            .WithLoops(-1, LoopType.Yoyo)
            .WithEase(Ease.InOutCirc)
            .Bind(x => _depthOfField.focalLength.value = x)
            .AddTo(this);
    }

    /// <summary>
    /// めまいエフェクトを停止し、デフォルト値に戻す
    /// </summary>
    public void StopDizzyEffect()
    {
        if (_dizzyMotionHandle.IsActive()) _dizzyMotionHandle.Cancel();

        // デフォルト値に戻す
        _depthOfField.focalLength.value = defaultDepthOfFieldFocusDistance;
    }

    protected override void Awake()
    {
        base.Awake();
        
        _volume = this.GetComponent<Volume>();
        
        _volume.profile.TryGet(out _filmGrain);
        _volume.profile.TryGet(out _chromaticAberration);
        _volume.profile.TryGet(out _depthOfField);
    }
}