using System;
using R3;
using VContainer.Unity;

/// <summary>
/// プレイヤーの精神力に応じてポストプロセスエフェクトを制御するクラス
/// </summary>
public class MentalPowerEffectController : IStartable, IDisposable
{
    private readonly Player _player;
    private readonly VolumeController _volumeController;
    private readonly CompositeDisposable _disposables = new();

    // めまいエフェクト制御用
    private bool _isDizzyEffectActive;
    private const float DIZZY_EFFECT_THRESHOLD = 0.3f; // 精神力30%以下でめまい発動

    public MentalPowerEffectController(Player player, VolumeController volumeController)
    {
        _player = player;
        _volumeController = volumeController;
    }

    /// <summary>
    /// 初期化処理（精神力の購読開始）
    /// </summary>
    public void Start()
    {
        _player.MentalPower
            .Subscribe(OnMentalPowerChanged)
            .AddTo(_disposables);
    }

    /// <summary>
    /// 精神力変化時のコールバック
    /// </summary>
    private void OnMentalPowerChanged(int mentalPower)
    {
        // 精神力割合を計算（0.0～1.0）
        var ratio = (mentalPower + 10) / (float)GameConstants.MAX_MENTAL_POWER;
        ratio = Math.Clamp(ratio, 0f, 1f);
        var inverseRatio = 1f - ratio; // 精神力が低いほど大きい値

        // エフェクト強度を設定
        _volumeController.SetFilmGrainIntensity(inverseRatio);
        _volumeController.SetChromaticAberrationIntensity(inverseRatio);
        _volumeController.SetVignetteIntensity(inverseRatio); // ビネットは控えめに

        // めまいエフェクトの制御
        if (ratio <= DIZZY_EFFECT_THRESHOLD && !_isDizzyEffectActive)
        {
            _volumeController.StartDizzyEffect();
            _isDizzyEffectActive = true;
        }
        else if (ratio > DIZZY_EFFECT_THRESHOLD && _isDizzyEffectActive)
        {
            _volumeController.StopDizzyEffect();
            _isDizzyEffectActive = false;
        }
    }

    /// <summary>
    /// リソースの解放
    /// </summary>
    public void Dispose()
    {
        if (_isDizzyEffectActive)
        {
            _volumeController.StopDizzyEffect();
        }
        _disposables?.Dispose();
    }
}
