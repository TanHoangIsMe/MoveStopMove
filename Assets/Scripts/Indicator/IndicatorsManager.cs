using System.Collections.Generic;
using UnityEngine;

public class IndicatorsManager : MonoBehaviour
{
    public static IndicatorsManager Instance { get; private set; }

    public Camera mainCamera;
    public RectTransform indicatorContainer; 
    public float edgeOffsetPercentX = 0.1f;
    public float edgeOffsetPercentY = 0.05f;
    public float angleSmoothTime = 0.1f;

    private Dictionary<Transform, IndicatorData> enemyIndicators = new Dictionary<Transform, IndicatorData>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private class IndicatorData
    {
        public RectTransform indicatorUI;
        public RectTransform pointTextUI;
        public float currentAngle;
        public float angleVelocity;
    }

    public void RegisterEnemy(Transform enemy)
    {
        if (enemyIndicators.ContainsKey(enemy))
            return;

        RectTransform indicator = IndicatorPool.Instance.GetIndicator();
        indicator.SetParent(indicatorContainer, false);
        
        RectTransform pointTextUI = indicator.Find("Point Text") as RectTransform;

        IndicatorData data = new IndicatorData
        {
            indicatorUI = indicator,
            pointTextUI = pointTextUI,
            currentAngle = 0f,
            angleVelocity = 0f
        };
        enemyIndicators.Add(enemy, data);
    }

    public void UnregisterEnemy(Transform enemy)
    {
        if (enemyIndicators.TryGetValue(enemy, out IndicatorData data))
        {
            IndicatorPool.Instance.ReturnIndicator(data.indicatorUI);
            enemyIndicators.Remove(enemy);
        }
    }

    private void Update()
    {
        List<Transform> keys = new List<Transform>(enemyIndicators.Keys);
        foreach (Transform enemy in keys)
        {
            if (!enemy.gameObject.activeInHierarchy)
            {
                UnregisterEnemy(enemy);
                continue;
            }
            UpdateIndicator(enemy, enemyIndicators[enemy]);
        }
    }

    private void UpdateIndicator(Transform enemy, IndicatorData data)
    {
        Bounds enemyBounds = GetEnemyBounds(enemy);
        bool isOut = IsEnemyOutOfView(enemyBounds);
        data.indicatorUI.gameObject.SetActive(isOut);
        if (!isOut)
            return;

        Vector3 screenPos = mainCamera.WorldToScreenPoint(enemy.position);
        if (screenPos.z < 0)
            screenPos *= -1;

        Vector3 direction = (enemy.position - mainCamera.transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        data.currentAngle = Mathf.SmoothDampAngle(data.currentAngle, -targetAngle, ref data.angleVelocity, angleSmoothTime);

        data.indicatorUI.rotation = Quaternion.Euler(0, 0, data.currentAngle);
        data.pointTextUI.rotation = Quaternion.identity;

        float edgeOffsetX = Screen.width * edgeOffsetPercentX;
        float edgeOffsetY = Screen.height * edgeOffsetPercentY;
        screenPos.x = Mathf.Clamp(screenPos.x, edgeOffsetX, Screen.width - edgeOffsetX);
        screenPos.y = Mathf.Clamp(screenPos.y, edgeOffsetY, Screen.height - edgeOffsetY);
        data.indicatorUI.position = screenPos;
    }

    private bool IsEnemyOutOfView(Bounds bounds)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        return !GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    private Bounds GetEnemyBounds(Transform enemy)
    {
        Collider collider = enemy.GetComponent<Collider>();
        if (collider != null)
            return collider.bounds;
        return new Bounds(enemy.position, Vector3.one * 0.5f);
    }
}
