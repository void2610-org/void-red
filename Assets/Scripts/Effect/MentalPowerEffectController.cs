using System;
using R3;
using VContainer.Unity;

/// <summary>
/// プレイヤーの精神力に応じてポストプロセスエフェクトを制御するクラス
/// </summary>
public class MentalPowerEffectController : IStartable, IDisposable
{
    private const float DIZZY_EFFECT_THRESHOLD = 0.6f;

    private readonly Player _player;
    private readonly VolumeController _volumeController;
    private readonly CompositeDisposable _disposables = new();

    // めまいエフェクト制御用
    private bool _isDizzyEffectActive;

    public MentalPowerEffectController(Player player)
    {
        _player = player;
        _volumeController = VolumeController.Instance;
    }

    /// <summary>
    /// 精神力変化時のコールバック
    /// </summary>
    private void OnMentalPowerChanged(int mentalPower)
    {
        // 精神力割合を計算（0.0～1.0）
        var ratio = mentalPower / 7f;
        if (ratio > 0.7) return;

        var inverseRatio = 1f - ratio; // 精神力が低いほど大きい値

        // エフェクト強度を設定
        _volumeController.SetFilmGrainIntensity(inverseRatio);
        _volumeController.SetChromaticAberrationIntensity(inverseRatio);
        _volumeController.SetVignetteIntensity(inverseRatio);

        // めまいエフェクトの制御
        if (inverseRatio > DIZZY_EFFECT_THRESHOLD && !_isDizzyEffectActive)
        {
            _volumeController.StartDizzyEffect();
            _isDizzyEffectActive = true;
        }
        else if (inverseRatio < DIZZY_EFFECT_THRESHOLD && _isDizzyEffectActive)
        {
            _volumeController.StopDizzyEffect();
            _isDizzyEffectActive = false;
        }
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    public void Start()
    {
        // TODO: 感情リソースシステムに移行後、エフェクト制御を更新する
        // _player.MentalPower
        //     .Subscribe(OnMentalPowerChanged)
        //     .AddTo(_disposables);
    }

    /// <summary>
    /// リソースの解放
    /// </summary>
    public void Dispose()
    {
        _volumeController.ResetToDefault();
        _disposables?.Dispose();
    }
}
