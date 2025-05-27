using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.AI;

[RequireComponent(typeof(TargetTracker))]
[RequireComponent(typeof(HealthController))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
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
    [SerializeField] private float stunDuration = 1.0f;

    [Header("References")]
    [SerializeField] private GameObject deathImagePrefab;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip laughSound;

    [Header("Animation Lengths (Fallback)")]
    [SerializeField] private float attackAnimLengthFallback = 1.2f;
    [SerializeField] private float dieAnimLengthFallback = 1.0f;

    private Animator animator;
    private HealthController healthController;
    private TargetTracker targetTracker;
    private AudioSource audioSource;
    private ScoreBoard scoreBoard;
    private NavMeshAgent agent;

    private float stunnedTimer = 0f;
    private float lastAttackTimestamp = -Mathf.Infinity;

    private enum ClownState { Idle, Run, Attack, Stunned, Die }
    private ClownState currentState = ClownState.Idle;

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
        healthController.OnDamage.AddListener(OnDamage);
        ChangeState(ClownState.Idle);
    }

    private void Update()
    {
        if (healthController.IsDead) return;
        if (targetTracker.CurrentTarget == null) return;

        switch (currentState)
        {
            case ClownState.Stunned:
                stunnedTimer -= Time.deltaTime;
                if (stunnedTimer <= 0f)
                {
                    ChangeState(ClownState.Idle);
                }
                break;
            case ClownState.Attack:
                // Attack logic handled by coroutine, wait for coroutine to finish
                break;
            case ClownState.Run:
                agent.SetDestination(targetTracker.CurrentTarget.transform.position);
                float distanceToTarget = Vector3.Distance(transform.position, targetTracker.CurrentTarget.transform.position);
                if (distanceToTarget <= attackRadius && Time.time - lastAttackTimestamp >= attackInterval)
                {
                    ChangeState(ClownState.Attack);
                }
                else if (agent.remainingDistance > agent.stoppingDistance)
                {
                    if (agent.velocity.magnitude > 0.1f)
                    {
                        if (!animator.GetBool(ANIM_RUN))
                        {
                            PlayRun();
                        }
                    }
                }
                else
                {
                    ChangeState(ClownState.Idle);
                }
                break;
            case ClownState.Idle:
                if (!animator.GetBool(ANIM_IDLE))
                {
                    PlayIdle();
                }
                float distToTarget = Vector3.Distance(transform.position, targetTracker.CurrentTarget.transform.position);
                if (distToTarget > attackRadius)
                {
                    ChangeState(ClownState.Run);
                }
                else if (distToTarget <= attackRadius && Time.time - lastAttackTimestamp >= attackInterval)
                {
                    ChangeState(ClownState.Attack);
                }
                break;
            case ClownState.Die:
                // Death handled by coroutine
                break;
        }
    }

    private void ChangeState(ClownState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        switch (newState)
        {
            case ClownState.Idle:
                agent.isStopped = true;
                PlayIdle();
                break;
            case ClownState.Run:
                agent.isStopped = false;
                PlayRun();
                break;
            case ClownState.Attack:
                agent.isStopped = true;
                StartCoroutine(AttackCoroutine());
                break;
            case ClownState.Stunned:
                agent.isStopped = true;
                PlayStunned();
                stunnedTimer = stunDuration;
                break;
            case ClownState.Die:
                agent.isStopped = true;
                PlayDie();
                break;
        }
    }

    private IEnumerator AttackCoroutine()
    {
        PlayGrab();
        float attackAnimLength = GetAnimationClipLength(ANIM_GRAB, attackAnimLengthFallback);
        float attackHitMoment = attackAnimLength * 0.5f;
        yield return new WaitForSeconds(attackHitMoment);
        if (targetTracker.CurrentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetTracker.CurrentTarget.transform.position);
            if (distanceToTarget <= attackRadius)
            {
                if (targetTracker.CurrentTarget.TryGetComponent<HealthController>(out var targetHealth))
                {
                    targetHealth.Damage(damage);
                    if (attackSound != null)
                    {
                        audioSource.PlayOneShot(attackSound);
                    }
                }
            }
        }
        yield return new WaitForSeconds(attackAnimLength - attackHitMoment);
        lastAttackTimestamp = Time.time;
        PlayLaugh();
        audioSource.PlayOneShot(laughSound);
        yield return new WaitForSeconds(laughSound.length);
        ChangeState(ClownState.Idle);
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

    private void OnDamage()
    {
        ChangeState(ClownState.Stunned);
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
        ChangeState(ClownState.Die);
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        audioSource.Stop();
        float dieAnimLength = GetAnimationClipLength(ANIM_DIE, dieAnimLengthFallback);
        yield return new WaitForSeconds(dieAnimLength + 1f);
        Destroy(gameObject);
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
