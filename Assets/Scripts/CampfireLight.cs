using UnityEngine;

/// <summary>
/// Realistic campfire light script that simulates natural flame flickering
/// Attach this script to a GameObject with a Light component
/// </summary>
[RequireComponent(typeof(Light))]
public class CampfireLight : MonoBehaviour
{
    [Header("Intensity Settings")]
    [SerializeField] private float baseIntensity = 1.5f;
    [SerializeField] private float intensityVariation = 0.8f;
    [SerializeField] private float intensitySpeed = 3.0f;

    [Header("Color Settings")]
    [SerializeField] private Color baseColor = new Color(1.0f, 0.6f, 0.2f); // Orange flame color
    [SerializeField] private Color colorVariation = new Color(0.3f, 0.2f, 0.1f);
    [SerializeField] private float colorSpeed = 2.5f;

    [Header("Range Settings")]
    [SerializeField] private float baseRange = 8.0f;
    [SerializeField] private float rangeVariation = 1.5f;
    [SerializeField] private float rangeSpeed = 1.8f;

    [Header("Flicker Settings")]
    [SerializeField] private bool enableRandomFlicker = true;
    [SerializeField] private float flickerChance = 0.02f; // 2% chance per frame
    [SerializeField] private float flickerIntensity = 0.3f;
    [SerializeField] private float flickerDuration = 0.1f;

    [Header("Wind Effect")]
    [SerializeField] private bool enableWindEffect = true;
    [SerializeField] private float windStrength = 0.5f;
    [SerializeField] private float windSpeed = 1.2f;

    [Header("Performance")]
    [SerializeField] private float updateFrequency = 30f; // Updates per second (lower = better performance)
    [SerializeField] private bool enableColorVariation = true;
    [SerializeField] private bool enableRangeVariation = false;
    [SerializeField] private bool useLowQualityNoise = true; // Use faster sine waves instead of Perlin noise

    // Private variables
    private Light fireLight;
    private float timeOffset;
    private float flickerTimer;
    private bool isFlickering;
    private float originalIntensity;
    private float updateTimer;
    private float targetIntensity;
    private float targetRange;
    private Color targetColor;

    // Cached noise values for performance
    private float cachedIntensityNoise;
    private float cachedColorNoise;
    private float cachedRangeNoise;
    private float cachedWindNoise;

    // Noise offsets for more organic movement
    private float intensityNoiseOffset;
    private float colorNoiseOffset;
    private float rangeNoiseOffset;
    private float windNoiseOffset;

    void Start()
    {
        fireLight = GetComponent<Light>();

        originalIntensity = fireLight.intensity;

        // Initialize random time offsets for organic variation
        timeOffset = Random.Range(0f, 100f);
        intensityNoiseOffset = Random.Range(0f, 100f);
        colorNoiseOffset = Random.Range(0f, 100f);
        rangeNoiseOffset = Random.Range(0f, 100f);
        windNoiseOffset = Random.Range(0f, 100f);

        // Initialize cached values
        targetIntensity = baseIntensity;
        targetRange = baseRange;
        targetColor = baseColor;

        // Set initial light properties
        fireLight.color = baseColor;
        fireLight.intensity = baseIntensity;
        fireLight.range = baseRange;

        // Set light type to Point for campfire effect
        fireLight.type = LightType.Point;

        // Set update timer
        updateTimer = 1f / updateFrequency;

        // Pre-calculate initial noise values
        CalculateNoiseValues();
    }

    void Update()
    {
        // Handle random flickering (this is lightweight)
        HandleRandomFlicker();

        // Only update expensive calculations at reduced frequency
        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            updateTimer = 1f / updateFrequency;
            CalculateNoiseValues();
            UpdateTargetValues();
        }

        // Smoothly interpolate to target values for smooth movement
        float lerpSpeed = Time.deltaTime * 5f;

        fireLight.intensity = Mathf.Lerp(fireLight.intensity, targetIntensity, lerpSpeed);

        if (enableColorVariation)
        {
            fireLight.color = Color.Lerp(fireLight.color, targetColor, lerpSpeed);
        }

        if (enableRangeVariation)
        {
            fireLight.range = Mathf.Lerp(fireLight.range, targetRange, lerpSpeed);
        }
    }

    private void CalculateNoiseValues()
    {
        float currentTime = Time.time + timeOffset;

        if (useLowQualityNoise)
        {
            // Use sine waves for much better performance than Perlin noise
            cachedIntensityNoise = (Mathf.Sin(currentTime * intensitySpeed + intensityNoiseOffset) + 1f) * 0.5f;
            cachedColorNoise = (Mathf.Sin(currentTime * colorSpeed + colorNoiseOffset) + 1f) * 0.5f;
            cachedRangeNoise = (Mathf.Sin(currentTime * rangeSpeed + rangeNoiseOffset) + 1f) * 0.5f;
            cachedWindNoise = (Mathf.Sin(currentTime * windSpeed + windNoiseOffset) + 1f) * 0.5f;
        }
        else
        {
            // Use Perlin noise for higher quality (more expensive)
            cachedIntensityNoise = Mathf.PerlinNoise(currentTime * intensitySpeed, intensityNoiseOffset);
            cachedColorNoise = Mathf.PerlinNoise(currentTime * colorSpeed, colorNoiseOffset);
            cachedRangeNoise = Mathf.PerlinNoise(currentTime * rangeSpeed, rangeNoiseOffset);
            cachedWindNoise = Mathf.PerlinNoise(currentTime * windSpeed, windNoiseOffset);
        }
    }

    private void UpdateTargetValues()
    {
        float windEffect = 1.0f;
        if (enableWindEffect)
        {
            windEffect = 1.0f + (cachedWindNoise - 0.5f) * windStrength;
        }

        targetIntensity = baseIntensity + (cachedIntensityNoise - 0.5f) * intensityVariation * 2.0f;
        targetIntensity *= windEffect;

        if (isFlickering)
        {
            targetIntensity *= (1.0f - flickerIntensity);
        }

        targetIntensity = Mathf.Max(0.1f, targetIntensity);

        if (enableColorVariation)
        {
            targetColor = baseColor;
            targetColor.r += (cachedColorNoise - 0.5f) * colorVariation.r * 2.0f;

            // Use same noise for G and B channels with slight offset to reduce calculations
            float colorNoise2 = useLowQualityNoise ?
                (Mathf.Sin(Time.time * colorSpeed + colorNoiseOffset + 1.5f) + 1f) * 0.5f :
                Mathf.PerlinNoise(Time.time * colorSpeed + 50f, colorNoiseOffset);
            float colorNoise3 = useLowQualityNoise ?
                (Mathf.Sin(Time.time * colorSpeed + colorNoiseOffset + 3f) + 1f) * 0.5f :
                Mathf.PerlinNoise(Time.time * colorSpeed + 100f, colorNoiseOffset);

            targetColor.g += (colorNoise2 - 0.5f) * colorVariation.g * 2.0f;
            targetColor.b += (colorNoise3 - 0.5f) * colorVariation.b * 2.0f;

            targetColor.r = Mathf.Clamp01(targetColor.r);
            targetColor.g = Mathf.Clamp01(targetColor.g);
            targetColor.b = Mathf.Clamp01(targetColor.b);
        }

        if (enableRangeVariation)
        {
            targetRange = baseRange + (cachedRangeNoise - 0.5f) * rangeVariation * 2.0f;
            targetRange *= windEffect;
            targetRange = Mathf.Max(1.0f, targetRange);
        }
    }

    private void HandleRandomFlicker()
    {
        if (enableRandomFlicker)
        {
            if (isFlickering)
            {
                flickerTimer -= Time.deltaTime;
                if (flickerTimer <= 0f)
                {
                    isFlickering = false;
                }
            }
            else
            {
                if (Random.value < flickerChance)
                {
                    isFlickering = true;
                    flickerTimer = flickerDuration;
                }
            }
        }
    }

    /// <summary>
    /// Manually trigger a flicker effect
    /// </summary>
    public void TriggerFlicker(float duration = -1f)
    {
        isFlickering = true;
        flickerTimer = duration > 0 ? duration : flickerDuration;
    }

    /// <summary>
    /// Set the wind strength dynamically
    /// </summary>
    public void SetWindStrength(float strength)
    {
        windStrength = Mathf.Clamp01(strength);
    }

    /// <summary>
    /// Enable or disable the campfire light
    /// </summary>
    public void SetCampfireEnabled(bool enabled)
    {
        fireLight.enabled = enabled;
    }

    /// <summary>
    /// Adjust the overall intensity scale
    /// </summary>
    public void SetIntensityScale(float scale)
    {
        baseIntensity = originalIntensity * scale;
    }

    void OnValidate()
    {
        // Clamp values in the inspector
        baseIntensity = Mathf.Max(0f, baseIntensity);
        intensityVariation = Mathf.Max(0f, intensityVariation);
        baseRange = Mathf.Max(0.1f, baseRange);
        rangeVariation = Mathf.Max(0f, rangeVariation);
        flickerChance = Mathf.Clamp01(flickerChance);
        flickerIntensity = Mathf.Clamp01(flickerIntensity);
        windStrength = Mathf.Clamp01(windStrength);

        // Ensure speeds are positive
        intensitySpeed = Mathf.Max(0.1f, intensitySpeed);
        colorSpeed = Mathf.Max(0.1f, colorSpeed);
        rangeSpeed = Mathf.Max(0.1f, rangeSpeed);
        windSpeed = Mathf.Max(0.1f, windSpeed);

        // Performance constraints
        updateFrequency = Mathf.Clamp(updateFrequency, 10f, 120f);
    }
}
