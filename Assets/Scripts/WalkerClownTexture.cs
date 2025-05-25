using UnityEngine;

public class WalkerClownTexture : MonoBehaviour
{
    [SerializeField] private Texture[] textures; // Array to hold the three textures

    void Start()
    {
        if (textures.Length != 3)
        {
            Debug.LogError("Please assign exactly 3 textures in the inspector.");
            return;
        }

        // Get the Renderer component
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("Renderer not found!");
            return;
        }

        // Select a random texture from the array
        int randomIndex = Random.Range(0, textures.Length);
        Debug.Log($"Random index: {randomIndex}");
        renderer.material.mainTexture = textures[randomIndex];
    }
}