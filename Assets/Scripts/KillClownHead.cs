using UnityEngine;

public class KillColownHead : MonoBehaviour
{
    private int laserCollisionCount = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // This method is called when another collider enters the collider attached to this object
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            HandleDestruction();
        }
        else if (collision.gameObject.CompareTag("Laser"))
        {
            laserCollisionCount++;
            if (laserCollisionCount >= 3)
            {
                HandleDestruction();
            }
        }
    }

    public void HandleDestruction()
    {
        // Display "100" at the position of the enemy
        GameObject scoreText = new GameObject("ScoreText");
        scoreText.transform.position = transform.position;
        TextMesh textMesh = scoreText.AddComponent<TextMesh>();
        textMesh.text = "100";
        textMesh.fontSize = 12;
        textMesh.color = Color.white;

        // Add LookAtCamera component to make the text face the camera
        scoreText.AddComponent<LookAtCamera>();

        // Destroy the score text after 3 seconds
        Destroy(scoreText, 3f);

        // Destroy the enemy object after 0.3 seconds
        Destroy(gameObject, 0.3f);
    }
}