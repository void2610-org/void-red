using UnityEngine;
using UnityEditor;

/// <summary>
/// RandomRangeValueのカスタムPropertyDrawer
/// </summary>
[CustomPropertyDrawer(typeof(RandomRangeValue))]
public class RandomRangeValueDrawer : PropertyDrawer
{
    private const float LINE_HEIGHT = 18f;
    private const float SPACING = 2f;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        var useRandomRange = property.FindPropertyRelative("useRandomRange");
        var fixedValue = property.FindPropertyRelative("fixedValue");
        var minValue = property.FindPropertyRelative("minValue");
        var maxValue = property.FindPropertyRelative("maxValue");
        
        // メインラベルを描画
        var labelRect = new Rect(position.x, position.y, position.width, LINE_HEIGHT);
        EditorGUI.LabelField(labelRect, label);
        
        // インデントを適用
        position.x += EditorGUI.indentLevel * 15f;
        position.width -= EditorGUI.indentLevel * 15f;
        
        // チェックボックス
        var toggleRect = new Rect(position.x, position.y + LINE_HEIGHT + SPACING, position.width, LINE_HEIGHT);
        useRandomRange.boolValue = EditorGUI.Toggle(toggleRect, "ランダム", useRandomRange.boolValue);
        
        // 値の表示位置
        var valueY = position.y + (LINE_HEIGHT + SPACING) * 2;
        
        if (useRandomRange.boolValue)
        {
            // ランダム範囲の場合：最小値と最大値を縦に配置
            var minRect = new Rect(position.x, valueY, position.width, LINE_HEIGHT);
            minValue.intValue = EditorGUI.IntField(minRect, "最小値", minValue.intValue);
            
            var maxRect = new Rect(position.x, valueY + LINE_HEIGHT + SPACING, position.width, LINE_HEIGHT);
            maxValue.intValue = EditorGUI.IntField(maxRect, "最大値", maxValue.intValue);
        }
        else
        {
            // 固定値の場合
            var fixedRect = new Rect(position.x, valueY, position.width, LINE_HEIGHT);
            fixedValue.intValue = EditorGUI.IntField(fixedRect, "固定値", fixedValue.intValue);
        }
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var useRandomRange = property.FindPropertyRelative("useRandomRange");
        if (useRandomRange.boolValue)
        {
            // ラベル + チェックボックス + 最小値 + 最大値
            return (LINE_HEIGHT + SPACING) * 4;
        }
        else
        {
            // ラベル + チェックボックス + 固定値
            return (LINE_HEIGHT + SPACING) * 3;
        }
    }
}

/// <summary>
/// RandomRangeFloatのカスタムPropertyDrawer
/// </summary>
[CustomPropertyDrawer(typeof(RandomRangeFloat))]
public class RandomRangeFloatDrawer : PropertyDrawer
{
    private const float LINE_HEIGHT = 18f;
    private const float SPACING = 2f;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        var useRandomRange = property.FindPropertyRelative("useRandomRange");
        var fixedValue = property.FindPropertyRelative("fixedValue");
        var minValue = property.FindPropertyRelative("minValue");
        var maxValue = property.FindPropertyRelative("maxValue");
        
        // メインラベルを描画
        var labelRect = new Rect(position.x, position.y, position.width, LINE_HEIGHT);
        EditorGUI.LabelField(labelRect, label);
        
        // インデントを適用
        position.x += EditorGUI.indentLevel * 15f;
        position.width -= EditorGUI.indentLevel * 15f;
        
        // チェックボックス
        var toggleRect = new Rect(position.x, position.y + LINE_HEIGHT + SPACING, position.width, LINE_HEIGHT);
        useRandomRange.boolValue = EditorGUI.Toggle(toggleRect, "ランダム", useRandomRange.boolValue);
        
        // 値の表示位置
        var valueY = position.y + (LINE_HEIGHT + SPACING) * 2;
        
        if (useRandomRange.boolValue)
        {
            // ランダム範囲の場合：最小値と最大値を縦に配置
            var minRect = new Rect(position.x, valueY, position.width, LINE_HEIGHT);
            minValue.floatValue = EditorGUI.FloatField(minRect, "最小値", minValue.floatValue);
            
            var maxRect = new Rect(position.x, valueY + LINE_HEIGHT + SPACING, position.width, LINE_HEIGHT);
            maxValue.floatValue = EditorGUI.FloatField(maxRect, "最大値", maxValue.floatValue);
        }
        else
        {
            // 固定値の場合
            var fixedRect = new Rect(position.x, valueY, position.width, LINE_HEIGHT);
            fixedValue.floatValue = EditorGUI.FloatField(fixedRect, "固定値", fixedValue.floatValue);
        }
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var useRandomRange = property.FindPropertyRelative("useRandomRange");
        if (useRandomRange.boolValue)
        {
            // ラベル + チェックボックス + 最小値 + 最大値
            return (LINE_HEIGHT + SPACING) * 4;
        }
        else
        {
            // ラベル + チェックボックス + 固定値
            return (LINE_HEIGHT + SPACING) * 3;
        }
    }
}