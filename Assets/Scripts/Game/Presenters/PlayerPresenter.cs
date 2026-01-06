using R3;
using System;

/// <summary>
/// プレイヤーとNPCの基底プレゼンタークラス（Presenter Layer）
/// カード管理・UI制御・ゲームロジックを統合
/// </summary>
public abstract class PlayerPresenter : IDisposable
{
    // 公開プロパティ
    public ReadOnlyReactiveProperty<int> MentalPower => _playerModel.MentalPower;
    
    // プライベートフィールド
    private readonly PlayerModel _playerModel;
    private readonly GameProgressService _gameProgressService;
    
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="gameProgressService">ゲーム進行サービス（オプショナル）</param>
    protected PlayerPresenter(GameProgressService gameProgressService = null)
    {
        _playerModel = new PlayerModel();
        _gameProgressService = gameProgressService;
    }
    
    // === 精神力管理 ===
    
    /// <summary>
    /// 精神力を消費
    /// </summary>
    /// <param name="amount">消費量</param>
    /// <returns>消費に成功したかどうか</returns>
    public bool ConsumeMentalPower(int amount) => _playerModel.TryConsumeMentalPower(amount);
    
    /// <summary>
    /// 精神力を回復
    /// </summary>
    /// <param name="amount">回復量</param>
    public void RestoreMentalPower(int amount) => _playerModel.RestoreMentalPower(amount);
    
    /// <summary>
    /// 精神力が足りるかチェック
    /// </summary>
    /// <param name="requiredAmount">必要な精神力</param>
    /// <returns>足りるかどうか</returns>
    public bool HasEnoughMentalPower(int requiredAmount) => _playerModel.MentalPower.CurrentValue >= requiredAmount;
    
    /// <summary>
    /// 精神力を設定
    /// </summary>
    /// <param name="value">設定する精神力</param>
    public void SetMentalPower(int value) => _playerModel.SetMentalPower(value);
    
    /// <summary>
    /// プレイヤーの状態をリセット
    /// </summary>
    public void ResetPlayerState()
    {
        _playerModel.SetMentalPower(GameConstants.MAX_MENTAL_POWER);
    }
    
    /// <summary>
    /// リソースの解放
    /// </summary>
    public virtual void Dispose()
    {
        _playerModel?.Dispose();
    }
}