using UnityEngine;

/// <summary>
/// TutorialDataのギズモをシーンビューで表示するためのヘルパーコンポーネント
/// TutorialDataアセットを設定すると、最後のステップの座標とサイズをギズモで表示
/// </summary>
public class TutorialGizmoHelper : MonoBehaviour
{
    [Header("チュートリアルデータ")]
    [SerializeField] private TutorialData tutorialData;
    
#if UNITY_EDITOR
    /// <summary>
    /// シーンビューでギズモを表示
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!tutorialData || tutorialData.StepCount == 0) return;
        
        // Canvas を取得してUI座標変換の基準にする
        var canvas = FindFirstObjectByType<Canvas>();
        if (!canvas) return;
        
        var canvasTransform = canvas.transform;
        
        // 全ステップを表示
        for (var i = 0; i < tutorialData.StepCount; i++)
        {
            var step = tutorialData.Steps[i];
            if (step == null) continue;
            
            // ステップ番号に応じて色を変更
            var alpha = i == tutorialData.StepCount - 1 ? 0.4f : 0.1f; // 最後のステップを強調
            var hue = (float)i / tutorialData.StepCount; // ステップ番号で色相を変更
            Gizmos.color = Color.HSVToRGB(hue, 0.7f, 1f);
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, alpha);
            
            // UI座標をワールド座標に変換
            var worldPos = canvasTransform.TransformPoint(new Vector3(step.MaskPosition.x, step.MaskPosition.y, 0f));
            var worldSize = Vector3.Scale(step.MaskSize, canvasTransform.lossyScale);
            
            // 矩形を描画
            Gizmos.DrawCube(worldPos, worldSize);
            
            // 枠線
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 1f);
            Gizmos.DrawWireCube(worldPos, worldSize);
        }
    }
#endif
}