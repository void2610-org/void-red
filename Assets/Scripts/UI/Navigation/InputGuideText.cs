using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[RequireComponent(typeof(TextMeshProUGUI))]
public class InputGuideText : MonoBehaviour
{
    public enum InputSchemeType
    {
        KeyboardAndMouse,
        Gamepad
    }

    [Serializable]
    public class InputGuideData
    {
        public string actionName;
        public InputActionReference actionReference;
    }

    [SerializeField] private InputGuideData inputGuideData;
    
    public event Action<InputSchemeType> OnSchemeChanged;
    
    private Action<InputSchemeType> _onSchemeChanged;
    private InputSchemeType _scheme = InputSchemeType.KeyboardAndMouse;
    private TextMeshProUGUI _text;

    private InputSchemeType Scheme
    {
        get => _scheme;
        set
        {
            if (_scheme == value) return;
            _scheme = value;
            OnSchemeChanged?.Invoke(_scheme);
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
    
    private void UpdateText()
    {
        var action = inputGuideData.actionReference.action;
        // 現在のスキーマに合致するバインディングを探す
        
        // TODO: Addressablesでスプライトを管理する
    }
    
    private void OnEnable()
    {
        UpdateText();
        OnSchemeChanged += _onSchemeChanged = _ => UpdateText();
        
        InputSystem.onEvent += OnEvent;
    }

    private void OnDisable()
    {
        if (_onSchemeChanged != null)
        {
            OnSchemeChanged -= _onSchemeChanged;
            _onSchemeChanged = null;
        }
        
        InputSystem.onEvent -= OnEvent;
    }
    
    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// binding.pathからAddressablesキーを生成する。
    /// InputSystemのバインディングパスを小文字化してスプライトパスに変換する。
    ///
    /// 例:
    /// - "<Keyboard>/space" → "keyboard/space"
    /// - "<Keyboard>/upArrow" → "keyboard/uparrow"
    /// - "<Mouse>/leftButton" → "mouse/leftbutton"
    /// - "<Gamepad>/buttonSouth" → "gamepad/buttonsouth"
    /// - "<Gamepad>/leftStick/up" → "gamepad/leftstick/up"
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
