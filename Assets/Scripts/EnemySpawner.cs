using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private ObjectPooling objectPooling;

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
            objectPooling.ActiveEnemy();
        }
    }

    public void StopSpawning()
    {
        isEndGame = true;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            objectPooling.DeActiveAll();
        }
    }
}
