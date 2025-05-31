using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingScoreText : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float floatHeight = 2f;
    [SerializeField] private float animationDuration = 1.5f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Visual Settings")]
    [SerializeField] private float baseScale = 1f;
    [SerializeField] private float maxScale = 1.5f;
    [SerializeField] private Color scoreColor = Color.yellow;
    [SerializeField] private Vector3 randomDrift = new(0.5f, 0f, 0.5f);

    private TextMeshPro textMesh;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 driftOffset;
    private Camera playerCamera;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null)
        {
            textMesh = gameObject.AddComponent<TextMeshPro>();
        }

        // Find the main camera or player camera
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindFirstObjectByType<Camera>();
        }
    }

    public void Initialize(int scoreValue, Vector3 worldPosition)
    {
        // Set up the text
        textMesh.text = "+" + scoreValue.ToString();
        textMesh.color = scoreColor;
        textMesh.fontSize = 4f;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.sortingOrder = 100; // Ensure it renders on top

        // Position setup
        startPosition = worldPosition;
        transform.position = startPosition;

        // Calculate random drift
        driftOffset = new Vector3(
            Random.Range(-randomDrift.x, randomDrift.x),
            0f,
            Random.Range(-randomDrift.z, randomDrift.z)
        );

        targetPosition = startPosition + Vector3.up * floatHeight + driftOffset;

        // Start the animation
        StartCoroutine(AnimateScore());
    }

    private void Update()
    {
        // Always face the camera
        if (playerCamera != null)
        {
            Vector3 directionToCamera = playerCamera.transform.position - transform.position;
            directionToCamera.y = 0; // Keep upright
            if (directionToCamera != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(-directionToCamera);
            }
        }
    }

    private IEnumerator AnimateScore()
    {
        float elapsedTime = 0f;
        Vector3 initialScale = Vector3.one * baseScale;
        Color initialColor = textMesh.color;

        while (elapsedTime < animationDuration)
        {
            float normalizedTime = elapsedTime / animationDuration;

            // Position animation
            float movementProgress = movementCurve.Evaluate(normalizedTime);
            transform.position = Vector3.Lerp(startPosition, targetPosition, movementProgress);

            // Scale animation
            float scaleProgress = scaleCurve.Evaluate(normalizedTime);
            float currentScale = Mathf.Lerp(baseScale, maxScale, scaleProgress);
            transform.localScale = Vector3.one * currentScale;

            // Alpha animation
            float alphaProgress = alphaCurve.Evaluate(normalizedTime);
            Color currentColor = initialColor;
            currentColor.a = alphaProgress;
            textMesh.color = currentColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Destroy the object when animation is complete
        Destroy(gameObject);
    }
}
