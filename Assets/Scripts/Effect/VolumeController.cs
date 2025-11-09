using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Void2610.UnityTemplate;
using LitMotion;

[RequireComponent(typeof(Volume))]
public class VolumeController : SingletonMonoBehaviour<VolumeController>
{
    /// <summary>
    /// Volumeエフェクトのカプセル化クラス
    /// </summary>
    private class VolumeEffect
    {
        private readonly VolumeParameter<float> _parameter;
        private readonly Vector2 _range;
        private readonly float _duration;
        private readonly Component _addTo;
        private MotionHandle _motionHandle;

        public VolumeEffect(VolumeParameter<float> parameter, Vector2 range, float duration, Component addTo)
        {
            _parameter = parameter;
            _range = range;
            _duration = duration;
            _addTo = addTo;
        }

        public void SetIntensity(float intensity)
        {
            _motionHandle.TryCancel();
            
            var target = Mathf.Lerp(_range.x, _range.y, intensity);
            var start = _parameter.value;
            _motionHandle = LMotion.Create(0f, 1f, _duration)
                .WithEase(Ease.OutQuad)
                .Bind(t => _parameter.value = Mathf.Lerp(start, target, t))
                .AddTo(_addTo);
        }
    }

    [SerializeField] private Vector2 filmGrainIntensityRange = new(0f, 1f);
    [SerializeField] private Vector2 chromaticAberrationIntensityRange = new(0f, 1f);
    [SerializeField] private Vector2 vignetteIntensityRange = new(0f, 1f);
    [SerializeField] private Vector2 screenSpaceLensFlareIntensityRange = new(0f, 1f);
    [SerializeField] private float defaultDepthOfFieldFocusDistance = 50f;
    [SerializeField] private float dizzyEffectDuration = 2f;
    [SerializeField] private float dizzyMinDistance = 1f;
    [SerializeField] private float dizzyMaxDistance = 10f;
    
    private const float DURATION = 0.5f;

    private Volume _volume;
    private DepthOfField _depthOfField;
    private MotionHandle _dizzyMotionHandle;

    private VolumeEffect _filmGrainEffect;
    private VolumeEffect _chromaticAberrationEffect;
    private VolumeEffect _vignetteEffect;
    private VolumeEffect _lensFlareEffect;
    private VolumeEffect _depthOfFieldEffect;

    public void SetFilmGrainIntensity(float intensity) => _filmGrainEffect.SetIntensity(intensity);
    public void SetChromaticAberrationIntensity(float intensity) => _chromaticAberrationEffect.SetIntensity(intensity);
    public void SetVignetteIntensity(float intensity) => _vignetteEffect.SetIntensity(intensity);
    public void SetScreenSpaceLensFlareIntensity(float intensity) => _lensFlareEffect.SetIntensity(intensity);

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
    /// めまいエフェクトを停止し、デフォルト値に滑らかに戻す
    /// </summary>
    public void StopDizzyEffect()
    {
        _dizzyMotionHandle.TryCancel();
        _depthOfFieldEffect.SetIntensity(1f);
    }

    /// <summary>
    /// 全てのエフェクトをデフォルト値に滑らかに戻す
    /// </summary>
    public void ResetToDefault()
    {
        _dizzyMotionHandle.TryCancel();
        _filmGrainEffect.SetIntensity(0f);
        _chromaticAberrationEffect.SetIntensity(0f);
        _vignetteEffect.SetIntensity(0f);
        _lensFlareEffect.SetIntensity(0f);
        _depthOfFieldEffect.SetIntensity(1f);
    }

    protected override void Awake()
    {
        base.Awake();

        // destroyCancellationTokenを初期化（LitMotionのAddTo使用に必要）
        _ = destroyCancellationToken;

        _volume = this.GetComponent<Volume>();

        _volume.profile.TryGet(out FilmGrain filmGrain);
        _volume.profile.TryGet(out ChromaticAberration chromaticAberration);
        _volume.profile.TryGet(out Vignette vignette);
        _volume.profile.TryGet(out ScreenSpaceLensFlare screenSpaceLensFlare);
        _volume.profile.TryGet(out _depthOfField);

        _filmGrainEffect = new VolumeEffect(filmGrain.intensity, filmGrainIntensityRange, DURATION, this);
        _chromaticAberrationEffect = new VolumeEffect(chromaticAberration.intensity, chromaticAberrationIntensityRange, DURATION, this);
        _vignetteEffect = new VolumeEffect(vignette.intensity, vignetteIntensityRange, DURATION, this);
        _lensFlareEffect = new VolumeEffect(screenSpaceLensFlare.intensity, screenSpaceLensFlareIntensityRange, DURATION, this);
        _depthOfFieldEffect = new VolumeEffect(_depthOfField.focalLength, new Vector2(0f, defaultDepthOfFieldFocusDistance), DURATION, this);
    }
}