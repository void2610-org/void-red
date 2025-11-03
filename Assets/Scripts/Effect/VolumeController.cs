using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Void2610.UnityTemplate;
using LitMotion;

[RequireComponent(typeof(Volume))]
public class VolumeController : SingletonMonoBehaviour<VolumeController>
{
    [SerializeField] private Vector2 filmGrainIntensityRange = new(0f, 1f);
    [SerializeField] private Vector2 chromaticAberrationIntensityRange = new(0f, 1f);
    [SerializeField] private Vector2 vignetteIntensityRange = new(0f, 1f);
    [SerializeField] private Vector2 screenSpaceLensFlareIntensityRange = new(0f, 1f);
    [SerializeField] private float defaultDepthOfFieldFocusDistance = 50f;
    [SerializeField] private float dizzyEffectDuration = 2f;
    [SerializeField] private float dizzyMinDistance = 1f;
    [SerializeField] private float dizzyMaxDistance = 10f;

    private Volume _volume;
    private FilmGrain _filmGrain;
    private ChromaticAberration _chromaticAberration;
    private DepthOfField _depthOfField;
    private Vignette _vignette;
    private ScreenSpaceLensFlare _screenSpaceLensFlare;
    private MotionHandle _dizzyMotionHandle;
    
    public void SetFilmGrainIntensity(float intensity)
    {
        _filmGrain.intensity.value = Mathf.Lerp(filmGrainIntensityRange.x, filmGrainIntensityRange.y, intensity);
    }
    
    public void SetChromaticAberrationIntensity(float intensity)
    {
        _chromaticAberration.intensity.value = Mathf.Lerp(chromaticAberrationIntensityRange.x, chromaticAberrationIntensityRange.y, intensity);
    }
    
    public void SetVignetteIntensity(float intensity)
    {
        _vignette.intensity.value = Mathf.Lerp(vignetteIntensityRange.x, vignetteIntensityRange.y, intensity);
    }
    
    public void SetScreenSpaceLensFlareIntensity(float intensity)
    {
        _screenSpaceLensFlare.intensity.value = Mathf.Lerp(screenSpaceLensFlareIntensityRange.x, screenSpaceLensFlareIntensityRange.y, intensity);
    }

    /// <summary>
    /// めまいエフェクトを開始（被写界深度の焦点距離を周期的に変化）
    /// </summary>
    public void StartDizzyEffect()
    {
        _dizzyMotionHandle.TryCancel();

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
        _dizzyMotionHandle.TryCancel();

        // デフォルト値に戻す
        _depthOfField.focalLength.value = defaultDepthOfFieldFocusDistance;
    }

    /// <summary>
    /// 全てのエフェクトをデフォルト値に戻す
    /// </summary>
    public void ResetToDefault()
    {
        SetFilmGrainIntensity(0f);
        SetChromaticAberrationIntensity(0f);
        SetVignetteIntensity(0f);
        SetScreenSpaceLensFlareIntensity(0f);
        StopDizzyEffect();
    }

    protected override void Awake()
    {
        base.Awake();

        // destroyCancellationTokenを初期化（LitMotionのAddTo使用に必要）
        _ = destroyCancellationToken;

        _volume = this.GetComponent<Volume>();
        
        _volume.profile.TryGet(out _filmGrain);
        _volume.profile.TryGet(out _chromaticAberration);
        _volume.profile.TryGet(out _vignette);
        _volume.profile.TryGet(out _screenSpaceLensFlare);
        _volume.profile.TryGet(out _depthOfField);
    }
}