using UnityEngine;
using System.Collections.Generic;

public class ScoreTextManager : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private GameObject floatingScoreTextPrefab;

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private bool allowPoolExpansion = true;

    [Header("Score Colors")]
    [SerializeField] private Color lowScoreColor = Color.white;
    [SerializeField] private Color mediumScoreColor = Color.yellow;
    [SerializeField] private Color highScoreColor = Color.orange;
    [SerializeField] private Color criticalScoreColor = Color.red;

    [Header("Score Thresholds")]
    [SerializeField] private int mediumScoreThreshold = 100;
    [SerializeField] private int highScoreThreshold = 200;
    [SerializeField] private int criticalScoreThreshold = 500;

    private static ScoreTextManager instance;
    public static ScoreTextManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ScoreTextManager>();
                if (instance == null)
                {
                    GameObject go = new("ScoreTextManager");
                    instance = go.AddComponent<ScoreTextManager>();
                }
            }
            return instance;
        }
    }

    private Queue<GameObject> scoreTextPool = new();
    private List<GameObject> activeScoreTexts = new();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializePool()
    {
        if (floatingScoreTextPrefab == null)
        {
            CreateDefaultFloatingTextPrefab();
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePooledObject();
        }
    }

    private void CreateDefaultFloatingTextPrefab()
    {
        // Create a default prefab if none is assigned
        GameObject prefab = new("FloatingScoreText");
        prefab.AddComponent<FloatingScoreText>();
        floatingScoreTextPrefab = prefab;

        Debug.LogWarning("No FloatingScoreText prefab assigned to ScoreTextManager. Using default.");
    }

    private GameObject CreatePooledObject()
    {
        GameObject obj = Instantiate(floatingScoreTextPrefab);
        obj.SetActive(false);
        scoreTextPool.Enqueue(obj);
        return obj;
    }

    public void ShowScore(int scoreValue, Vector3 worldPosition)
    {
        GameObject scoreTextObj = GetPooledObject();
        if (scoreTextObj == null) return;

        FloatingScoreText floatingText = scoreTextObj.GetComponent<FloatingScoreText>();
        if (floatingText == null)
        {
            Debug.LogError("FloatingScoreText component not found on prefab!");
            return;
        }

        // Set the color based on score value
        Color scoreColor = GetScoreColor(scoreValue);

        scoreTextObj.SetActive(true);
        floatingText.Initialize(scoreValue, worldPosition);

        // Apply color to the text component
        var textMesh = scoreTextObj.GetComponent<TMPro.TextMeshPro>();
        if (textMesh != null)
        {
            var color = textMesh.color;
            color = scoreColor;
            textMesh.color = color;
        }

        activeScoreTexts.Add(scoreTextObj);

        // Remove from active list when destroyed (handled by FloatingScoreText)
        StartCoroutine(RemoveFromActiveListAfterDelay(scoreTextObj, 2f));
    }

    private GameObject GetPooledObject()
    {
        if (scoreTextPool.Count > 0)
        {
            return scoreTextPool.Dequeue();
        }
        else if (allowPoolExpansion)
        {
            return CreatePooledObject();
        }

        Debug.LogWarning("ScoreText pool is empty and expansion is disabled!");
        return null;
    }

    private Color GetScoreColor(int score)
    {
        if (score >= criticalScoreThreshold)
            return criticalScoreColor;
        else if (score >= highScoreThreshold)
            return highScoreColor;
        else if (score >= mediumScoreThreshold)
            return mediumScoreColor;
        else
            return lowScoreColor;
    }

    private System.Collections.IEnumerator RemoveFromActiveListAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            activeScoreTexts.Remove(obj);
            obj.SetActive(false);
            scoreTextPool.Enqueue(obj);
        }
    }

    public void ClearAllScoreTexts()
    {
        foreach (GameObject obj in activeScoreTexts)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                scoreTextPool.Enqueue(obj);
            }
        }
        activeScoreTexts.Clear();
    }
}
