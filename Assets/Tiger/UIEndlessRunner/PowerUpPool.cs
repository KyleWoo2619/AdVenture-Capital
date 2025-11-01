using UnityEngine;
using System.Collections.Generic;

public class PowerUpPool : MonoBehaviour
{
    public RectTransform powerUpPrefab;
    public int initialSize = 6;
    readonly Queue<RectTransform> pool = new();

    void Start()
    {
        for (int i = 0; i < initialSize; i++)
        {
            var inst = Instantiate(powerUpPrefab, transform);
            inst.gameObject.SetActive(false);
            pool.Enqueue(inst);
        }
    }

    public RectTransform Get(Transform parent)
    {
        RectTransform item = pool.Count == 0 ? Instantiate(powerUpPrefab) : pool.Dequeue();
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
