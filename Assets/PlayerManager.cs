using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(HealthController))]
public class PlayerManager : MonoBehaviour
{
    private HealthController healthController;

    private void Start()
    {
        healthController = GetComponent<HealthController>();
        if (healthController != null)
        {
            healthController.OnDeath.AddListener(OnDeath);
        }
    }

    private void OnDeath()
    {
        Debug.Log("Player died!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
