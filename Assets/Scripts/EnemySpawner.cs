using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField, Range(1, 100)] private int enemySpawnDelayInSeconds = 5;
    [SerializeField, Range(1, 100)] private int maxEnemyCount = 10;

    private void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab is not assigned to the spawner!");
            return;
        }
        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy()
    {
        while (true)
        {
            yield return new WaitForSeconds(enemySpawnDelayInSeconds);

            try
            {
                if (enemyPrefab == null)
                {
                    Debug.LogError("Enemy Prefab is null!");
                    yield break;
                }

                int currentEnemyCount = GameObject.FindGameObjectsWithTag(enemyPrefab.tag).Length;
                Debug.Log($"Current enemy count: {currentEnemyCount}, Max allowed: {maxEnemyCount}");

                if (currentEnemyCount < maxEnemyCount)
                {
                    Instantiate(enemyPrefab, transform.position, transform.rotation);
                }
                else
                {
                    Debug.Log("Max enemy count reached, not spawning more enemies.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Exception in SpawnEnemy: {ex.Message}");
                yield break;
            }
        }
    }
}