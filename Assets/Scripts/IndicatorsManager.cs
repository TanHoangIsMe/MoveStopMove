using UnityEngine;

public class IndicatorsManager : MonoBehaviour
{
    public Transform enemy; 
    public RectTransform indicatorUI; 
    public Camera mainCamera;

    public float edgeOffsetPercentX = 0.1f; 
    public float edgeOffsetPercentY = 0.05f;

    private void Update()
    {
        Bounds enemyBounds = GetEnemyBounds(enemy);

        if (IsEnemyOutOfView(enemyBounds))
        {
            indicatorUI.gameObject.SetActive(true);

            Vector3 screenPos = mainCamera.WorldToScreenPoint(enemy.position);

            if (screenPos.z < 0)
            {
                screenPos *= -1;
            }

            Vector3 direction = (enemy.position - mainCamera.transform.position).normalized;
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            indicatorUI.rotation = Quaternion.Euler(0, 0, -angle);

            float edgeOffsetX = Screen.width * edgeOffsetPercentX;
            float edgeOffsetY = Screen.height * edgeOffsetPercentY;
            screenPos.x = Mathf.Clamp(screenPos.x, edgeOffsetX, Screen.width - edgeOffsetX);
            screenPos.y = Mathf.Clamp(screenPos.y, edgeOffsetY, Screen.height - edgeOffsetY);

            indicatorUI.position = screenPos;
        }
        else
        {
            indicatorUI.gameObject.SetActive(false);
        }
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
        {
            return collider.bounds;
        }
        return new Bounds(enemy.position, Vector3.one * 0.5f); 
    }
}
