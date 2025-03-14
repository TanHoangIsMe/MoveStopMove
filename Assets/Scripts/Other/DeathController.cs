using UnityEngine;

public class DeathController : MonoBehaviour
{
    public static event System.Action<EnemyController> OnEnemyDeath;

    public static void NotifyEnemyDeath(EnemyController enemy)
    {
        OnEnemyDeath?.Invoke(enemy);
    }
}
