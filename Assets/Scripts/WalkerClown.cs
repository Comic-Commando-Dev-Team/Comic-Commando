using UnityEngine;

[RequireComponent(typeof(TargetTracker))]
[RequireComponent(typeof(HealthController))]
public class WalkerClown : MonoBehaviour
{
    [SerializeField] private float speed = 2.0f;
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private int _damage = 10; // Przykładowa wartość obrażeń
    [SerializeField] private GameObject deathImagePrefab; // Prefab obrazka PNG

    private Animator animator;
    private HealthController healthController;
    private TargetTracker targetTracker;

    void Start()
    {
        animator = GetComponent<Animator>();
        healthController = GetComponent<HealthController>();
        targetTracker = GetComponent<TargetTracker>();

        if (animator == null)
        {
            Debug.LogError("Animator not found!");
        }
        else
        {
            // Check if parameters exist in the Animator
            if (!animator.HasParameterOfType("True_Clown_Run", AnimatorControllerParameterType.Bool) ||
                !animator.HasParameterOfType("True_Clown_Idle", AnimatorControllerParameterType.Bool))
            {
                Debug.LogError("Animator parameters 'True_Clown_Run' or 'True_Clown_Idle' not found!");
            }
        }

        if (healthController != null)
        {
            healthController.ChangeHealth(100 - healthController.CurrentHealth); // Set health to 100
        }
    }

    void Update()
    {
        if (healthController.IsDead) return;
        if (targetTracker.CurrentTarget == null) return;

        float distance = Vector3.Distance(transform.position, targetTracker.CurrentTarget.transform.position);

        // Look at player with an additional 90 degrees rotation on Y axis
        Vector3 direction = (targetTracker.CurrentTarget.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        lookRotation *= Quaternion.Euler(0, 90, 0); // Add 90 degrees to the Y axis
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * speed);
        if (distance > stoppingDistance)
        {
            // Move towards the player
            transform.position += direction * speed * Time.deltaTime;

            // Play running animation
            if (!animator.GetBool("True_Clown_Run"))
            {
                animator.SetBool("True_Clown_Run", true);
                animator.SetBool("True_Clown_Idle", false);
            }
        }
        else
        {
            // Stop and play idle animation
            if (!animator.GetBool("True_Clown_Idle"))
            {
                animator.SetBool("True_Clown_Run", false);
                animator.SetBool("True_Clown_Idle", true);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            if (healthController != null)
            {
                healthController.Damage(_damage);
                Debug.Log("Clown hit by bullet!");
                Debug.Log($"Clown health: {healthController.CurrentHealth}");

                // Check if health is zero and destroy the game object
                if (healthController.CurrentHealth <= 0)
                {
                    SpawnDeathImage();
                    Destroy(gameObject);
                }
            }
        }
    }

    private void SpawnDeathImage()
    {
        if (deathImagePrefab != null && targetTracker.CurrentTarget != null)
        {
            Vector3 spawnPosition = transform.position;
            spawnPosition.y += 1; // Increase Y position by 1 unit

            GameObject deathImage = Instantiate(deathImagePrefab, spawnPosition, Quaternion.identity);
            StartCoroutine(DestroyAfterTime(deathImage, 2f));

            // Rotate the image to face the player and adjust rotation on X axis
            Vector3 direction = (targetTracker.CurrentTarget.transform.position - deathImage.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            lookRotation *= Quaternion.Euler(90, 0, 0); // Rotate 90 degrees on X axis
            deathImage.transform.rotation = lookRotation;
        }
    }

    private System.Collections.IEnumerator DestroyAfterTime(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }
}

public static class AnimatorExtensions
{
    public static bool HasParameterOfType(this Animator animator, string name, AnimatorControllerParameterType type)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.type == type && param.name == name)
            {
                return true;
            }
        }
        return false;
    }
}