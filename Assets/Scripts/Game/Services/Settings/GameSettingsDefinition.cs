using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using Void2610.SettingsSystem;
using Void2610.UnityTemplate;

public class GameSettingsDefinition : ISettingsDefinition
{
    private readonly GameProgressService _gameProgressService;

    public GameSettingsDefinition(GameProgressService gameProgressService)
    {
        _gameProgressService = gameProgressService;
    }

    public IEnumerable<SettingsCategory> CreateCategories()
    {
        yield return new SettingsCategory("オーディオ", new ISettingBase[]
        {
            new SliderSetting(
                name: "BGM音量",
                desc: "BGMの音量を設定します",
                defaultVal: BgmManager.Instance.BgmVolume,
                min: 0f,
                max: 1f
            ),
            new SliderSetting(
                name: "SE音量",
                desc: "効果音の音量を設定します",
                defaultVal: SeManager.Instance.SeVolume,
                min: 0f,
                max: 1f
            ),
            new ButtonSetting(
                name: "SE音量テスト",
                desc: "現在のSE音量で効果音を再生します",
                btnText: "再生"
            )
        });

        yield return new SettingsCategory("グラフィック", new ISettingBase[]
        {
            new EnumSetting(
                name: "フルスクリーン",
                desc: "フルスクリーン表示の切り替え",
                opts: new[] { "false", "true" },
                defaultValue: Screen.fullScreen ? "true" : "false",
                displayNames: new[] { "オフ", "オン" }
            )
        });

        yield return new SettingsCategory("データ", new ISettingBase[]
        {
            new ButtonSetting(
                name: "セーブデータ削除",
                desc: "セーブデータを削除して初期状態に戻します",
                btnText: "削除",
                needsConfirmation: true,
                confirmMsg: "セーブデータを削除しますか？\nこの操作は取り消せません。"
            )
        });
    }

    public void BindSettingActions(IReadOnlyList<SettingsCategory> categories, CompositeDisposable disposables)
    {
        foreach (var setting in categories.SelectMany(c => c.Settings))
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
                    buttonSetting.OnButtonClicked
                        .Subscribe(_ => SeManager.Instance.PlaySe("CardSelect"))
                        .AddTo(disposables);
                    break;
                case EnumSetting { SettingName: "フルスクリーン" } enumSetting:
                    enumSetting.OnValueChanged
                        .Subscribe(v => Screen.fullScreen = v == "true")
                        .AddTo(disposables);
                    break;
                case ButtonSetting { SettingName: "セーブデータ削除" } buttonSetting:
                    buttonSetting.OnButtonClicked
                        .Subscribe(_ => _gameProgressService.ResetToDefaultData())
                        .AddTo(disposables);
                    break;
            }
        }
    }
}
