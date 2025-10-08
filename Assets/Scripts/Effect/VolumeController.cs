using UnityEngine;
using UnityEngine.Rendering;
using Void2610.UnityTemplate;

[RequireComponent(typeof(Volume))]
public class VolumeController : SingletonMonoBehaviour<VolumeController>
{
    private Volume _volume;

    protected override void Awake()
    {
        base.Awake();
        _volume = this.GetComponent<Volume>();
    }
}