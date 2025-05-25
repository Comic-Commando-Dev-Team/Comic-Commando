using UnityEngine;
using TMPro;

public class ScoreBoard : MonoBehaviour
{
    public GameObject scoreTextObject; // Obiekt gry, który zawiera komponent TextMeshPro
    private TextMeshPro scoreText;
    private int score = 0;

    void Start()
    {
        if (scoreTextObject != null)
        {
            scoreText = scoreTextObject.GetComponent<TextMeshPro>();
            if (scoreText == null)
            {
                Debug.LogError("TextMeshPro component not found on the assigned object!");
            }
        }
        else
        {
            Debug.LogError("ScoreTextObject is not assigned!");
        }

        UpdateScoreText();
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateScoreText();
        Debug.Log("Score: " + score);
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }
}