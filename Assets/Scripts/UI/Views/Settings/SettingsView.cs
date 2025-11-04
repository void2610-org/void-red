using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

/// <summary>
/// 設定画面のUI表示を担当するViewクラス
/// 純粋なView - 外部からのデータ注入とイベント通知のみ
/// </summary>
public class SettingsView : BaseWindowView
{
    [Header("UI設定")]
    [SerializeField] private Transform settingsContainer;
    
    [Header("設定項目プレハブ")]
    [SerializeField] private GameObject settingsContentContainerPrefab;
    [SerializeField] private GameObject titleTextPrefab;
    [SerializeField] private GameObject sliderSettingPrefab;
    [SerializeField] private GameObject buttonSettingPrefab;
    [SerializeField] private GameObject enumSettingPrefab;
    
    // 外部への通知用イベント
    private readonly Subject<(string settingName, float value)> _onSliderChanged = new();
    private readonly Subject<(string settingName, string value)> _onEnumChanged = new();
    private readonly Subject<string> _onButtonClicked = new();

    private readonly List<GameObject> _settingUIObjects = new();
    private readonly List<ISettingItemNavigatable> _settingItems = new();
    private readonly CompositeDisposable _itemSubscriptions = new();

    private ConfirmationDialogService _confirmationDialogService;
    private int _currentFocusIndex;
    
    public Observable<(string settingName, float value)> OnSliderChanged => _onSliderChanged;
    public Observable<(string settingName, string value)> OnEnumChanged => _onEnumChanged;
    public Observable<string> OnButtonClicked => _onButtonClicked;
    
    /// <summary>
    /// 設定データ構造体
    /// </summary>
    [Serializable]
    public struct SettingDisplayData
    {
        public string name;
        public string displayName;
        public SettingType type;
        public float floatValue;
        public string stringValue;
        public float minValue;
        public float maxValue;
        public string[] options;
        public string[] displayNames;
        public string buttonText;
        public bool requiresConfirmation;
        public string confirmationMessage;
    }
    
    public enum SettingType
    {
        Slider,
        Enum,
        Button
    }

    /// <summary>
    /// 外部から設定データを注入してUIを更新
    /// </summary>
    /// <param name="settingsData">設定データ配列</param>
    /// <param name="confirmationDialog">確認ダイアログサービス</param>
    public void SetSettings(SettingDisplayData[] settingsData, ConfirmationDialogService confirmationDialog)
    {
        _confirmationDialogService = confirmationDialog;

        ClearSettingsUI();

        // 各設定項目のUIを生成
        foreach (var settingData in settingsData)
            CreateSettingUI(settingData);

        _currentFocusIndex = 0;
    }
    
    /// <summary>
    /// 設定項目のUIを生成
    /// </summary>
    private void CreateSettingUI(SettingDisplayData settingData)
    {
        // 設定項目のコンテナを作成（横並び用）
        var containerObject = Instantiate(settingsContentContainerPrefab, settingsContainer);
        // タイトルテキストを作成（左側）
        CreateTitleText(containerObject.transform, settingData.displayName);
        
        // 設定固有のUIを作成（右側）
        switch (settingData.type)
        {
            case SettingType.Slider:
                CreateSliderUI(settingData, containerObject.transform);
                break;
            case SettingType.Button:
                CreateButtonUI(settingData, containerObject.transform);
                break;
            case SettingType.Enum:
                CreateEnumUI(settingData, containerObject.transform);
                break;
            default:
                Debug.LogWarning($"未対応の設定タイプ: {settingData.type}");
                break;
        }
        
        _settingUIObjects.Add(containerObject);
    }
    
    /// <summary>
    /// タイトルテキストを作成
    /// </summary>
    private void CreateTitleText(Transform parent, string titleText)
    {
        var titleObject = Instantiate(titleTextPrefab, parent);
        
        // プレハブからTextコンポーネントを取得してテキストを設定
        var textComponent = titleObject.GetComponentInChildren<TextMeshProUGUI>();
        textComponent.text = titleText;
        // レイアウト要素を追加してタイトル幅を固定
        if (!titleObject.TryGetComponent<LayoutElement>(out var layoutElement))
            layoutElement = titleObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 150f; // タイトルの固定幅
        layoutElement.flexibleWidth = 0f;    // 伸縮しない
    }
    
    /// <summary>
    /// スライダー設定のUIを生成
    /// </summary>
    private void CreateSliderUI(SettingDisplayData settingData, Transform parent)
    {
        var uiObject = Instantiate(sliderSettingPrefab, parent);

        // SliderSettingItemコンポーネントを取得または追加
        var settingItem = uiObject.GetComponent<SliderSettingItem>();
        if (!settingItem) settingItem = uiObject.AddComponent<SliderSettingItem>();

        // 初期化
        settingItem.Initialize(settingData.name, settingData.minValue, settingData.maxValue, settingData.floatValue);

        // イベントをSubscribe
        settingItem.OnValueChanged
            .Subscribe(data => _onSliderChanged.OnNext((data.settingName, (float)data.value)))
            .AddTo(_itemSubscriptions);

        // ナビゲーション用リストに追加
        _settingItems.Add(settingItem);
    }
    
    /// <summary>
    /// ボタン設定のUIを生成
    /// </summary>
    private void CreateButtonUI(SettingDisplayData settingData, Transform parent)
    {
        var uiObject = Instantiate(buttonSettingPrefab, parent);

        // ButtonSettingItemコンポーネントを取得または追加
        var settingItem = uiObject.GetComponent<ButtonSettingItem>();
        if (!settingItem) settingItem = uiObject.AddComponent<ButtonSettingItem>();

        // 初期化
        settingItem.Initialize(settingData.name, settingData.buttonText);

        // イベントをSubscribe
        settingItem.OnValueChanged
            .Subscribe(data => {
                if (settingData.requiresConfirmation)
                    ShowConfirmationDialog(settingData).Forget();
                else
                    _onButtonClicked.OnNext(data.settingName);
            })
            .AddTo(_itemSubscriptions);

        // ナビゲーション用リストに追加
        _settingItems.Add(settingItem);
    }
    
    /// <summary>
    /// 確認ダイアログを表示
    /// </summary>
    private async UniTaskVoid ShowConfirmationDialog(SettingDisplayData settingData)
    {
        var result = await _confirmationDialogService.ShowDialog(
            settingData.confirmationMessage,
            "実行"
        );
        
        if (result)
        {
            _onButtonClicked.OnNext(settingData.name);
        }
    }
    
    /// <summary>
    /// Enum設定のUIを生成
    /// </summary>
    private void CreateEnumUI(SettingDisplayData settingData, Transform parent)
    {
        var uiObject = Instantiate(enumSettingPrefab, parent);

        // EnumSettingItemコンポーネントを取得または追加
        var settingItem = uiObject.GetComponent<EnumSettingItem>();
        if (!settingItem) settingItem = uiObject.AddComponent<EnumSettingItem>();

        // 初期化
        settingItem.Initialize(settingData.name, settingData.options, settingData.displayNames, settingData.stringValue);

        // イベントをSubscribe
        settingItem.OnValueChanged
            .Subscribe(data => _onEnumChanged.OnNext((data.settingName, (string)data.value)))
            .AddTo(_itemSubscriptions);

        // ナビゲーション用リストに追加
        _settingItems.Add(settingItem);
    }
    
    /// <summary>
    /// 上下ナビゲーション（フォーカス移動）
    /// </summary>
    public void NavigateVertical(float direction)
    {
        if (_settingItems.Count == 0) return;
        if (Mathf.Abs(direction) < 0.1f) return;

        // 方向に応じてフォーカスを移動
        if (direction > 0)
            _currentFocusIndex = (_currentFocusIndex - 1 + _settingItems.Count) % _settingItems.Count;
        else
            _currentFocusIndex = (_currentFocusIndex + 1) % _settingItems.Count;
    }

    /// <summary>
    /// 左右ナビゲーション（現在フォーカス項目の操作）
    /// </summary>
    public void NavigateHorizontal(float direction)
    {
        if (_settingItems.Count == 0 || _currentFocusIndex < 0 || _currentFocusIndex >= _settingItems.Count)
            return;

        _settingItems[_currentFocusIndex].OnNavigateHorizontal(direction);
    }

    /// <summary>
    /// 決定操作（現在フォーカス項目の実行）
    /// </summary>
    public void SubmitCurrent()
    {
        if (_settingItems.Count == 0 || _currentFocusIndex < 0 || _currentFocusIndex >= _settingItems.Count)
            return;

        _settingItems[_currentFocusIndex].OnSubmit();
    }

    /// <summary>
    /// 設定UIをクリア
    /// </summary>
    private void ClearSettingsUI()
    {
        _settingItems.Clear();

        // Subscriptionをクリア
        _itemSubscriptions.Clear();

        // UI要素をDestroy
        foreach (var uiObject in _settingUIObjects)
        {
            if (uiObject) DestroyImmediate(uiObject);
        }
        _settingUIObjects.Clear();

        _currentFocusIndex = 0;
    }
}