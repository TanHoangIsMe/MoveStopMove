using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int poolSize;
    [SerializeField] private Transform ground;

    private List<GameObject> pool;

    float minX, maxX, minZ, maxZ;

    private void Start()
    {
        pool = new List<GameObject>();

        Vector3 groundSize = ground.localScale * 10;
        minX = ground.position.x - groundSize.x / 2;
        maxX = ground.position.x + groundSize.x / 2;
        minZ = ground.position.z - groundSize.z / 2;
        maxZ = ground.position.z + groundSize.z / 2;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, GetRandomPosition(), Quaternion.identity);
            pool.Add(enemy);
        }
    }

    public void ActiveEnemy()
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy) 
            {
                obj.transform.position = GetRandomPosition();
                obj.SetActive(true);
                return;
            }
        }
    }

    private Vector3 GetRandomPosition()
    {
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);
        Vector3 spawnPosition = new Vector3(randomX, 0f, randomZ);
        return spawnPosition;
    }
}
