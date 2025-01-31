using UnityEngine;

[RequireComponent(typeof(TargetTracker))]
[RequireComponent(typeof(HealthController))]
public class ClownHead : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField]
    private float stoppingDistance = 10f
    [SerializeField] private float repositionDistance = 12f;

    [Header("Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float attackRate = 2f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private int shotsBeforeReposition = 3;

    private HealthController healthController;
    private TargetTracker targetTracker;
    private float nextAttackTime;
    private Vector3 targetPosition;
    private bool isRepositioning;
    private int shotsFired;

    private void Start()
    {
        targetTracker = GetComponent<TargetTracker>();
        SetNewTargetPosition();
        healthController = GetComponent<HealthController>();
        healthController.OnDeath.AddListener(OnDeath);
        shotsFired = 0;
    }

    private void Update()
    {
        if (healthController.IsDead) return;
        if (targetTracker.CurrentTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, targetTracker.CurrentTarget.transform.position);

        // Look at player
        transform.LookAt(targetTracker.CurrentTarget.transform);

        if (isRepositioning)
        {
            // Move to new position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isRepositioning = false;
                shotsFired = 0;
            }
        }
        else if (distanceToPlayer > stoppingDistance)
        {
            // Move towards player
            transform.position += transform.forward * (movementSpeed * Time.deltaTime);
        }
        else
        {
            // Attack when in range
            if (Time.time >= nextAttackTime)
            {
                Attack();
                shotsFired++;
                if (shotsFired >= shotsBeforeReposition)
                {
                    SetNewTargetPosition();
                    isRepositioning = true;
                }
            }
        }
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

    private void SetNewTargetPosition()
    {
        if (targetTracker.CurrentTarget == null) return;

        Vector3 targetPos = targetTracker.CurrentTarget.transform.position;
        Vector3 newTargetPosition;
        int maxAttempts = 10;
        int attempts = 0;

        do
        {
            // Get random point around player
            Vector2 randomCircle = Random.insideUnitCircle.normalized * repositionDistance;
            newTargetPosition = new Vector3(targetPos.x + randomCircle.x, transform.position.y, targetPos.z + randomCircle.y);
            attempts++;
        }
        while (!IsValidPosition(newTargetPosition) && attempts < maxAttempts);

        if (attempts < maxAttempts)
        {
            targetPosition = newTargetPosition;
        }
        else
        {
            // Fallback to current position if no valid position found
            targetPosition = transform.position;
        }
    }

    private bool IsValidPosition(Vector3 position)
    {
        // Check if the position is above ground
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 10, Vector3.down, out hit, 20f))
        {
            return hit.collider != null && hit.point.y >= transform.position.y;
        }
        return false;
    }

    private void OnDeath()
    {
        Destroy(gameObject);
    }

    private void OnValidate()
    {
        // Stoping distance should be less than reposition distance
        stoppingDistance = Mathf.Min(stoppingDistance, repositionDistance);
    }
}
