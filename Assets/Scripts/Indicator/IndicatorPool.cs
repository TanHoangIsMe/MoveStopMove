using System.Collections.Generic;
using UnityEngine;

public class IndicatorPool : MonoBehaviour
{
    public static IndicatorPool Instance;
    public RectTransform indicatorPrefab;
    public RectTransform poolContainer;

    private Queue<RectTransform> pool = new Queue<RectTransform>();

    private void Awake()
    {
        Instance = this;
    }

    public RectTransform GetIndicator()
    {
        if (pool.Count > 0)
        {
            RectTransform indicator = pool.Dequeue();
            indicator.gameObject.SetActive(true);
            return indicator;
        }
        else
        {
            RectTransform newIndicator = Instantiate(indicatorPrefab, transform);
            return newIndicator;
        }
    }

    public void ReturnIndicator(RectTransform indicator)
    {
        indicator.gameObject.SetActive(false);
        indicator.SetParent(poolContainer);
        pool.Enqueue(indicator);
    }
}
