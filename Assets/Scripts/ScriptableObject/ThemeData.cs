using UnityEngine;

/// <summary>
/// テーマデータを保持するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "NewThemeData", menuName = "VoidRed/Theme Data")]
public class ThemeData : ScriptableObject
{
    [Header("テーマ情報")]
    [SerializeField] private string themeId;
    [SerializeField] private string title;
    [TextArea(2, 4)] [SerializeField] private string description;

    /// <summary>
    /// テーマの一意ID
    /// </summary>
    public string ThemeId => themeId;

    public string Title => title;
    public string Description => description;
}