using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private string[] targetTags = { "Enemy" };

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hitObject = collision.gameObject;

        // Check if we hit a valid target
        bool isValidTarget = false;
        foreach (string tag in targetTags)
        {
            if (hitObject.CompareTag(tag))
            {
                isValidTarget = true;
                break;
            }
        }

        if (isValidTarget)
        {
            if (hitObject.TryGetComponent<HealthController>(out HealthController health))
            {
                health.Damage(damage);
            }
        }

        // Destroy the bullet on any collision
        Destroy(gameObject);
    }
}