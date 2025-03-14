using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyPooling enemyPooling;

    public bool isEndGame = false;
    private Coroutine spawnCoroutine;

    private void Start()
    {
        spawnCoroutine = StartCoroutine(ActiveEnemy());        
    }

    private IEnumerator ActiveEnemy()
    {
        while (!isEndGame)
        {
            yield return new WaitForSeconds(3f);
            enemyPooling.ActiveEnemy();
        }
    }

    public void StopSpawning()
    {
        isEndGame = true;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            enemyPooling.DeActiveAll();
        }
    }
}
