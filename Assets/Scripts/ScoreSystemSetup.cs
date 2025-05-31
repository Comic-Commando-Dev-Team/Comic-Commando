using UnityEngine;
using TMPro;

[System.Serializable]
public class ScoreSystemSetup : MonoBehaviour
{
    [Header("Setup Instructions")]
    [SerializeField] private bool setupComplete = false;

    [Header("Prefab Creation")]
    [SerializeField] private bool createFloatingTextPrefab = true;

    [ContextMenu("Setup Score System")]
    public void SetupScoreSystem()
    {
        if (setupComplete)
        {
            Debug.Log("Score system already set up!");
            return;
        }

        // 1. Create ScoreTextManager in the scene
        CreateScoreTextManager();

        // 2. Create FloatingScoreText prefab
        if (createFloatingTextPrefab)
        {
            CreateFloatingTextPrefab();
        }

        setupComplete = true;
        Debug.Log("Score system setup complete! The ScoreTextManager will now handle all floating score text.");
    }

    private void CreateScoreTextManager()
    {
        ScoreTextManager existing = FindFirstObjectByType<ScoreTextManager>();
        if (existing != null)
        {
            Debug.Log("ScoreTextManager already exists in scene.");
            return;
        }

        GameObject managerObj = new GameObject("ScoreTextManager");
        ScoreTextManager manager = managerObj.AddComponent<ScoreTextManager>();

        Debug.Log("Created ScoreTextManager in scene.");
    }

    private void CreateFloatingTextPrefab()
    {
        // Create the prefab object
        GameObject prefab = new GameObject("FloatingScoreText");

        // Add FloatingScoreText component
        FloatingScoreText floatingText = prefab.AddComponent<FloatingScoreText>();

        // Add TextMeshPro component
        TextMeshPro textMesh = prefab.AddComponent<TextMeshPro>();
        textMesh.text = "+100";
        textMesh.fontSize = 4f;
        textMesh.color = Color.yellow;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.sortingOrder = 100;

        // Set up the font (use default TextMeshPro font)
        TMP_FontAsset defaultFont = Resources.GetBuiltinResource<TMP_FontAsset>("LegacyRuntime.fontsettings");
        if (defaultFont != null)
        {
            textMesh.font = defaultFont;
        }

        Debug.Log("Created FloatingScoreText prefab. Save it to your Prefabs folder and assign it to the ScoreTextManager!");

        // Select the prefab in the hierarchy for easy saving
#if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = prefab;
#endif
    }
}