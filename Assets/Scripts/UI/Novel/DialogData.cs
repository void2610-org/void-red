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
    [SerializeField] private string speakerName;
    [SerializeField] private string dialogText;
    [SerializeField] private string characterImageName;
    
    private Dictionary<DialogParameterType, object> _parameters;
    
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
    /// 背景画像の名前
    /// </summary>
    public string BackgroundImageName => GetParameterValue<string>(DialogParameterType.BackgroundImageName, "");
    
    /// <summary>
    /// 再生するSEのクリップ名
    /// </summary>
    public string SeClipName => GetParameterValue<string>(DialogParameterType.SeClipName, "");
    
    
    /// <summary>
    /// カスタム文字速度（-1の場合はデフォルト速度）
    /// </summary>
    public float CustomCharSpeed => GetParameterValue<float>(DialogParameterType.CustomCharSpeed, -1f);
    
    /// <summary>
    /// 自動で次に進むかどうか
    /// </summary>
    public bool AutoAdvance => GetParameterValue<bool>(DialogParameterType.AutoAdvance, false);
    
    /// <summary>
    /// SEが設定されているかどうか
    /// </summary>
    public bool HasSe => !string.IsNullOrEmpty(SeClipName);
    
    /// <summary>
    /// 背景画像が設定されているかどうか
    /// </summary>
    public bool HasBackgroundImage => !string.IsNullOrEmpty(BackgroundImageName);
    
    /// <summary>
    /// カスタム文字速度が設定されているかどうか
    /// </summary>
    public bool HasCustomCharSpeed => CustomCharSpeed > 0f;
    
    /// <summary>
    /// コンストラクタ（動的パラメータ対応版）
    /// </summary>
    public DialogData(string speakerName, string dialogText, Dictionary<DialogParameterType, object> parameters)
    {
        this.speakerName = speakerName;
        this.dialogText = dialogText;
        this.characterImageName = "";
        
        this._parameters = parameters ?? new Dictionary<DialogParameterType, object>();
    }
    
    /// <summary>
    /// コンストラクタ（簡易版）
    /// </summary>
    public DialogData(string speakerName, string dialogText, string characterImageName = "")
    {
        this.speakerName = speakerName;
        this.dialogText = dialogText;
        this.characterImageName = characterImageName;
        
        this._parameters = new Dictionary<DialogParameterType, object>();
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
        if (_parameters.TryGetValue(parameterType, out var value) && value is T typedValue)
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
        _parameters[parameterType] = value;
    }
    
    /// <summary>
    /// 特定のパラメータが設定されているかチェック
    /// </summary>
    /// <param name="parameterType">パラメータタイプ</param>
    /// <returns>設定されている場合はtrue</returns>
    public bool HasParameter(DialogParameterType parameterType)
    {
        return _parameters.ContainsKey(parameterType);
    }
}
