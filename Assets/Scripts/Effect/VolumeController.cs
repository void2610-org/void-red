using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Void2610.UnityTemplate;

[RequireComponent(typeof(Volume))]
public class VolumeController : SingletonMonoBehaviour<VolumeController>
{
    [SerializeField] private Vector2 filmGrainIntensityRange = new Vector2(0f, 1f);
    [SerializeField] private Vector2 chromaticAberrationIntensityRange = new Vector2(0f, 1f);
    
    private Volume _volume;
    
    public void SetFilmGrainIntensity(float intensity)
    {
        if (_volume.profile.TryGet<FilmGrain>(out var filmGrain))
        {
            filmGrain.intensity.value = Mathf.Clamp(intensity, filmGrainIntensityRange.x, filmGrainIntensityRange.y);
        }
    }
    
    public void SetChromaticAberrationIntensity(float intensity)
    {
        if (_volume.profile.TryGet<ChromaticAberration>(out var chromaticAberration))
        {
            chromaticAberration.intensity.value = Mathf.Clamp(intensity, chromaticAberrationIntensityRange.x, chromaticAberrationIntensityRange.y);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _volume = this.GetComponent<Volume>();
    }
}