using UnityEngine;
using Void2610.UnityTemplate;

public class VolumeController : SingletonMonoBehaviour<VolumeController>
{
    [SerializeField] private GlobalVolume globalVolume;

    protected override void Awake()
    {
        base.Awake();
    }
}