using UnityEngine;

/// <summary>
/// テーマデータを保持するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "NewThemeData", menuName = "VoidRed/Theme Data")]
public class ThemeData : ScriptableObject
{
    [SerializeField] private string title;
    [TextArea(2, 4)][SerializeField] private string description;

    public string ThemeId => this.name;
    public string Title => title;
    public string Description => description;
}
