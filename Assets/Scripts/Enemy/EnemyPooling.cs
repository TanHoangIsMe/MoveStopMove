using System.Collections.Generic;
using UnityEngine;

public class EnemyPooling : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int poolSize;
    [SerializeField] private Transform ground;
    [SerializeField] private Transform player;
    [SerializeField] private Transform container;

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
            enemy.transform.SetParent(container);
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

    public void DeActiveAll()
    {
        foreach (GameObject obj in pool)
            if(obj.activeSelf)
                obj.GetComponent<EnemyController>().HandleEndGame();
    }

    private Vector3 GetRandomPosition()
    {
        float safeDistance = 5f; 
        Vector3 playerPosition = player.position;
        Vector3 spawnPosition;

        while (true)
        {
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            spawnPosition = new Vector3(randomX, 0f, randomZ);

            if (Vector3.Distance(spawnPosition, playerPosition) >= safeDistance)
                return spawnPosition;
        }
    }
}
