using UnityEngine;

public class OnObstacleHandler : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private EnemyController enemyController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            if (playerController != null) 
                playerController.isColliding = true;
            else Debug.Log("");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            if (playerController != null) playerController.isColliding = false;
            else Debug.Log("");
        }
    }
}
