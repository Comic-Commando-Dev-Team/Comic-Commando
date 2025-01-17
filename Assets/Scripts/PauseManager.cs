using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public Transform leftHand;  // Referencja do lewej rêki gracza
    public Transform rightHand; // Referencja do prawej rêki gracza
    public float heightThreshold = 1.5f; // Wysokoœæ, powy¿ej której rêce s¹ uznawane za podniesione (np. 1.5 metra od ziemi)

    private bool isPaused = false; // Czy gra jest wstrzymana
    private bool handsLowered = true; // Czy rêce zosta³y opuszczone po poprzedniej akcji

    void Update()
    {
        // Sprawdzenie pozycji r¹k
        bool leftHandUp = leftHand.position.y > heightThreshold;
        bool rightHandUp = rightHand.position.y > heightThreshold;

        // Obie rêce musz¹ byæ podniesione
        if (leftHandUp && rightHandUp && handsLowered)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }

            // Po wykonaniu akcji oznaczamy, ¿e rêce nie s¹ ju¿ "opuszczone"
            handsLowered = false;
        }

        // Rêce musz¹ zostaæ opuszczone, aby mo¿liwa by³a kolejna akcja
        if (!leftHandUp && !rightHandUp)
        {
            handsLowered = true;
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0; // Zatrzymanie czasu w grze
        isPaused = true;
    }

    void ResumeGame()
    {
        Time.timeScale = 1; // Wznowienie czasu w grze
        isPaused = false;
    }
}
