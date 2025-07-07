using UnityEngine;
using UnityEditor;

/// <summary>
/// ランダム範囲を持つ条件の共通プロパティドロワー
/// </summary>
public static class RandomRangeConditionDrawer
{
    /// <summary>
    /// RandomRangeValueを描画
    /// </summary>
    public static void DrawRandomRangeValue(SerializedProperty property, string label = "値")
    {
        EditorGUILayout.PropertyField(property, new GUIContent(label), true);
    }
    
    /// <summary>
    /// RandomRangeFloatを描画
    /// </summary>
    public static void DrawRandomRangeFloat(SerializedProperty property, string label = "値")
    {
        EditorGUILayout.PropertyField(property, new GUIContent(label), true);
    }
}

// 各条件タイプのカスタムPropertyDrawer
[CustomPropertyDrawer(typeof(PlayStyleWinCondition))]
public class PlayStyleWinConditionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position.height = EditorGUIUtility.singleLineHeight;
        
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);
        
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            var playStyle = property.FindPropertyRelative("requiredPlayStyle");
            EditorGUILayout.PropertyField(playStyle);
            var requiredCount = property.FindPropertyRelative("requiredCount");
            RandomRangeConditionDrawer.DrawRandomRangeValue(requiredCount, "必要勝利数");
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return property.isExpanded ? EditorGUIUtility.singleLineHeight * 4 : EditorGUIUtility.singleLineHeight;
    }
}

[CustomPropertyDrawer(typeof(PlayStyleLoseCondition))]
public class PlayStyleLoseConditionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position.height = EditorGUIUtility.singleLineHeight;
        
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);
        
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            var playStyle = property.FindPropertyRelative("requiredPlayStyle");
            EditorGUILayout.PropertyField(playStyle);
            var requiredCount = property.FindPropertyRelative("requiredCount");
            RandomRangeConditionDrawer.DrawRandomRangeValue(requiredCount, "必要敗北数");
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return property.isExpanded ? EditorGUIUtility.singleLineHeight * 4 : EditorGUIUtility.singleLineHeight;
    }
}

[CustomPropertyDrawer(typeof(WinRateCondition))]
public class WinRateConditionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position.height = EditorGUIUtility.singleLineHeight;
        
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);
        
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            var requiredWinRate = property.FindPropertyRelative("requiredWinRate");
            RandomRangeConditionDrawer.DrawRandomRangeFloat(requiredWinRate, "必要勝率");
            var minimumGames = property.FindPropertyRelative("minimumGames");
            EditorGUILayout.PropertyField(minimumGames, new GUIContent("最低試合数"));
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.EndProperty();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return property.isExpanded ? EditorGUIUtility.singleLineHeight * 4 : EditorGUIUtility.singleLineHeight;
    }
}