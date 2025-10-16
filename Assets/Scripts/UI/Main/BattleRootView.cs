using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// バトルシーンのルートビュー
/// UIナビゲーションの起点となる選択可能な要素を持つ
/// </summary>
[RequireComponent(typeof(Selectable))]
public class BattleRootView : MonoBehaviour
{
    /// <summary>
    /// ルート要素が選択されているかどうか
    /// </summary>
    public bool IsRootSelected => SafeNavigationManager.GetCurrentSelected() == this.gameObject;
}
