using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;

namespace Void2610.UnityTemplate
{
    public class BgmManager : SingletonMonoBehaviour<BgmManager>
    {
        [System.Serializable]
        public class SoundData
        {
            public string name;
            public AudioClip audioClip;
            public float volume = 1.0f;
            public BgmType bgmType = BgmType.Battle;
        }

        [SerializeField] private AudioMixerGroup bgmMixerGroup;
        [SerializeField] private List<SoundData> bgmList = new List<SoundData>();

        private AudioSource _audioSource;
        private const float FADE_TIME = 1.0f;

        private bool _isPlaying;
        private float _bgmVolume = 1.0f;
        private bool _isFading;
        private SoundData _currentBGM;
        private MotionHandle _fadeHandle;
        private MotionHandle _duckingHandle;
        private float _originalVolume = 1.0f;

        public float BgmVolume
        {
            get => _bgmVolume;
            set
            {
                _bgmVolume = value;
                if (value <= 0.0f)
                {
                    value = 0.0001f;
                }

                bgmMixerGroup.audioMixer.SetFloat("BgmVolume", Mathf.Log10(value) * 20);
            }
        }

        public void Resume()
        {
            if (_currentBGM == null) return;

            _isPlaying = true;
            _audioSource.Play();
            
            _fadeHandle.TryCancel();
            _fadeHandle = LMotion.Create(_audioSource.volume, _currentBGM.volume, FADE_TIME)
                .WithEase(Ease.InQuad)
                .BindToVolume(_audioSource)
                .AddTo(this);
        }

        public void Pause()
        {
            _isPlaying = false;
            PauseInternal().Forget();
        }
        
        private async UniTaskVoid PauseInternal()
        {
            _fadeHandle.TryCancel();
            await LMotion.Create(_audioSource.volume, 0f, FADE_TIME)
                .WithEase(Ease.InQuad)
                .BindToVolume(_audioSource)
                .ToUniTask();
            
            _audioSource.Stop();
        }

        public async UniTask Stop()
        {
            _isPlaying = false;

            _fadeHandle.TryCancel();
            await LMotion.Create(_audioSource.volume, 0f, FADE_TIME)
                .WithEase(Ease.InQuad)
                .BindToVolume(_audioSource)
                .ToUniTask();

            _audioSource.Stop();
            _currentBGM = null;
        }

        /// <summary>
        /// BGMボリュームを一時的に下げる（SE再生時のダッキング用）
        /// </summary>
        /// <param name="duckVolume">下げる先のボリューム（0.0f～1.0f）</param>
        /// <param name="fadeTime">フェード時間</param>
        public async UniTask DuckVolume(float duckVolume = 0.2f, float fadeTime = 0.5f)
        {
            _originalVolume = _audioSource.volume;

            _duckingHandle.TryCancel();
            _duckingHandle = LMotion.Create(_audioSource.volume, _originalVolume * duckVolume, fadeTime)
                .WithEase(Ease.OutQuad)
                .BindToVolume(_audioSource)
                .AddTo(this);

            await _duckingHandle.ToUniTask();
        }

        /// <summary>
        /// ダッキングしたボリュームを元に戻す
        /// </summary>
        /// <param name="fadeTime">フェード時間</param>
        public async UniTask RestoreVolume(float fadeTime = 0.5f)
        {
            _duckingHandle.TryCancel();
            _duckingHandle = LMotion.Create(_audioSource.volume, _originalVolume, fadeTime)
                .WithEase(Ease.InQuad)
                .BindToVolume(_audioSource)
                .AddTo(this);

            await _duckingHandle.ToUniTask();
        }

        public void PlayBGMBySceneType(BgmType bgmType)
        {
            var targetBgmList = bgmList.FindAll(x => x.bgmType == bgmType);
            var data = targetBgmList[Random.Range(0, targetBgmList.Count)];
            PlayBGMInternal(data).Forget();
        }
        
        private async UniTaskVoid PlayBGMInternal(SoundData data)
        {
            // 現在のBGMをフェードアウト
            if (_currentBGM != null)
            {
                _fadeHandle.TryCancel();
                await LMotion.Create(_audioSource.volume, 0f, FADE_TIME)
                    .WithEase(Ease.InQuad)
                    .BindToVolume(_audioSource)
                    .ToUniTask();
                _audioSource.Stop();
            }

            _currentBGM = data;
            _audioSource.clip = _currentBGM.audioClip;
            _audioSource.volume = 0;
            _audioSource.Play();
            _isPlaying = true;

            // フェードイン
            _isFading = true;
            _fadeHandle = LMotion.Create(0f, _currentBGM.volume, FADE_TIME)
                .WithEase(Ease.InQuad)
                .BindToVolume(_audioSource)
                .AddTo(this);

            // フェードイン完了を待機
            FadeInComplete().Forget();
        }
        
        private async UniTaskVoid FadeInComplete()
        {
            await UniTask.Delay((int)(FADE_TIME * 1000));
            _isFading = false;
        }

        protected override void Awake()
        {
            base.Awake();
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.outputAudioMixerGroup = bgmMixerGroup;
            _audioSource.playOnAwake = false;
            _audioSource.loop = false; // ループは手動で管理
            _audioSource.volume = 0f;
        }

        private void Update()
        {
            if (_isPlaying && _audioSource.clip && !_isFading)
            {
                if (!_audioSource.isPlaying)
                {
                    // 曲が終了したので次の曲を再生
                    _isFading = true;
                    LoopToNextBGM(0f).Forget();
                }
                else
                {
                    var remainingTime = _audioSource.clip.length - _audioSource.time;
                    if (remainingTime <= FADE_TIME)
                    {
                        _isFading = true;
                        LoopToNextBGM(remainingTime).Forget();
                    }
                }
            }
        }
        
        private async UniTaskVoid LoopToNextBGM(float fadeTime)
        {
            if (_currentBGM == null) return;

            var currentBgmType = _currentBGM.bgmType;

            _fadeHandle.TryCancel();
            await LMotion.Create(_audioSource.volume, 0f, fadeTime)
                .WithEase(Ease.InQuad)
                .BindToVolume(_audioSource)
                .ToUniTask();

            PlayBGMBySceneType(currentBgmType);
        }
    }
}