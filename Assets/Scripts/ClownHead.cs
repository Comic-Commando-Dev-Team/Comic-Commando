using UnityEngine;

[RequireComponent(typeof(TargetTracker))]
[RequireComponent(typeof(HealthController))]
public class ClownHead : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float baseMovementSpeed = 2f;
    [SerializeField] private float stoppingDistance = 10f;
    [SerializeField] private float baseOrbitSpeed = 2f;
    [SerializeField] private float speedVariationRange = 0.5f;

    [Header("Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float attackRate = 2f;
    [SerializeField] private float projectileSpeed = 10f;

    [Header("Death")]
    [SerializeField] private int points = 100;

    private HealthController healthController;
    private TargetTracker targetTracker;
    private ScoreBoard scoreBoard;

    private float nextAttackTime;
    private float currentMovementSpeed;
    private float currentOrbitSpeed;
    private int orbitDirection;

    private const float MIN_STOPPING_DISTANCE = 0f;

    private void Awake()
    {
        InitializeComponents();
        InitializeMovementParameters();
    }

    private void InitializeComponents()
    {
        targetTracker = GetComponent<TargetTracker>();
        healthController = GetComponent<HealthController>();
        scoreBoard = FindAnyObjectByType<ScoreBoard>();

        if (healthController != null)
        {
            healthController.OnDeath.AddListener(OnDeath);
        }
    }

    private void InitializeMovementParameters()
    {
        // Randomize initial speeds within variation range
        currentMovementSpeed = baseMovementSpeed + Random.Range(-speedVariationRange, speedVariationRange);
        currentOrbitSpeed = baseOrbitSpeed + Random.Range(-speedVariationRange, speedVariationRange);

        // Randomly choose orbit direction
        orbitDirection = Random.value > 0.5f ? 1 : -1;
    }

    private void Update()
    {
        if (ShouldSkipUpdate()) return;

        HandleMovement();
    }

    private bool ShouldSkipUpdate()
    {
        return healthController.IsDead || targetTracker.CurrentTarget == null;
    }

    private void HandleMovement()
    {
        Vector3 targetPosition = targetTracker.CurrentTarget.transform.position;
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        transform.LookAt(targetPosition);

        if (distanceToTarget > stoppingDistance)
        {
            MoveTowardsTarget();
        }
        else
        {
            OrbitAroundTarget();
            AttemptAttack();
        }
    }

    private void MoveTowardsTarget()
    {
        transform.position += transform.forward * (currentMovementSpeed * Time.deltaTime);
    }

    private void OrbitAroundTarget()
    {
        if (targetTracker.CurrentTarget == null) return;

        Vector3 targetPosition = targetTracker.CurrentTarget.transform.position;
        Vector3 directionToTarget = (transform.position - targetPosition).normalized;

        float orbitAngle = currentOrbitSpeed * orbitDirection * Time.deltaTime;
        Vector3 orbitPosition = Quaternion.Euler(0, orbitAngle, 0) * directionToTarget * stoppingDistance;

        transform.position = targetPosition + orbitPosition;
    }

    private void AttemptAttack()
    {
        if (Time.time >= nextAttackTime)
        {
            Attack();
        }
    }

    private void Attack()
    {
        if (!IsValidForAttack()) return;

        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, transform.rotation);
        if (projectile.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = transform.forward * projectileSpeed;
        }

        nextAttackTime = Time.time + attackRate;
    }

    private bool IsValidForAttack()
    {
        return shootPoint != null && projectilePrefab != null;
    }

    private void ShowScorePoints()
    {
        // Show floating score text at the enemy's position
        Vector3 scorePosition = transform.position;
        scorePosition.y += 1.5f; // Slightly above the enemy

        // Use the ScoreTextManager for better performance and visuals
        ScoreTextManager.Instance.ShowScore(points, scorePosition);
    }

    private void OnDeath()
    {
        if (scoreBoard != null)
        {
            scoreBoard.AddScore(points);
        }
        ShowScorePoints();
        Destroy(gameObject);
    }

    private void OnValidate()
    {
        stoppingDistance = Mathf.Max(stoppingDistance, MIN_STOPPING_DISTANCE);
        speedVariationRange = Mathf.Max(speedVariationRange, 0f);
    }
}
