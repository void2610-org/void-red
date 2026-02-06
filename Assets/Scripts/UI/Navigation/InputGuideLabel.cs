using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class InputGuideLabel : MonoBehaviour
{
    public enum InputSchemeType
    {
        KeyboardAndMouse,
        Gamepad
    }

    [SerializeField] private InputActionReference inputActionReference;

    private Image _image;

    private readonly Subject<InputSchemeType> _schemeChanged = new();
    private IDisposable _schemeSubscription;
    private InputSchemeType _scheme = InputSchemeType.KeyboardAndMouse;
    private AsyncOperationHandle<Sprite> _spriteHandle;

    private InputSchemeType Scheme
    {
        get => _scheme;
        set
        {
            if (_scheme == value) return;
            _scheme = value;
            _schemeChanged.OnNext(_scheme);
        }
    }

    // デバイスからの生の入力を受け取って現在のスキーマを更新する
    private void OnEvent(InputEventPtr eventPtr, InputDevice device)
    {
        var eventType = eventPtr.type;
        if (eventType != StateEvent.Type && eventType != DeltaStateEvent.Type)
            return;

        using var inputEventControlEnumerator = eventPtr.EnumerateControls(
            InputControlExtensions.Enumerate.IncludeNonLeafControls |
            InputControlExtensions.Enumerate.IncludeSyntheticControls |
            InputControlExtensions.Enumerate.IgnoreControlsInCurrentState |
            InputControlExtensions.Enumerate.IgnoreControlsInDefaultState
        ).GetEnumerator();
        var anyControl = inputEventControlEnumerator.MoveNext();

        if (!anyControl) return;

        Scheme = device switch
        {
            Keyboard or Mouse => InputSchemeType.KeyboardAndMouse,
            Gamepad => InputSchemeType.Gamepad,
            _ => Scheme
        };
    }

    private async UniTask UpdateText()
    {
        // 現在のスキーマに合致するバインディングを探す
        var binding = inputActionReference.action.bindings.FirstOrDefault(IsBindingForCurrentScheme);
        if (string.IsNullOrEmpty(binding.path))
        {
            // バインディングが見つからない場合は非表示
            _image.enabled = false;

            // 前回ロードしたスプライトを解放
            if (_spriteHandle.IsValid())
                Addressables.Release(_spriteHandle);

            return;
        }

        // バインディングからAddressablesキーを生成
        var addressableKey = "Assets/Sprites/Input/" + GetSpriteNameFromBinding(binding) + ".png";
        if (string.IsNullOrEmpty(addressableKey))
        {
            _image.enabled = false;

            // 前回ロードしたスプライトを解放
            if (_spriteHandle.IsValid())
                Addressables.Release(_spriteHandle);

            return;
        }

        // Addressablesからスプライトをロード
        try
        {
            var oldHandle = _spriteHandle; // 古いハンドルを保存

            _spriteHandle = Addressables.LoadAssetAsync<Sprite>(addressableKey);
            var sprite = await _spriteHandle.ToUniTask();

            // 新しいスプライトを設定してから古いハンドルを解放
            _image.sprite = sprite;
            _image.enabled = true;

            // 古いハンドルを解放（新しいスプライト設定後）
            if (oldHandle.IsValid())
                Addressables.Release(oldHandle);
        }
        catch (Exception e)
        {
            if (!this) return;

            Debug.LogWarning($"Failed to load sprite: {addressableKey}. Error: {e.Message}");
            _image.enabled = false;

            // エラー時も古いハンドルを解放
            if (_spriteHandle.IsValid())
                Addressables.Release(_spriteHandle);
        }
    }

    private void OnEnable()
    {
        UpdateText().Forget();
        _schemeSubscription = _schemeChanged.Subscribe(_ => UpdateText().Forget());

        InputSystem.onEvent += OnEvent;
    }

    private void OnDisable()
    {
        _schemeSubscription?.Dispose();
        _schemeSubscription = null;

        InputSystem.onEvent -= OnEvent;

        // Addressablesハンドルを解放
        if (_spriteHandle.IsValid())
        {
            Addressables.Release(_spriteHandle);
        }
    }

    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.enabled = false;
    }

    /// <summary>
    /// binding.pathからAddressablesキーを生成する。
    /// InputSystemのバインディングパスを小文字化してスプライトパスに変換する。
    /// </summary>
    private string GetSpriteNameFromBinding(InputBinding binding)
    {
        if (string.IsNullOrEmpty(binding.path)) return "";

        // デバイス名を抽出: "<Keyboard>" → "Keyboard"
        var start = binding.path.IndexOf('<') + 1;
        var end = binding.path.IndexOf('>');
        if (start < 0 || end < 0 || end <= start) return "";

        var device = binding.path.Substring(start, end - start);

        // コントロール名を抽出: "/shift" → "shift", "/leftStick/up" → "leftstick/up"
        var slashIndex = binding.path.IndexOf('/');
        var control = "";
        if (slashIndex >= 0 && slashIndex < binding.path.Length - 1)
        {
            control = binding.path[(slashIndex + 1)..];
        }

        // Addressablesキーとして使用するため、小文字に統一
        // これにより、InputSystemの命名規則（camelCase）がファイル名（lowercase）と一致する
        return $"{device}/{control}".ToLower();
    }

    /// <summary>
    /// 現在の入力スキーマに該当するかを、バインディングの path を元に判定する
    /// </summary>
    private bool IsBindingForCurrentScheme(InputBinding binding)
    {
        if (string.IsNullOrEmpty(binding.path))
            return false;

        if (_scheme == InputSchemeType.KeyboardAndMouse)
        {
            // 例: "<Keyboard>/p" または "<Mouse>/leftButton" なら KeyboardAndMouse と判定
            return binding.path.StartsWith("<Keyboard>") || binding.path.StartsWith("<Mouse>");
        }
        else if (_scheme == InputSchemeType.Gamepad)
        {
            // 例: "<Gamepad>/buttonSouth" などの場合
            return binding.path.StartsWith("<Gamepad>") ||
                   binding.path.StartsWith("<XInputController>") ||
                   binding.path.StartsWith("<DualShockGamepad>");
        }
        return false;
    }
}
