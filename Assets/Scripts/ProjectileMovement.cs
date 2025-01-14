using Meta.WitAi;
using UnityEngine;

public class ProjectileMovement : MonoBehaviour
{
    [SerializeField] private float MovementSpeed = 0.5f;
    private float MovementVelocity;
    private void Start()
    {
        MovementVelocity = MovementSpeed * Time.deltaTime;
        Invoke("SelfDestroyAfterXSeconds", 10);
    }
    void Update()
    {
        transform.position += transform.forward * MovementVelocity;
        
    }
    void SelfDestroyAfterXSeconds()
    {
        Destroy(this.gameObject);
    }
}
