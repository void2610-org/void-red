using UnityEditor;
using UnityEngine;

/// <summary>
/// MemoryType enumのカスタムProperty Drawer
/// Inspectorで日本語名を表示する
/// </summary>
[CustomPropertyDrawer(typeof(MemoryType))]
public class MemoryTypePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var enumValues = System.Enum.GetValues(typeof(MemoryType));
        var options = new string[enumValues.Length];

        for (var i = 0; i < enumValues.Length; i++)
        {
            var value = (MemoryType)enumValues.GetValue(i);
            options[i] = $"{value} ({value.ToJapaneseName()})";
        }

        var selectedIndex = EditorGUI.Popup(position, label.text, property.enumValueIndex, options);
        property.enumValueIndex = selectedIndex;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
}
