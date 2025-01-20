using UnityEngine;

public class EnemyShootProjectiles : MonoBehaviour
{
    [SerializeField] private GameObject Projectile;
    [SerializeField] private float FiringRate = 1.0f;
    [SerializeField] private bool IsFiring = true;
    void Start()
    {
        if (IsFiring)
        {
            InvokeRepeating("SpawnProjectile", 3, FiringRate);
        }
        
    }
    void SpawnProjectile()
    {
        GameObject.Instantiate(Projectile, transform.position, transform.rotation);
    }
}
