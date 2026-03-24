using UnityEngine;

/// <summary>
/// ヘルプデータを保持するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "NewHelpData", menuName = "VoidRed/Help Data")]
public class HelpData : ScriptableObject
{
    [Header("ヘルプ情報")]
    [SerializeField] private bool isImageOnly;
    [SerializeField] private string title;
    [SerializeField] private Sprite image;
    [TextArea(3, 10)]
    [SerializeField] private string description;

    public bool IsImageOnly => isImageOnly;
    public string Title => title;
    public Sprite Image => image;
    public string Description => description;
}
