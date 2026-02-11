using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敵の顔アイコンを表示するView
/// </summary>
public class EnemyFaceView : MonoBehaviour
{
    [SerializeField] private Image faceIcon;

    public void Initialize(EnemyData enemyData) => faceIcon.sprite = enemyData.IconSprite;
}
