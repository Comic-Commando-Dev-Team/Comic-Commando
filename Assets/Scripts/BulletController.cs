using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private string[] targetTags = { "Enemy" };
    [SerializeField] private float lifeTime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hitObject = collision.gameObject;

        Debug.Log($"Hit object: {hitObject.name} with tag: {hitObject.tag}");

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

        Debug.Log($"Is valid target: {isValidTarget}");

        if (isValidTarget)
        {
            if (hitObject.TryGetComponent<HealthController>(out HealthController health))
            {
                health.Damage(damage);
            }
        }
    }
}
