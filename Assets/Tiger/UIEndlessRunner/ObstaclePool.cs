using UnityEngine;
using System.Collections.Generic;

public class ObstaclePool : MonoBehaviour
{
    public RectTransform obstaclePrefab;
    public int initialSize = 12;
    readonly Queue<RectTransform> pool = new();

    void Start()
    {
        for (int i = 0; i < initialSize; i++)
        {
            var inst = Instantiate(obstaclePrefab, transform);
            inst.gameObject.SetActive(false);
            pool.Enqueue(inst);
        }
    }

    public RectTransform Get(Transform parent)
    {
        RectTransform item = pool.Count == 0 ? Instantiate(obstaclePrefab) : pool.Dequeue();
        item.SetParent(parent, false);
        item.gameObject.SetActive(true);
        return item;
    }

    public void Return(RectTransform item)
    {
        item.gameObject.SetActive(false);
        item.SetParent(transform, false);
        pool.Enqueue(item);
    }
}
