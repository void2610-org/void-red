using System.Collections.Generic;
using R3;
using UnityEngine;
using Void2610.SettingsSystem;
using Void2610.UnityTemplate;

/// <summary>
/// ゲーム固有の設定定義
/// BGM/SE音量、セーブデータ削除などのプロジェクト固有設定を管理
/// </summary>
public class GameSettingsDefinition : ISettingsDefinition
{
    private readonly GameProgressService _gameProgressService;

    public GameSettingsDefinition(GameProgressService gameProgressService)
    {
        _gameProgressService = gameProgressService;
    }

    /// <summary>
    /// 設定項目を作成して返す
    /// </summary>
    public IEnumerable<ISettingBase> CreateSettings()
    {
        yield return new SliderSetting(
            name: "BGM音量",
            desc: "BGMの音量を設定します",
            defaultVal: BgmManager.Instance?.BgmVolume ?? 0.5f,
            min: 0f,
            max: 1f
        );
        yield return new SliderSetting(
            name: "SE音量",
            desc: "効果音の音量を設定します",
            defaultVal: SeManager.Instance?.SeVolume ?? 0.5f,
            min: 0f,
            max: 1f
        );
        yield return new ButtonSetting(
            name: "SE音量テスト",
            desc: "現在のSE音量で効果音を再生します",
            btnText: "再生"
        );
        yield return new EnumSetting(
            name: "フルスクリーン",
            desc: "フルスクリーン表示の切り替え",
            opts: new[] { "false", "true" },
            defaultValue: Screen.fullScreen ? "true" : "false",
            displayNames: new[] { "オフ", "オン" }
        );
        yield return new ButtonSetting(
            name: "セーブデータ削除",
            desc: "セーブデータを削除して初期状態に戻します",
            btnText: "削除",
            needsConfirmation: true,
            confirmMsg: "セーブデータを削除しますか？\nこの操作は取り消せません。"
        );
    }

    /// <summary>
    /// 設定値の変更をシステムに反映するバインディングを設定
    /// </summary>
    public void BindSettingActions(IReadOnlyList<ISettingBase> settings, CompositeDisposable disposables)
    {
        foreach (var setting in settings)
        {
            switch (setting)
            {
                case SliderSetting { SettingName: "BGM音量" } sliderSetting:
                    sliderSetting.OnSettingChanged
                        .Subscribe(_ => BgmManager.Instance.BgmVolume = sliderSetting.CurrentValue)
                        .AddTo(disposables);
                    break;
                case SliderSetting { SettingName: "SE音量" } sliderSetting:
                    sliderSetting.OnSettingChanged
                        .Subscribe(_ => SeManager.Instance.SeVolume = sliderSetting.CurrentValue)
                        .AddTo(disposables);
                    break;
                case ButtonSetting { SettingName: "SE音量テスト" } buttonSetting:
                    buttonSetting.ButtonAction = () => SeManager.Instance.PlaySe("CardSelect");
                    break;
                case EnumSetting { SettingName: "フルスクリーン" } enumSetting:
                    enumSetting.OnValueChanged
                        .Subscribe(v => Screen.fullScreen = v == "true")
                        .AddTo(disposables);
                    break;
                case ButtonSetting { SettingName: "セーブデータ削除" } buttonSetting:
                    buttonSetting.ButtonAction = () => _gameProgressService?.ResetToDefaultData();
                    break;
            }
        }
    }
}
