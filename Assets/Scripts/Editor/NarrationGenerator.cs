using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Void2610.UnityTemplate;

/// <summary>
/// CardDataアセットにテスト用の語り文を自動設定するEditorスクリプト
/// </summary>
public class NarrationGenerator : EditorWindow
{
    [MenuItem("VoidRed/語り文自動生成")]
    public static void GenerateNarrations()
    {
        var cardDataGuids = AssetDatabase.FindAssets("t:CardData");
        var updatedCount = 0;
        
        foreach (var guid in cardDataGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var cardData = AssetDatabase.LoadAssetAtPath<CardData>(path);
            
            if (cardData == null) continue;
            
            // 語り文を生成
            var narrations = new SerializableDictionary<PlayStyle, string>();
            
            narrations[PlayStyle.Hesitation] = $"語り文 {cardData.CardName} 迷い";
            narrations[PlayStyle.Impulse] = $"語り文 {cardData.CardName} 衝動";
            narrations[PlayStyle.Conviction] = $"語り文 {cardData.CardName} 確信";
            
            // SerializationObjectを使用してアセットを更新
            var serializedObject = new SerializedObject(cardData);
            var narrationProperty = serializedObject.FindProperty("narrationByPlayStyle");
            
            if (narrationProperty != null)
            {
                // SerializableDictionaryの内部構造に直接アクセス
                var serializedListProperty = narrationProperty.FindPropertyRelative("_serializedList");
                
                if (serializedListProperty != null)
                {
                    serializedListProperty.ClearArray();
                    
                    // 各PlayStyleに対してエントリを追加
                    foreach (var kvp in narrations)
                    {
                        serializedListProperty.InsertArrayElementAtIndex(serializedListProperty.arraySize);
                        var elementProperty = serializedListProperty.GetArrayElementAtIndex(serializedListProperty.arraySize - 1);
                        
                        var keyProperty = elementProperty.FindPropertyRelative("key");
                        var valueProperty = elementProperty.FindPropertyRelative("value");
                        
                        if (keyProperty != null && valueProperty != null)
                        {
                            keyProperty.enumValueIndex = (int)kvp.Key;
                            valueProperty.stringValue = kvp.Value;
                        }
                    }
                    
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(cardData);
                    updatedCount++;
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"語り文を{updatedCount}個のCardDataに設定しました。");
    }
}