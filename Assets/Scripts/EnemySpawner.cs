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
    private void Update()
    {
        if (EnemyToSpawn != null)
        {
            if (Spawning) InvokeRepeating("SpawnEnemy", 3, SpawnDelayInSeconds);
        }

    }
    void SpawnEnemy()
    {
        int EnemyCount = GameObject.FindGameObjectsWithTag(EnemyToSpawn.tag).Length;
        if (EnemyCount <= MaxEnemyType)
        {
            GameObject.Instantiate(EnemyToSpawn, transform.position, transform.rotation);
        }
    }

}
