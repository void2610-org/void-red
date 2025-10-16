using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer.Unity;

public class MouseHoverUISelector : ITickable
{
    private const string IGNORE_TAG = "IgnoreHoverSelection";
    
    public void Tick()
    {
        var eventSystem = EventSystem.current;
        if (!eventSystem) return;

        var pointerData = new PointerEventData(eventSystem) { position = Input.mousePosition };

        var results = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            var hoveredObject = result.gameObject;

            // 特定のタグを持つオブジェクトは無視
            if (hoveredObject.CompareTag(IGNORE_TAG))
                break;

            // 最初に適切なUIを見つけたら、それを選択
            if (eventSystem.currentSelectedGameObject != hoveredObject)
                SafeNavigationManager.SetSelectedGameObjectSafe(hoveredObject);
            
            break; // 最初の適切なUIだけを選択する
        }
    }
}