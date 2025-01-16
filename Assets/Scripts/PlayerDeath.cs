using Oculus.Interaction;
using System.Linq;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    private readonly string[] enemyTags = { "EnemyWalker", "EnemyHead", "EnemyProjectile" };
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided!");
        if (enemyTags.Contains(collision.gameObject.tag))
        {
            string EnemyName = collision.gameObject.name;
            Debug.Log("Player died to: " + EnemyName);
            Time.timeScale = 0;
            
        }
    }
}
