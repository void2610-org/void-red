using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1つのセリフを表すデータクラス
/// 話者、セリフ内容、SE情報などを含む
/// </summary>
[Serializable]
public class DialogData
{
    [Header("セリフ情報")]
    [SerializeField] private string speakerName;
    [SerializeField, TextArea(3, 5)] private string dialogText;
    
    [Header("画像情報")]
    [SerializeField] private string characterImageName; // キャラクター画像の名前
    
    [Header("音響効果")]
    [SerializeField] private string seClipName;
    
    [Header("表示設定")]
    [SerializeField] private float customCharSpeed; // -1の場合はデフォルト速度を使用
    [SerializeField] private bool autoAdvance = false; // 自動で次に進むかどうか
    
    [Header("動的パラメータ")]
    [SerializeField] private Dictionary<DialogParameterType, object> parameters = new Dictionary<DialogParameterType, object>();
    
    /// <summary>
    /// 話者名
    /// </summary>
    public string SpeakerName => speakerName;
    
    /// <summary>
    /// セリフの内容
    /// </summary>
    public string DialogText => dialogText;
    
    /// <summary>
    /// キャラクター画像の名前
    /// </summary>
    public string CharacterImageName => GetParameterValue<string>(DialogParameterType.CharacterImageName, characterImageName);
    
    /// <summary>
    /// 再生するSEのクリップ名
    /// </summary>
    public string SeClipName => GetParameterValue<string>(DialogParameterType.SEClipName, seClipName);
    
    
    /// <summary>
    /// カスタム文字速度（-1の場合はデフォルト速度）
    /// </summary>
    public float CustomCharSpeed => GetParameterValue<float>(DialogParameterType.CustomCharSpeed, customCharSpeed);
    
    /// <summary>
    /// 自動で次に進むかどうか
    /// </summary>
    public bool AutoAdvance => GetParameterValue<bool>(DialogParameterType.AutoAdvance, autoAdvance);
    
    
    /// <summary>
    /// セリフに有効なテキストが含まれているかどうか
    /// </summary>
    public bool HasValidText => !string.IsNullOrEmpty(dialogText);
    
    /// <summary>
    /// SEが設定されているかどうか
    /// </summary>
    public bool HasSe => !string.IsNullOrEmpty(seClipName);
    
    /// <summary>
    /// デフォルト文字速度を使用するかどうか
    /// </summary>
    public bool UseDefaultCharSpeed => customCharSpeed < 0f;
    
    /// <summary>
    /// コンストラクタ（従来の固定パラメータ用）
    /// </summary>
    public DialogData(string speakerName, string dialogText, string characterImageName = "", string seClipName = "", float customCharSpeed = -1f)
    {
        this.speakerName = speakerName;
        this.dialogText = dialogText;
        this.characterImageName = characterImageName;
        this.seClipName = seClipName;
        this.customCharSpeed = customCharSpeed;
        this.autoAdvance = false;
        this.parameters = new Dictionary<DialogParameterType, object>();
    }
    
    /// <summary>
    /// コンストラクタ（動的パラメータ対応）
    /// </summary>
    public DialogData(string speakerName, string dialogText, Dictionary<DialogParameterType, object> parameters)
    {
        this.speakerName = speakerName;
        this.dialogText = dialogText;
        this.parameters = parameters ?? new Dictionary<DialogParameterType, object>();
        
        // デフォルト値を設定（後方互換性のため）
        this.characterImageName = "";
        this.seClipName = "";
        this.customCharSpeed = -1f;
        this.autoAdvance = false;
    }
    
    /// <summary>
    /// パラメータ値を取得する
    /// </summary>
    /// <typeparam name="T">取得する値の型</typeparam>
    /// <param name="parameterType">パラメータタイプ</param>
    /// <param name="defaultValue">デフォルト値</param>
    /// <returns>パラメータ値またはデフォルト値</returns>
    private T GetParameterValue<T>(DialogParameterType parameterType, T defaultValue)
    {
        if (parameters.TryGetValue(parameterType, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return defaultValue;
    }
    
    /// <summary>
    /// パラメータを設定する
    /// </summary>
    /// <param name="parameterType">パラメータタイプ</param>
    /// <param name="value">設定する値</param>
    public void SetParameter(DialogParameterType parameterType, object value)
    {
        parameters[parameterType] = value;
    }
    
    /// <summary>
    /// 特定のパラメータが設定されているかチェック
    /// </summary>
    /// <param name="parameterType">パラメータタイプ</param>
    /// <returns>設定されている場合はtrue</returns>
    public bool HasParameter(DialogParameterType parameterType)
    {
        return parameters.ContainsKey(parameterType);
    }
}
