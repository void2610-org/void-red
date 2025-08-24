using UnityEngine;
using UnityToolbarExtender;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class SceneSwitchLeftButton
{
    private const string TITLE_SCENE_PATH = "Assets/Scenes/TitleScene.unity";
    private const string HOME_SCENE_PATH = "Assets/Scenes/HomeScene.unity";
    private const string BATTLE_SCENE_PATH = "Assets/Scenes/BattleScene.unity";
    private const string NOVEL_SCENE_PATH = "Assets/Scenes/NovelScene.unity";
	
    static SceneSwitchLeftButton()
    {
        ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUILeft);
    }

    private static void OnToolbarGUILeft()
    {
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent("TITLE", "")))
            EditorSceneManager.OpenScene(TITLE_SCENE_PATH, OpenSceneMode.Single);
        if (GUILayout.Button(new GUIContent("HOME", "")))
            EditorSceneManager.OpenScene(HOME_SCENE_PATH, OpenSceneMode.Single);
        if (GUILayout.Button(new GUIContent("BATTLE", "")))
        	EditorSceneManager.OpenScene(BATTLE_SCENE_PATH, OpenSceneMode.Single);
        if (GUILayout.Button(new GUIContent("NOVEL", "")))
            EditorSceneManager.OpenScene(NOVEL_SCENE_PATH, OpenSceneMode.Single);
    }
}