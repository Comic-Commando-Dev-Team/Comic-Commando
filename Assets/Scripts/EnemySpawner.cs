using System.Collections;
using System.ComponentModel;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField, Range(1, 100)]
    [Description("The maximum number of enemies that can be spawned at once. Uses tags to count enemies.")]
    private int maxEnemyCount = 3;
    [SerializeField, Range(1, 100)] private int enemySpawnDelayInSeconds = 5;


    private void Start()
    {
        StartCoroutine(SpawnEnemy());
    }

    private IEnumerator SpawnEnemy()
    {
        while (true)
        {
            yield return new WaitForSeconds(enemySpawnDelayInSeconds);

            if (GameObject.FindGameObjectsWithTag(enemyPrefab.tag).Length < maxEnemyCount)
            {
                Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1);
    }
}
