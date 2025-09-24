using System;
using System.Collections.Generic;

/// <summary>
/// ダイアログパラメータのタイプを定義するenum
/// スプレッドシートの動的パラメータシステムで使用
/// </summary>
public enum DialogParameterType
{
    /// <summary>
    /// キャラクター画像名（string）
    /// </summary>
    CharacterImageName,
    
    /// <summary>
    /// 背景画像名（string）
    /// </summary>
    BackgroundImageName,
    
    /// <summary>
    /// SEクリップ名（string）
    /// </summary>
    SeClipName,
    
    /// <summary>
    /// カスタム文字速度（float）
    /// </summary>
    CustomCharSpeed,
    
    /// <summary>
    /// 自動で次に進むまでの時間（float、0以下の場合は自動進行なし）
    /// </summary>
    AutoAdvance,
    
    /// <summary>
    /// アイテム取得演出（List&lt;string&gt; - 画像名,アイテム名,アイテム説明）
    /// </summary>
    GetItem
}

/// <summary>
/// DialogParameterType の拡張メソッド
/// </summary>
public static class DialogParameterTypeExtensions
{
    /// <summary>
    /// 文字列からDialogParameterTypeに変換
    /// </summary>
    /// <param name="typeString">パラメータタイプの文字列</param>
    /// <param name="parameterType">変換されたパラメータタイプ</param>
    /// <returns>変換に成功した場合はtrue</returns>
    public static bool TryParseParameterType(string typeString, out DialogParameterType parameterType)
    {
        return Enum.TryParse(typeString, true, out parameterType);
    }
    
    /// <summary>
    /// パラメータタイプの期待する値の型を取得
    /// </summary>
    /// <param name="parameterType">パラメータタイプ</param>
    /// <returns>期待する値の型</returns>
    public static Type GetExpectedValueType(this DialogParameterType parameterType)
    {
        return parameterType switch
        {
            DialogParameterType.CharacterImageName => typeof(string),
            DialogParameterType.BackgroundImageName => typeof(string),
            DialogParameterType.SeClipName => typeof(string),
            DialogParameterType.CustomCharSpeed => typeof(float),
            DialogParameterType.AutoAdvance => typeof(float),
            DialogParameterType.GetItem => typeof(List<string>),
            _ => typeof(string)
        };
    }
    
    /// <summary>
    /// 文字列値を適切な型に変換
    /// </summary>
    /// <param name="parameterType">パラメータタイプ</param>
    /// <param name="valueString">変換する文字列値</param>
    /// <returns>変換された値</returns>
    public static object ConvertValue(this DialogParameterType parameterType, string valueString)
    {
        if (string.IsNullOrEmpty(valueString))
        {
            return parameterType.GetDefaultValue();
        }
        
        return parameterType switch
        {
            DialogParameterType.CharacterImageName => valueString,
            DialogParameterType.BackgroundImageName => valueString,
            DialogParameterType.SeClipName => valueString,
            DialogParameterType.CustomCharSpeed => float.TryParse(valueString, out var floatValue) ? floatValue : -1f,
            DialogParameterType.AutoAdvance => float.TryParse(valueString, out var autoValue) ? autoValue : -1f,
            DialogParameterType.GetItem => ParseCommaSeparatedList(valueString),
            _ => valueString
        };
    }
    
    /// <summary>
    /// パラメータタイプのデフォルト値を取得
    /// </summary>
    /// <param name="parameterType">パラメータタイプ</param>
    /// <returns>デフォルト値</returns>
    public static object GetDefaultValue(this DialogParameterType parameterType)
    {
        return parameterType switch
        {
            DialogParameterType.CharacterImageName => "",
            DialogParameterType.BackgroundImageName => "",
            DialogParameterType.SeClipName => "",
            DialogParameterType.CustomCharSpeed => -1f,
            DialogParameterType.AutoAdvance => -1f,
            DialogParameterType.GetItem => new List<string>(),
            _ => ""
        };
    }
    
    /// <summary>
    /// カンマ区切りの文字列をList&lt;string&gt;に変換
    /// </summary>
    /// <param name="commaSeparatedString">カンマ区切りの文字列</param>
    /// <returns>文字列のリスト</returns>
    private static List<string> ParseCommaSeparatedList(string commaSeparatedString)
    {
        if (string.IsNullOrEmpty(commaSeparatedString))
            return new List<string>();
        
        var items = new List<string>();
        var parts = commaSeparatedString.Split(',');
        
        foreach (var part in parts)
        {
            var trimmedPart = part.Trim();
            if (!string.IsNullOrEmpty(trimmedPart))
                items.Add(trimmedPart);
        }
        
        return items;
    }
}