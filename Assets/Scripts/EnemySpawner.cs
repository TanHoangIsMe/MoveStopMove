using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private ObjectPooling objectPooling;

    private void Start()
    {
        StartCoroutine(ActiveEnemy());        
    }

    private IEnumerator ActiveEnemy()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            objectPooling.ActiveEnemy();
        }
    }
}
