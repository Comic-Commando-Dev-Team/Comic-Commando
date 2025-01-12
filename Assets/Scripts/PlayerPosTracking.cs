using UnityEngine;

public class PlayerPosTracking : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] float MovementSpeed = 0.3f;
    float MovementVelocity;
    void Start()
    {
        MovementVelocity = MovementSpeed * Time.deltaTime;
        Player = GameObject.FindGameObjectWithTag("Player");
    }
    void Update()
    {
        MoveToPlayer();
    }

    void MoveToPlayer()
    {
        if (Player != null) {
            LookAtPlayer(); // Makes sure Object always facer player
        }

        transform.position += transform.forward * MovementVelocity;
    }

    void LookAtPlayer()
    {
        Vector3 PlayerPos = Player.transform.position;
        transform.LookAt(PlayerPos); // Laser tracking capabilities
    }
}
