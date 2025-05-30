using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

/// <summary>
/// VR Flashlight that can be toggled on/off and managed by the ItemSwitcher system.
/// Compatible with hand tracking and provides illumination for dark environments.
/// </summary>
public class Flashlight : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private Light _flashlightBeam;
    [SerializeField] private float _maxRange = 20f;
    [SerializeField] private float _intensity = 3f;
    [SerializeField] private float _spotAngle = 45f;
    [SerializeField] private Color _lightColor = Color.white;

    [Header("Battery System")]
    [SerializeField] private bool _useBatterySystem = true;
    [SerializeField] private float _maxBatteryLife = 300f; // 5 minutes
    [SerializeField] private float _batteryDrainRate = 1f; // Per second when on
    [SerializeField] private AnimationCurve _batteryIntensityCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 1f);

    [Header("Visual Feedback")]
    [SerializeField] private GameObject _flashlightModel;
    [SerializeField] private Renderer _batteryIndicator;
    [SerializeField] private Material _batteryFullMaterial;
    [SerializeField] private Material _batteryLowMaterial;
    [SerializeField] private Material _batteryEmptyMaterial;
    [SerializeField] private float _lowBatteryThreshold = 0.25f;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _clickOnSound;
    [SerializeField] private AudioClip _clickOffSound;
    [SerializeField] private AudioClip _batteryLowSound;
    [SerializeField] private bool _playLowBatteryWarning = true;

    [Header("Input")]
    [SerializeField] private InputActionProperty _toggleAction;
    [SerializeField] private XRNode _controllerNode = XRNode.RightHand;

    [Header("Performance")]
    [SerializeField] private bool _enableShadows = true;
    [SerializeField] private LightShadows _shadowType = LightShadows.Soft;
    [SerializeField] private float _updateInterval = 0.1f; // Battery update frequency

    [Header("Debug")]
    [SerializeField] private bool _showDebugInfo;
    [SerializeField] private bool _unlimitedBattery;

    // Private fields
    private bool _isOn;
    private float _currentBattery;
    private bool _batteryDepleted;
    private float _nextUpdateTime;
    private bool _lowBatteryWarningPlayed;
    private float _originalIntensity;

    // Properties
    public bool IsOn => _isOn && !_batteryDepleted;
    public float BatteryPercentage => _useBatterySystem ? _currentBattery / _maxBatteryLife : 1f;
    public bool IsBatteryLow => BatteryPercentage <= _lowBatteryThreshold;
    public bool IsBatteryDepleted => _batteryDepleted;

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeFlashlight();
    }

    private void OnEnable()
    {
        _toggleAction.action?.Enable();

        // Reset low battery warning when item is enabled
        _lowBatteryWarningPlayed = false;
    }

    private void OnDisable()
    {
        _toggleAction.action?.Disable();

        // Turn off the light when disabled to save battery
        if (_isOn)
        {
            SetLightState(false, false); // Don't play sound when disabled by item switcher
        }
    }

    private void Update()
    {
        HandleInput();
        UpdateBattery();
        UpdateVisualFeedback();
    }
    #endregion

    #region Initialization
    private void InitializeFlashlight()
    {
        // Initialize battery
        _currentBattery = _maxBatteryLife;
        _batteryDepleted = false;

        // Setup light component
        if (_flashlightBeam == null)
        {
            _flashlightBeam = GetComponentInChildren<Light>();
        }

        if (_flashlightBeam != null)
        {
            _originalIntensity = _intensity;
            ConfigureLight();
            _flashlightBeam.enabled = false;
        }
        else
        {
            Debug.LogWarning("VRFlashlight: No Light component found! Please assign a Light component.");
        }

        // Setup audio
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Initialize visual state
        _isOn = false;
        UpdateBatteryIndicator();
    }

    private void ConfigureLight()
    {
        _flashlightBeam.type = LightType.Spot;
        _flashlightBeam.range = _maxRange;
        _flashlightBeam.intensity = _intensity;
        _flashlightBeam.spotAngle = _spotAngle;
        _flashlightBeam.color = _lightColor;
        _flashlightBeam.shadows = _enableShadows ? _shadowType : LightShadows.None;

        // Optimize for VR performance
        _flashlightBeam.renderMode = LightRenderMode.ForcePixel;
        _flashlightBeam.cullingMask = ~0; // All layers by default
    }
    #endregion

    #region Input Handling
    private void HandleInput()
    {
        if (_toggleAction.action?.WasPerformedThisFrame() == true)
        {
            ToggleFlashlight();
        }
    }
    #endregion

    #region Core Functionality
    public void ToggleFlashlight()
    {
        if (_batteryDepleted && !_unlimitedBattery)
        {
            // Play empty battery sound or feedback
            if (_audioSource && _batteryLowSound)
            {
                _audioSource.PlayOneShot(_batteryLowSound);
            }
            SendHapticFeedback(0.3f, 0.1f);
            return;
        }

        SetLightState(!_isOn, true);
    }

    private void SetLightState(bool state, bool playSound = true)
    {
        if (_batteryDepleted && state && !_unlimitedBattery)
            return;

        _isOn = state;

        if (_flashlightBeam != null)
        {
            _flashlightBeam.enabled = _isOn;
        }

        // Play audio feedback
        if (playSound && _audioSource)
        {
            AudioClip soundToPlay = _isOn ? _clickOnSound : _clickOffSound;
            if (soundToPlay != null)
            {
                _audioSource.PlayOneShot(soundToPlay);
            }
        }

        // Haptic feedback
        if (playSound) // Use same condition as audio for haptics
        {
            SendHapticFeedback(_isOn ? 0.4f : 0.2f, 0.1f);
        }

        if (_showDebugInfo)
        {
            Debug.Log($"VRFlashlight: Light {(_isOn ? "ON" : "OFF")} - Battery: {BatteryPercentage:P0}");
        }
    }
    #endregion

    #region Battery System
    private void UpdateBattery()
    {
        if (!_useBatterySystem || _unlimitedBattery || Time.time < _nextUpdateTime)
            return;

        _nextUpdateTime = Time.time + _updateInterval;

        if (_isOn && _flashlightBeam && _flashlightBeam.enabled)
        {
            _currentBattery -= _batteryDrainRate * _updateInterval;
            _currentBattery = Mathf.Max(0f, _currentBattery);

            // Check if battery is depleted
            if (_currentBattery <= 0f)
            {
                _batteryDepleted = true;
                SetLightState(false, true);

                if (_showDebugInfo)
                {
                    Debug.Log("VRFlashlight: Battery depleted!");
                }
            }
            else
            {
                // Adjust intensity based on battery level
                float batteryRatio = BatteryPercentage;
                float intensityMultiplier = _batteryIntensityCurve.Evaluate(batteryRatio);
                _flashlightBeam.intensity = _originalIntensity * intensityMultiplier;

                // Play low battery warning
                if (IsBatteryLow && !_lowBatteryWarningPlayed && _playLowBatteryWarning)
                {
                    if (_audioSource && _batteryLowSound)
                    {
                        _audioSource.PlayOneShot(_batteryLowSound);
                    }
                    _lowBatteryWarningPlayed = true;
                }
            }
        }
    }

    public void RechargeBattery(float amount)
    {
        if (!_useBatterySystem) return;

        _currentBattery = Mathf.Clamp(_currentBattery + amount, 0f, _maxBatteryLife);
        _batteryDepleted = _currentBattery <= 0f;
        _lowBatteryWarningPlayed = false;

        if (_showDebugInfo)
        {
            Debug.Log($"VRFlashlight: Battery recharged by {amount}. Current: {BatteryPercentage:P0}");
        }
    }

    public void SetFullBattery()
    {
        RechargeBattery(_maxBatteryLife);
    }
    #endregion

    #region Visual Feedback
    private void UpdateVisualFeedback()
    {
        UpdateBatteryIndicator();
    }

    private void UpdateBatteryIndicator()
    {
        if (_batteryIndicator == null) return;

        Material materialToUse;

        if (_batteryDepleted)
        {
            materialToUse = _batteryEmptyMaterial;
        }
        else if (IsBatteryLow)
        {
            materialToUse = _batteryLowMaterial;
        }
        else
        {
            materialToUse = _batteryFullMaterial;
        }

        if (materialToUse != null)
        {
            _batteryIndicator.material = materialToUse;
        }
    }
    #endregion

    #region Haptic Feedback
    private void SendHapticFeedback(float amplitude, float duration)
    {
        UnityEngine.XR.InputDevice device = InputDevices.GetDeviceAtXRNode(_controllerNode);
        if (device.isValid)
        {
            device.SendHapticImpulse(0, amplitude, duration);
        }
    }
    #endregion

    #region Public API
    public void TurnOn()
    {
        if (!_isOn && !_batteryDepleted)
        {
            SetLightState(true, true);
        }
    }

    public void TurnOff()
    {
        if (_isOn)
        {
            SetLightState(false, true);
        }
    }

    public void SetIntensity(float intensity)
    {
        _intensity = Mathf.Max(0f, intensity);
        _originalIntensity = _intensity;

        if (_flashlightBeam != null)
        {
            // Apply current battery effect
            float batteryMultiplier = _useBatterySystem ?
                _batteryIntensityCurve.Evaluate(BatteryPercentage) : 1f;
            _flashlightBeam.intensity = _intensity * batteryMultiplier;
        }
    }

    public void SetRange(float range)
    {
        _maxRange = Mathf.Max(0.1f, range);
        if (_flashlightBeam != null)
        {
            _flashlightBeam.range = _maxRange;
        }
    }

    public void SetSpotAngle(float angle)
    {
        _spotAngle = Mathf.Clamp(angle, 1f, 179f);
        if (_flashlightBeam != null)
        {
            _flashlightBeam.spotAngle = _spotAngle;
        }
    }
    #endregion

    #region Debug
    private void OnGUI()
    {
        if (!_showDebugInfo) return;

        GUI.Box(new Rect(10, 10, 200, 120), "VR Flashlight Debug");
        GUI.Label(new Rect(20, 35, 180, 20), $"Status: {(_isOn ? "ON" : "OFF")}");
        GUI.Label(new Rect(20, 55, 180, 20), $"Battery: {BatteryPercentage:P1}");
        GUI.Label(new Rect(20, 75, 180, 20), $"Depleted: {_batteryDepleted}");
        GUI.Label(new Rect(20, 95, 180, 20), $"Low Battery: {IsBatteryLow}");

        if (_useBatterySystem && GUI.Button(new Rect(20, 105, 80, 20), "Recharge"))
        {
            SetFullBattery();
        }
    }
    #endregion
}
