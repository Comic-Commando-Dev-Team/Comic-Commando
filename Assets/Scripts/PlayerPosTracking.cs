using UnityEngine;

public class PlayerPosTracking : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] float MovementSpeed = 5;
    [SerializeField] float TurnSpeed = 1;
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }
    void Update()
    {
        
    }
}
