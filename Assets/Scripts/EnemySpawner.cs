using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public enum SpawnType { Point, Area }

    [Header("Spawner Settings")]
    [SerializeField] private SpawnType spawnType = SpawnType.Point;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField, Min(1)] private float spawnInterval = 5f;
    [SerializeField] private float spawnIntervalRandomness = 1f;
    [SerializeField, Min(1)] private int maxEnemyCount = 10;

    [Header("Area Settings")]
    [SerializeField, Min(1)] private float spawnRadius = 5f;

    private float nextSpawnTime;
    private int currentEnemyCount;

    private void Start()
    {
        nextSpawnTime = Time.time + GetRandomSpawnInterval();
    }

    private void Update()
    {
        if (currentEnemyCount >= maxEnemyCount) return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + GetRandomSpawnInterval();
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab != null)
        {
            Vector3 spawnPosition = transform.position;

            if (spawnType == SpawnType.Area)
            {
                spawnPosition += Random.insideUnitSphere * spawnRadius;
                spawnPosition.y = transform.position.y; // Keep the same height as the spawner
            }

            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            currentEnemyCount++;

            if (enemy.TryGetComponent<HealthController>(out HealthController healthController))
            {
                healthController.OnDeath.AddListener(OnEnemyDestroyed);
            }
        }
    }

    private float GetRandomSpawnInterval()
    {
        return spawnInterval + Random.Range(-spawnIntervalRandomness, spawnIntervalRandomness);
    }

    private void OnEnemyDestroyed()
    {
        currentEnemyCount--;
    }

    private void OnValidate()
    {
        if (spawnType == SpawnType.Area)
        {
            spawnRadius = Mathf.Max(1, spawnRadius);
        }
        else
        {
            spawnRadius = 0;
        }

        spawnInterval = Mathf.Max(0.1f, spawnInterval);
        spawnIntervalRandomness = Mathf.Max(0, spawnIntervalRandomness);
        maxEnemyCount = Mathf.Max(1, maxEnemyCount);

        if (enemyPrefab != null && !enemyPrefab.TryGetComponent<HealthController>(out HealthController healthController))
        {
            Debug.LogWarning("Enemy prefab does not have a HealthController component. Enemy limit will not work.");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (spawnType == SpawnType.Point)
        {
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
        else if (spawnType == SpawnType.Area)
        {
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
#endif
}
