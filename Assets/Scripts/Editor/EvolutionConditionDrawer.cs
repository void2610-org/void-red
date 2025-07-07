using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;

/// <summary>
/// SerializeReferenceで使用されるEvolutionConditionBaseのカスタムエディタ拡張
/// </summary>
public static class EvolutionConditionDrawer
{
    /// <summary>
    /// 利用可能な条件タイプのリスト
    /// </summary>
    public static readonly Type[] ConditionTypes = new[]
    {
        typeof(PlayStyleWinCondition),
        typeof(PlayStyleLoseCondition),
        typeof(TotalWinCondition),
        typeof(CollapseCountCondition),
        typeof(ConsecutiveWinCondition),
        typeof(TotalUseCondition),
        typeof(WinRateCondition)
    };
    
    /// <summary>
    /// 条件タイプの表示名
    /// </summary>
    public static readonly string[] ConditionTypeNames = new[]
    {
        "プレイスタイル勝利",
        "プレイスタイル敗北",
        "総勝利数",
        "崩壊回数",
        "連続勝利",
        "総使用回数",
        "勝率"
    };
}

/// <summary>
/// CardDataのカスタムエディタ
/// </summary>
[CustomEditor(typeof(CardData))]
public class CardDataEditor : Editor
{
    private SerializedProperty _evolutionConditionGroups;
    private SerializedProperty _degradationConditionGroups;
    private bool _showEvolutionConditions = true;
    private bool _showDegradationConditions = true;
    
    private void OnEnable()
    {
        _evolutionConditionGroups = serializedObject.FindProperty("evolutionConditionGroups");
        _degradationConditionGroups = serializedObject.FindProperty("degradationConditionGroups");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // デフォルトのインスペクターを描画（条件以外）
        DrawPropertiesExcluding(serializedObject, "evolutionConditionGroups", "degradationConditionGroups");
        
        EditorGUILayout.Space();
        
        // 進化条件
        _showEvolutionConditions = EditorGUILayout.Foldout(_showEvolutionConditions, "進化条件", true);
        if (_showEvolutionConditions)
        {
            EditorGUI.indentLevel++;
            DrawConditionGroupsList(_evolutionConditionGroups, "進化");
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // 劣化条件
        _showDegradationConditions = EditorGUILayout.Foldout(_showDegradationConditions, "劣化条件", true);
        if (_showDegradationConditions)
        {
            EditorGUI.indentLevel++;
            DrawConditionGroupsList(_degradationConditionGroups, "劣化");
            EditorGUI.indentLevel--;
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private static void DrawConditionGroupsList(SerializedProperty groupsProperty, string conditionType)
    {
        EditorGUILayout.LabelField($"{conditionType}条件グループ（いずれか1つのグループを満たせば{conditionType}）", EditorStyles.boldLabel);
        
        for (int i = 0; i < groupsProperty.arraySize; i++)
        {
            var groupElement = groupsProperty.GetArrayElementAtIndex(i);
            var conditionsProperty = groupElement.FindPropertyRelative("conditions");
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            
            // グループヘッダー
            var groupLabel = conditionsProperty.arraySize == 1 ? 
                $"グループ {i + 1} (1つの条件)" : 
                $"グループ {i + 1} (全て満たす必要がある{conditionsProperty.arraySize}つの条件)";
            EditorGUILayout.LabelField(groupLabel, EditorStyles.boldLabel);
            
            // グループ削除ボタン
            if (GUILayout.Button("削除", GUILayout.Width(50)))
            {
                groupsProperty.DeleteArrayElementAtIndex(i);
                break;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel++;
            
            // グループ内の条件を描画
            for (int j = 0; j < conditionsProperty.arraySize; j++)
            {
                var conditionElement = conditionsProperty.GetArrayElementAtIndex(j);
                var condition = conditionElement.managedReferenceValue as EvolutionConditionBase;
                
                EditorGUILayout.BeginVertical("helpbox");
                EditorGUILayout.BeginHorizontal();
                
                // 条件タイプ名を表示
                var typeName = condition?.GetConditionTypeName() ?? "未設定";
                EditorGUILayout.LabelField($"条件 {j + 1}: {typeName}", EditorStyles.boldLabel);
                
                // 条件変更ボタン
                if (GUILayout.Button("変更", GUILayout.Width(50)))
                {
                    ShowConditionTypeMenu(conditionElement);
                }
                
                // 条件削除ボタン
                if (GUILayout.Button("削除", GUILayout.Width(50)))
                {
                    conditionsProperty.DeleteArrayElementAtIndex(j);
                    break;
                }
                
                EditorGUILayout.EndHorizontal();
                
                // 条件の詳細を描画
                if (condition != null)
                {
                    DrawConditionDetails(conditionElement, condition);
                    
                    // 説明文を表示
                    EditorGUILayout.HelpBox($"説明: {condition.GetDescription()}", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("条件が設定されていません", MessageType.Warning);
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            
            // グループ内に条件を追加ボタン
            if (GUILayout.Button("このグループに条件を追加"))
            {
                conditionsProperty.arraySize++;
                var newElement = conditionsProperty.GetArrayElementAtIndex(conditionsProperty.arraySize - 1);
                ShowConditionTypeMenu(newElement);
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        // 新しいグループ追加ボタン
        if (GUILayout.Button($"新しい{conditionType}条件グループを追加"))
        {
            groupsProperty.arraySize++;
            var newGroup = groupsProperty.GetArrayElementAtIndex(groupsProperty.arraySize - 1);
            var newConditions = newGroup.FindPropertyRelative("conditions");
            newConditions.arraySize = 1;
            var newElement = newConditions.GetArrayElementAtIndex(0);
            ShowConditionTypeMenu(newElement);
        }
    }
    
    
    /// <summary>
    /// 条件タイプ選択メニューを表示
    /// </summary>
    private static void ShowConditionTypeMenu(SerializedProperty conditionElement)
    {
        var menu = new GenericMenu();
        
        for (var i = 0; i < EvolutionConditionDrawer.ConditionTypes.Length; i++)
        {
            var type = EvolutionConditionDrawer.ConditionTypes[i];
            var typeName = EvolutionConditionDrawer.ConditionTypeNames[i];
            menu.AddItem(new GUIContent(typeName), false, () =>
            {
                conditionElement.managedReferenceValue = Activator.CreateInstance(type);
                conditionElement.serializedObject.ApplyModifiedProperties();
            });
        }
        
        menu.ShowAsContext();
    }
    
    /// <summary>
    /// 条件の詳細を描画（再帰的に呼び出すため分離）
    /// </summary>
    private static void DrawConditionDetails(SerializedProperty element, EvolutionConditionBase condition)
    {
        EditorGUI.indentLevel++;
        
        // プレイスタイル条件
        var requiredPlayStyle = element.FindPropertyRelative("requiredPlayStyle");
        if (requiredPlayStyle != null)
        {
            EditorGUILayout.PropertyField(requiredPlayStyle);
        }
        
        // カウント系条件
        if (condition is PlayStyleWinCondition || condition is PlayStyleLoseCondition ||
            condition is TotalWinCondition || condition is CollapseCountCondition ||
            condition is ConsecutiveWinCondition || condition is TotalUseCondition)
        {
            var requiredCountProperty = element.FindPropertyRelative("requiredCount");
            var countLabel = condition switch
            {
                PlayStyleWinCondition => "必要勝利数",
                PlayStyleLoseCondition => "必要敗北数",
                TotalWinCondition => "必要勝利数",
                CollapseCountCondition => "必要崩壊数",
                ConsecutiveWinCondition => "必要連続勝利数",
                TotalUseCondition => "必要使用回数",
                _ => "必要数"
            };
            RandomRangeConditionDrawer.DrawRandomRangeValue(requiredCountProperty, countLabel);
        }
        // 勝率条件
        else if (condition is WinRateCondition)
        {
            var requiredWinRateProperty = element.FindPropertyRelative("requiredWinRate");
            RandomRangeConditionDrawer.DrawRandomRangeFloat(requiredWinRateProperty, "必要勝率");
            
            var minimumGames = element.FindPropertyRelative("minimumGames");
            if (minimumGames != null)
            {
                EditorGUILayout.PropertyField(minimumGames, new GUIContent("最低試合数"));
            }
        }
        
        EditorGUI.indentLevel--;
    }
}