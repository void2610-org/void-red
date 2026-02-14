using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StaggeredSlideInGroup))]
public class StaggeredSlideInGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var group = (StaggeredSlideInGroup)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("レイアウトを適用"))
        {
            group.ApplyLayout();
            if (!Application.isPlaying)
                EditorUtility.SetDirty(group);
        }

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("アニメーションをプレビュー"))
            {
                group.Play();
            }
        }
    }
}
