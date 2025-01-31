using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TargetTracker))]
[RequireComponent(typeof(HealthController))]
public class ClownHead : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float stoppingDistance = 10f;
    [SerializeField] private float orbitSpeed = 2f;

    [Header("Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float attackRate = 2f;
    [SerializeField] private float projectileSpeed = 10f;

    [Header("Death")]
    [SerializeField] private GameObject deathImagePrefab; // Prefab obrazka PNG
    [SerializeField] private int points = 100; // Punkty za zniszczenie przeciwnika

    private HealthController healthController;
    private TargetTracker targetTracker;
    private float nextAttackTime;
    private bool isOrbiting;
    private ScoreBoard scoreBoard;

    private void Start()
    {
        targetTracker = GetComponent<TargetTracker>();
        healthController = GetComponent<HealthController>();
        healthController.OnDeath.AddListener(OnDeath);
        scoreBoard = FindAnyObjectByType<ScoreBoard>();
    }

    private void Update()
    {
        if (healthController.IsDead) return;
        if (targetTracker.CurrentTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, targetTracker.CurrentTarget.transform.position);

        // Look at player
        transform.LookAt(targetTracker.CurrentTarget.transform);

        if (distanceToPlayer > stoppingDistance)
        {
            // Move towards player
            isOrbiting = false;
            transform.position += transform.forward * (movementSpeed * Time.deltaTime);
        }
        else
        {
            // Orbit around player and attack
            isOrbiting = true;
            OrbitAroundPlayer();
            if (Time.time >= nextAttackTime)
            {
                Attack();
            }
        }
    }

    private void OrbitAroundPlayer()
    {
        if (targetTracker.CurrentTarget == null) return;

        // Calculate orbit position
        Vector3 direction = (transform.position - targetTracker.CurrentTarget.transform.position).normalized;
        Vector3 orbitPosition = Quaternion.Euler(0, orbitSpeed * Time.deltaTime, 0) * direction * stoppingDistance;
        transform.position = targetTracker.CurrentTarget.transform.position + orbitPosition;
    }

    private void Attack()
    {
        if (shootPoint != null && projectilePrefab != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, transform.rotation);
            if (projectile.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.linearVelocity = transform.forward * projectileSpeed;
            }
        }
        nextAttackTime = Time.time + attackRate;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            if (healthController != null)
            {
                healthController.Damage(10); // Przykładowa wartość obrażeń
                Debug.Log("Clown hit by bullet!");
                Debug.Log($"Clown health: {healthController.CurrentHealth}");

                // Check if health is zero and destroy the game object
                if (healthController.CurrentHealth <= 0)
                {
                    SpawnDeathImage();
                    if (scoreBoard != null)
                    {
                        scoreBoard.AddScore(points);
                    }
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
            lookRotation *= Quaternion.Euler(0, 90, 0); // Rotate 90 degrees on Y axis
            deathImage.transform.rotation = lookRotation;
        }
    }

    private IEnumerator DestroyAfterTime(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }

    private void OnDeath()
    {
        Destroy(gameObject);
    }

    private void OnValidate()
    {
        // Ensure stopping distance is positive
        stoppingDistance = Mathf.Max(stoppingDistance, 0);
    }
}
