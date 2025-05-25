using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.AI;

[RequireComponent(typeof(TargetTracker))]
[RequireComponent(typeof(HealthController))]
[RequireComponent(typeof(NavMeshAgent))]
public class WalkerClown : MonoBehaviour
{
    // Animation parameter names
    private const string ANIM_RUN = "True_Clown_Run";
    private const string ANIM_IDLE = "True_Clown_Idle";
    private const string ANIM_CROUCH_WALK = "True_Clown_Crouch_Walk";
    private const string ANIM_GRAB = "True_Clown_Grab";
    private const string ANIM_STUNNED = "True_Clown_Stunned";
    private const string ANIM_LAUGH = "True_Clown_Laugh";
    private const string ANIM_DIE = "True_Clown_Die";
    private const string ANIM_START = "StartAnim";

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float stoppingDistance = 2f;

    [Header("Combat")]
    [SerializeField] private int damage = 10;
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private float attackRadius = 2.5f;
    [SerializeField] private float attackInterval = 2.0f;

    [Header("References")]
    [SerializeField] private GameObject deathImagePrefab;
    [SerializeField] private AudioClip attackSound;

    [Header("Animation Lengths (Fallback)")]
    [SerializeField] private float attackAnimLengthFallback = 1.2f;
    [SerializeField] private float dieAnimLengthFallback = 1.0f;

    private Animator animator;
    private HealthController healthController;
    private TargetTracker targetTracker;
    private AudioSource audioSource;
    private ScoreBoard scoreBoard;
    private NavMeshAgent agent;

    private float lastAttackSoundTime = -3f;
    private bool isStunned = false;
    private float stunnedTimer = 0f;
    private float stunnedDuration = 1.0f;
    private float lastAttackTimestamp = -Mathf.Infinity;
    private bool isAttackInProgress = false;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator not found!");
            enabled = false;
            return;
        }
        healthController = GetComponent<HealthController>();
        if (healthController == null)
        {
            Debug.LogError("HealthController not found!");
            enabled = false;
            return;
        }
        targetTracker = GetComponent<TargetTracker>();
        if (targetTracker == null)
        {
            Debug.LogError("TargetTracker not found!");
            enabled = false;
            return;
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource not found!");
            enabled = false;
            return;
        }
        scoreBoard = FindAnyObjectByType<ScoreBoard>();
        if (scoreBoard == null)
        {
            Debug.LogWarning("ScoreBoard not found in scene.");
        }
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent not found!");
            enabled = false;
            return;
        }
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.updateRotation = true;
        agent.updatePosition = true;

        int randomAnim = Random.value > 0.5f ? 1 : 2;
        animator.SetInteger(ANIM_START, randomAnim);

        if (!animator.HasParameterOfType(ANIM_RUN, AnimatorControllerParameterType.Bool) ||
            !animator.HasParameterOfType(ANIM_IDLE, AnimatorControllerParameterType.Bool))
        {
            Debug.LogError($"Animator parameters '{ANIM_RUN}' or '{ANIM_IDLE}' not found!");
            enabled = false;
            return;
        }

        healthController.OnDeath.AddListener(OnDeath);
    }

    private void Update()
    {
        if (healthController.IsDead) return;
        if (targetTracker.CurrentTarget == null) return;

        if (isStunned)
        {
            stunnedTimer -= Time.deltaTime;
            if (stunnedTimer <= 0f)
            {
                isStunned = false;
                agent.isStopped = false;
            }
            else
            {
                agent.isStopped = true;
            }
            return;
        }

        agent.isStopped = false;
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(targetTracker.CurrentTarget.transform.position);

        float distanceToTarget = Vector3.Distance(transform.position, targetTracker.CurrentTarget.transform.position);

        if (distanceToTarget <= attackRadius && !isAttackInProgress && Time.time - lastAttackTimestamp >= attackInterval)
        {
            isAttackInProgress = true;
            agent.isStopped = true;
            StartCoroutine(AttackCoroutine());
        }
        else if (agent.remainingDistance > agent.stoppingDistance)
        {
            if (agent.velocity.magnitude > 0.1f)
            {
                if (!animator.GetBool(ANIM_RUN))
                {
                    animator.SetBool(ANIM_RUN, true);
                    animator.SetBool(ANIM_IDLE, false);
                }
            }
        }
        else
        {
            if (!animator.GetBool(ANIM_IDLE))
            {
                animator.SetBool(ANIM_RUN, false);
                animator.SetBool(ANIM_IDLE, true);
            }
        }
    }

    private IEnumerator AttackCoroutine()
    {
        PlayGrab();
        float attackAnimLength = GetAnimationClipLength(ANIM_GRAB, attackAnimLengthFallback);
        // Wait for the moment the attack should hit (e.g., halfway through the animation)
        float attackHitMoment = attackAnimLength * 0.5f;
        yield return new WaitForSeconds(attackHitMoment);

        // Check if target is still in range
        if (targetTracker.CurrentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetTracker.CurrentTarget.transform.position);
            if (distanceToTarget <= attackRadius)
            {
                if (targetTracker.CurrentTarget.TryGetComponent<HealthController>(out var targetHealth))
                {
                    targetHealth.Damage(damage);
                    if (audioSource != null && attackSound != null && Time.time - lastAttackSoundTime >= 3f)
                    {
                        audioSource.PlayOneShot(attackSound);
                        lastAttackSoundTime = Time.time;
                    }
                }
            }
        }

        // Wait for the rest of the animation
        yield return new WaitForSeconds(attackAnimLength - attackHitMoment);
        lastAttackTimestamp = Time.time;
        isAttackInProgress = false;
        agent.isStopped = false;
    }

    private void SpawnDeathImage()
    {
        if (deathImagePrefab != null && targetTracker.CurrentTarget != null)
        {
            Vector3 spawnPosition = transform.position;
            spawnPosition.y += 1;
            GameObject deathImage = Instantiate(deathImagePrefab, spawnPosition, Quaternion.identity);
            Destroy(deathImage, 2f);
            Vector3 direction = (targetTracker.CurrentTarget.transform.position - deathImage.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            deathImage.transform.rotation = lookRotation;
        }
    }

    private void OnDeath()
    {
        if (scoreBoard != null)
        {
            scoreBoard.AddScore(scoreValue);
        }
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        SpawnDeathImage();
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        if (audioSource != null)
            audioSource.enabled = false;
        moveSpeed = 0f;
        PlayDie();
        float dieAnimLength = GetAnimationClipLength(ANIM_DIE, dieAnimLengthFallback);
        yield return new WaitForSeconds(dieAnimLength + 1f);
        Destroy(gameObject);
    }

    public void Stun()
    {
        isStunned = true;
        stunnedTimer = stunnedDuration;
        PlayStunned();
        if (agent != null)
        {
            agent.isStopped = true;
        }
    }

    // Animation helpers
    public void PlayIdle() => SetAnimBools(ANIM_IDLE, ANIM_RUN);
    public void PlayRun() => SetAnimBools(ANIM_RUN, ANIM_IDLE);
    public void PlayCrouchWalk() => animator.SetTrigger(ANIM_CROUCH_WALK);
    public void PlayGrab() => animator.SetTrigger(ANIM_GRAB);
    public void PlayStunned() => animator.SetTrigger(ANIM_STUNNED);
    public void PlayLaugh() => animator.SetTrigger(ANIM_LAUGH);
    public void PlayDie() => animator.SetTrigger(ANIM_DIE);

    private void SetAnimBools(string trueParam, string falseParam)
    {
        animator.SetBool(trueParam, true);
        animator.SetBool(falseParam, false);
    }

    private float GetAnimationClipLength(string clipName, float fallback)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return fallback;
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        return fallback;
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
