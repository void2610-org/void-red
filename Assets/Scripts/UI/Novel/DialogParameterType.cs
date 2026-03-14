using System;

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
    /// アイテム取得演出（ItemGetData - 画像名,アイテム名,アイテム説明）
    /// </summary>
    GetItem,

    /// <summary>
    /// 選択肢表示（ChoiceData - 質問文,選択肢1,選択肢2,...)
    /// </summary>
    Choice,

    /// <summary>
    /// カード風選択肢表示（CardChoiceData - [選択肢1,画像1]/[選択肢2,画像2]）
    /// </summary>
    CardChoice,

    /// <summary>
    /// カード獲得演出（パラメータなし - 列が存在すれば発火。選択結果に基づいてカードを決定）
    /// </summary>
    GetCard
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
    public static bool TryParseParameterType(string typeString, out DialogParameterType parameterType) => Enum.TryParse(typeString, true, out parameterType);

    /// <summary>
    /// パラメータタイプの期待する値の型を取得
    /// </summary>
    /// <param name="parameterType">パラメータタイプ</param>
    /// <returns>期待する値の型</returns>
    public static Type GetExpectedValueType(this DialogParameterType parameterType) => parameterType switch
    {
        DialogParameterType.CharacterImageName => typeof(string),
        DialogParameterType.BackgroundImageName => typeof(string),
        DialogParameterType.SeClipName => typeof(string),
        DialogParameterType.CustomCharSpeed => typeof(float),
        DialogParameterType.AutoAdvance => typeof(float),
        DialogParameterType.GetItem => typeof(ItemGetData),
        DialogParameterType.Choice => typeof(ChoiceData),
        DialogParameterType.CardChoice => typeof(CardChoiceData),
        DialogParameterType.GetCard => typeof(bool),
        _ => typeof(string)
    };

    /// <summary>
    /// パラメータタイプのデフォルト値を取得
    /// </summary>
    /// <param name="parameterType">パラメータタイプ</param>
    /// <returns>デフォルト値</returns>
    public static object GetDefaultValue(this DialogParameterType parameterType) => parameterType switch
    {
        DialogParameterType.CharacterImageName => "",
        DialogParameterType.BackgroundImageName => "",
        DialogParameterType.SeClipName => "",
        DialogParameterType.CustomCharSpeed => -1f,
        DialogParameterType.AutoAdvance => -1f,
        DialogParameterType.GetItem => null,
        DialogParameterType.Choice => null,
        DialogParameterType.CardChoice => null,
        DialogParameterType.GetCard => false,
        _ => ""
    };

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
            DialogParameterType.GetItem => ItemGetData.FromCommaSeparatedString(valueString),
            DialogParameterType.Choice => ChoiceData.FromCommaSeparatedString(valueString),
            DialogParameterType.CardChoice => CardChoiceData.FromSlashSeparatedString(valueString),
            DialogParameterType.GetCard => true,
            _ => valueString
        };
    }
}
