using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(TargetTracker))]
[RequireComponent(typeof(HealthController))]
public class WalkerClown : MonoBehaviour
{
    [SerializeField] private float speed = 2.0f;
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private int _damage = 10;
    [SerializeField] private GameObject deathImagePrefab;
    [SerializeField] private AudioClip stopSound;
    [SerializeField] private int points = 100;
    [SerializeField] private string crouchWalkAnim = "True_Clown_Crouch_Walk";
    [SerializeField] private string grabAnim = "True_Clown_Grab";
    [SerializeField] private string stunnedAnim = "True_Clown_Stunned";
    [SerializeField] private string laughAnim = "True_Clown_Laugh";
    [SerializeField] private string dieAnim = "True_Clown_Die";

    private Animator animator;
    private HealthController healthController;
    private TargetTracker targetTracker;
    private AudioSource audioSource;
    private bool isStopped = false;
    private float lastStopSoundTime = -3f;
    private ScoreBoard scoreBoard;
    private bool isStunned = false;
    private float stunnedTimer = 0f;
    private float stunnedDuration = 1.0f;

    public void PlayIdle()
    {
        animator.SetBool("True_Clown_Idle", true);
        animator.SetBool("True_Clown_Run", false);
    }

    public void PlayRun()
    {
        animator.SetBool("True_Clown_Run", true);
        animator.SetBool("True_Clown_Idle", false);
    }

    public void PlayCrouchWalk()
    {
        animator.SetTrigger("True_Clown_Crouch_Walk");
    }

    public void PlayGrab()
    {
        animator.SetTrigger("True_Clown_Grab");
    }

    public void PlayStunned()
    {
        animator.SetTrigger("True_Clown_Stunned");
    }

    public void PlayLaugh()
    {
        animator.SetTrigger("True_Clown_Laugh");
    }

    public void PlayDie()
    {
        animator.SetTrigger("True_Clown_Die");
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        healthController = GetComponent<HealthController>();
        targetTracker = GetComponent<TargetTracker>();
        audioSource = GetComponent<AudioSource>();
        scoreBoard = FindAnyObjectByType<ScoreBoard>();

        int randomAnim = Random.value > 0.5f ? 1 : 2;
        animator.SetInteger("StartAnim", randomAnim);


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
            healthController.OnDeath.AddListener(OnDeath);
        }
    }

    void Update()
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

            isStopped = false; // Reset the stopped state
        }
        else
        {
            // Stop and play idle animation
            if (!animator.GetBool("True_Clown_Idle"))
            {
                animator.SetBool("True_Clown_Run", false);
                animator.SetBool("True_Clown_Idle", true);
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

        float grabAnimLength = 1.2f; // <- wpisz faktyczną długość animacji Grab w sekundach
        yield return new WaitForSeconds(grabAnimLength);

        // Play stop sound with cooldown
        if (audioSource != null && stopSound != null && Time.time - lastStopSoundTime >= 3f)
        {
            audioSource.PlayOneShot(stopSound);
            lastStopSoundTime = Time.time; // Update the last play time
        }

        yield return new WaitForSeconds(4f);

        // Reset the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void SpawnDeathImage()
    {
        if (deathImagePrefab != null && targetTracker.CurrentTarget != null)
        {
            Vector3 spawnPosition = transform.position;
            spawnPosition.y += 1; // Increase Y position by 1 unit

            GameObject deathImage = Instantiate(deathImagePrefab, spawnPosition, Quaternion.identity);
            Destroy(deathImage, 2f);

            // Rotate the image to face the player and adjust rotation on X axis
            Vector3 direction = (targetTracker.CurrentTarget.transform.position - deathImage.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            lookRotation *= Quaternion.Euler(0, 90, 0); // Rotate 90 degrees on Y axis
            deathImage.transform.rotation = lookRotation;
        }
    }

    private void OnDeath()
    {
        if (scoreBoard != null)
        {
            scoreBoard.AddScore(points);
        }
        SpawnDeathImage();
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        if (audioSource != null)
        audioSource.enabled = false;
        
        float previousSpeed = speed;
        speed = 0f; // Zatrzymaj klauna podczas animacji śmierci


        PlayDie();

        // Poczekaj na długość animacji
        float dieAnimLength = 1.0f; // <- wpisz tu długość animacji True_Clown_Die w sekundach
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        yield return new WaitForSeconds(dieAnimLength + 1f);

        Destroy(gameObject);
    }
    
    public void Stun()
    {
        isStunned = true;
        stunnedTimer = stunnedDuration;
        PlayStunned();
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
