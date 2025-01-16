using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void Update()
    {
        // Make the text face the camera
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0); // Rotate 180 degrees to face the camera correctly
    }
}