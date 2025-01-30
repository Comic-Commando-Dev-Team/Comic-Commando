using UnityEngine;

public class TargetTracker : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("The tag of the target object to track")]
    [SerializeField] private string targetTag = "Player";

    [Tooltip("How often to search for the target if it is lost")]
    [SerializeField] private float targetRefreshRate = 1f;

    private GameObject currentTarget;
    private float nextSearchTime;

    public GameObject CurrentTarget => currentTarget;

    private void Start()
    {
        FindTarget();
    }

    private void Update()
    {
        if (currentTarget == null || Time.time >= nextSearchTime)
        {
            FindTarget();
            nextSearchTime = Time.time + targetRefreshRate;
        }
    }

    private void FindTarget()
    {
        GameObject target = GameObject.FindWithTag(targetTag);

        if (target != null)
        {
            currentTarget = target;
        }
    }
}
