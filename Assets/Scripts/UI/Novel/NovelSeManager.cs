using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// ノベルシーンの効果音再生を管理する単純なクラス
/// </summary>
public class NovelSeManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundData
    {
        public string name;
        public AudioClip audioClip;
        public float volume = 1.0f;
    }

    [SerializeField] private AudioMixerGroup seMixerGroup;
    [SerializeField] private SoundData[] soundData;

    private AudioSource _seAudioSource;
    private float _seVolume = 0.5f;

    /// <summary>
    /// SE音量プロパティ（0.0f～1.0f）
    /// </summary>
    public float SeVolume
    {
        get => _seVolume;
        set
        {
            _seVolume = Mathf.Clamp01(value);
            if (_seVolume <= 0.0f) _seVolume = 0.0001f;
            
            seMixerGroup.audioMixer.SetFloat("SeVolume", Mathf.Log10(_seVolume) * 20);
        }
    }

    /// <summary>
    /// 名前を指定してSEを再生
    /// </summary>
    /// <param name="seName">SE名</param>
    /// <param name="volume">音量倍率</param>
    /// <param name="pitch">ピッチ（-1でランダム）</param>
    /// <returns>再生時間（秒）</returns>
    public float PlaySe(string seName, float volume = 1.0f, float pitch = -1.0f)
    {
        var data = soundData.FirstOrDefault(t => t.name == seName);
        
        if (data == null) 
        {
            Debug.LogWarning($"SE '{seName}' が見つかりません。");
            return 0f;
        }

        _seAudioSource.clip = data.audioClip;
        _seAudioSource.volume = data.volume * volume;
        
        // ピッチがマイナスの場合はランダム化
        _seAudioSource.pitch = pitch < 0.0f ? Random.Range(0.8f, 1.2f) : pitch;
        _seAudioSource.Play();
        return data.audioClip.length / _seAudioSource.pitch;
    }

    /// <summary>
    /// 遅延してSEを再生
    /// </summary>
    /// <param name="seName">SE名</param>
    /// <param name="delayTime">遅延時間（秒）</param>
    /// <param name="volume">音量倍率</param>
    /// <param name="pitch">ピッチ</param>
    /// <param name="important">重要なSEフラグ</param>
    public void WaitAndPlaySe(string seName, float delayTime, float volume = 1.0f, float pitch = 1.0f)
    {
        WaitAndPlaySeAsync(seName, delayTime, volume, pitch).Forget();
    }
    
    public void StopSe()
    {
        if (_seAudioSource.isPlaying)
            _seAudioSource.Stop();
    }

    /// <summary>
    /// 遅延SE再生の内部処理
    /// </summary>
    private async UniTaskVoid WaitAndPlaySeAsync(string seName, float delayTime, float volume, float pitch)
    {
        await UniTask.Delay((int)(delayTime * 1000));
        PlaySe(seName, volume, pitch);
    }

    protected void Awake()
    {
        _seAudioSource = gameObject.AddComponent<AudioSource>();
        _seAudioSource.outputAudioMixerGroup = seMixerGroup;
    }
}