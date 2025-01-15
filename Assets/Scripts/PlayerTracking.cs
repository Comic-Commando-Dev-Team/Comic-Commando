using UnityEngine;

public class PlayerTracking : MonoBehaviour
{
    [SerializeField] GameObject PlayerObject;

    void Start()
    {
        PlayerObject = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (PlayerObject != null)
        {
            LookAtPlayer(); // Makes sure Object always faces the player
        }
    }

    void LookAtPlayer()
    {
        Vector3 playerPos = PlayerObject.transform.position;
        Vector3 lookVector = new Vector3(playerPos.x, transform.position.y, playerPos.z) - transform.position;
        Quaternion rot = Quaternion.LookRotation(lookVector);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f); // 5f to szybkoœæ obrotu
    }
}

