using UnityEngine;

public class WalkerClownTexture : MonoBehaviour
{
    [SerializeField] private Texture[] textures;

    void Start()
    {
        if (textures == null || textures.Length == 0)
        {
            Debug.LogWarning("No textures assigned in the inspector.");
            enabled = false;
            return;
        }

        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("Renderer not found!");
            enabled = false;
            return;
        }

        // Select a random texture from the array
        int randomIndex = Random.Range(0, textures.Length);
        renderer.material.mainTexture = textures[randomIndex];
    }
}