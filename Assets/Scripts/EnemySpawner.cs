using System.Collections;
using System.ComponentModel;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("General settings")]
    [SerializeField] private GameObject EnemyToSpawn;
    [SerializeField, Range(1,100)] private int SpawnDelayInSeconds = 5;
    [SerializeField, Min(1)] private bool Spawning = true;
    [Header("Multiple enemies settings")] 
    [SerializeField, Min(1), Range(1,100)] int MaxEnemyType = 3;

    private IEnumerator SpawnEnemies()
    {
        while (Spawning) {
            int EnemyCount = GameObject.FindGameObjectsWithTag(EnemyToSpawn.tag).Length;
            if (EnemyCount <= MaxEnemyType) {
                Instantiate(EnemyToSpawn, transform.position, transform.rotation);
            }
            yield return new WaitForSeconds(SpawnDelayInSeconds);
        }
        
    }
    private void Start()
    {
        if (EnemyToSpawn != null && Spawning)
        {
            StartCoroutine (SpawnEnemies());
        }

    }

}
