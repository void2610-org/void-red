using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// TutorialStepのカスタムPropertyDrawer
/// 前ステップからマスク情報をコピーするボタンを提供
/// </summary>
[CustomPropertyDrawer(typeof(TutorialStep))]
public class TutorialStepPropertyDrawer : PropertyDrawer
{
    private static readonly float LineHeight = EditorGUIUtility.singleLineHeight;
    private static readonly float Spacing = EditorGUIUtility.standardVerticalSpacing;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return LineHeight;

        var messageProperty = property.FindPropertyRelative("message");
        var messageHeight = EditorGUI.GetPropertyHeight(messageProperty);

        // フォールドアウト + message + isPlayerDialog + maskPosition + maskSize
        var height = LineHeight + Spacing
            + messageHeight + Spacing
            + LineHeight + Spacing
            + LineHeight + Spacing
            + LineHeight + Spacing;

        // 前ステップが存在する場合のみボタン行を追加
        if (GetArrayIndex(property) > 0)
            height += LineHeight + Spacing;

        return height;
    }

    private static int GetArrayIndex(SerializedProperty property)
    {
        var match = Regex.Match(property.propertyPath, @"\[(\d+)\]$");
        return match.Success ? int.Parse(match.Groups[1].Value) : -1;
    }

    private static void CopyMaskFromPreviousStep(SerializedProperty property, int currentIndex)
    {
        var previousPath = Regex.Replace(
            property.propertyPath,
            @"\[\d+\]$",
            $"[{currentIndex - 1}]");

        var previousStep = property.serializedObject.FindProperty(previousPath);

        var prevPosition = previousStep.FindPropertyRelative("maskPosition");
        var prevSize = previousStep.FindPropertyRelative("maskSize");
        var currPosition = property.FindPropertyRelative("maskPosition");
        var currSize = property.FindPropertyRelative("maskSize");

        currPosition.vector2Value = prevPosition.vector2Value;
        currSize.vector2Value = prevSize.vector2Value;

        property.serializedObject.ApplyModifiedProperties();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var rect = new Rect(position.x, position.y, position.width, LineHeight);
        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            var messageProperty = property.FindPropertyRelative("message");
            var isPlayerDialogProperty = property.FindPropertyRelative("isPlayerDialog");
            var maskPositionProperty = property.FindPropertyRelative("maskPosition");
            var maskSizeProperty = property.FindPropertyRelative("maskSize");

            rect.y += LineHeight + Spacing;
            rect.height = EditorGUI.GetPropertyHeight(messageProperty);
            EditorGUI.PropertyField(rect, messageProperty);

            rect.y += rect.height + Spacing;
            rect.height = LineHeight;
            EditorGUI.PropertyField(rect, isPlayerDialogProperty);

            rect.y += LineHeight + Spacing;
            EditorGUI.PropertyField(rect, maskPositionProperty);

            rect.y += LineHeight + Spacing;
            EditorGUI.PropertyField(rect, maskSizeProperty);

            var arrayIndex = GetArrayIndex(property);
            if (arrayIndex > 0)
            {
                rect.y += LineHeight + Spacing;
                var buttonRect = EditorGUI.IndentedRect(rect);
                if (GUI.Button(buttonRect, "前ステップのマスク情報をコピー"))
                {
                    CopyMaskFromPreviousStep(property, arrayIndex);
                }
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }
}
