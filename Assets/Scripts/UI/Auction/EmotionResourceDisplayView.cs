using System;
using System.Collections.Generic;
using UnityEngine;

// 8種類の感情リソースを表示するView
// 各感情の色付きインジケーターと残量を横並びで表示
public class EmotionResourceDisplayView : MonoBehaviour
{
    [SerializeField] private EmotionResourceItemView itemPrefab;
    [SerializeField] private Transform container;

    private readonly Dictionary<EmotionType, EmotionResourceItemView> _items = new();

    public void Initialize()
    {
        // 既存のアイテムをクリア
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        _items.Clear();

        // 8種類の感情タイプ分のアイテムを生成
        foreach (EmotionType emotion in Enum.GetValues(typeof(EmotionType)))
        {
            var item = Instantiate(itemPrefab, container);
            item.Initialize(emotion);
            _items[emotion] = item;
        }
    }

    public void UpdateResources(IReadOnlyDictionary<EmotionType, int> resources)
    {
        foreach (var (emotion, amount) in resources)
        {
            if (_items.TryGetValue(emotion, out var item))
            {
                item.UpdateAmount(amount);
            }
        }
    }

    public void UpdateResource(EmotionType emotion, int amount)
    {
        if (_items.TryGetValue(emotion, out var item))
        {
            item.UpdateAmount(amount);
        }
    }

    public void SetSelectedEmotion(EmotionType emotion)
    {
        foreach (var (type, item) in _items)
        {
            item.SetSelected(type == emotion);
        }
    }
}
