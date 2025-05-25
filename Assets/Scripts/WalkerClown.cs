using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(TargetTracker))]
[RequireComponent(typeof(HealthController))]
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

    [Header("References")]
    [SerializeField] private GameObject deathImagePrefab;
    [SerializeField] private AudioClip stopSound;

    [Header("Animation Lengths (Fallback)")]
    [SerializeField] private float grabAnimLengthFallback = 1.2f;
    [SerializeField] private float dieAnimLengthFallback = 1.0f;

    private Animator animator;
    private HealthController healthController;
    private TargetTracker targetTracker;
    private AudioSource audioSource;
    private ScoreBoard scoreBoard;

    private bool isStopped = false;
    private float lastStopSoundTime = -3f;
    private bool isStunned = false;
    private float stunnedTimer = 0f;
    private float stunnedDuration = 1.0f;

    private void Start()
    {
        animator = GetComponent<Animator>();
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
            }
            return;
        }

        float distance = Vector3.Distance(transform.position, targetTracker.CurrentTarget.transform.position);
        Vector3 direction = (targetTracker.CurrentTarget.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * moveSpeed);

        if (distance > stoppingDistance)
        {
            transform.position += moveSpeed * Time.deltaTime * direction;
            if (!animator.GetBool(ANIM_RUN))
            {
                animator.SetBool(ANIM_RUN, true);
                animator.SetBool(ANIM_IDLE, false);
            }
            isStopped = false;
        }
        else
        {
            if (!animator.GetBool(ANIM_IDLE))
            {
                animator.SetBool(ANIM_RUN, false);
                animator.SetBool(ANIM_IDLE, true);
            }
            if (!isStopped)
            {
                isStopped = true;
                StartCoroutine(HandleStopActions());
            }
        }
    }

    private IEnumerator HandleStopActions()
    {
        PlayGrab();
        float grabAnimLength = GetAnimationClipLength(ANIM_GRAB, grabAnimLengthFallback);
        yield return new WaitForSeconds(grabAnimLength);

        if (audioSource != null && stopSound != null && Time.time - lastStopSoundTime >= 3f)
        {
            audioSource.PlayOneShot(stopSound);
            lastStopSoundTime = Time.time;
        }
        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
