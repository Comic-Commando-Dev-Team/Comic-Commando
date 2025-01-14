using UnityEngine;

public class MoveTowardsPlayer : MonoBehaviour
{
    [SerializeField] private GameObject PlayerObject;
    [SerializeField] private float MovementSpeed = 0.3f;
    [SerializeField] private Animator Animator;
    private float MovementVelocity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MovementVelocity = MovementSpeed * Time.deltaTime;
        PlayerObject = GameObject.FindGameObjectWithTag("Player");
        Animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerObject != null && PlayerObject.activeSelf == true)
        {
            transform.position += transform.forward * MovementVelocity;
            Animator.SetBool("PlayerIsAlive", true);
        }
        if (PlayerObject == null || PlayerObject.activeSelf == false)
        {
            Animator.SetBool("PlayerIsAlive", false);
        }
    }
}
