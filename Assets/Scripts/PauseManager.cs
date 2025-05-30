using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

/// <summary>
/// Manages game pause functionality for VR interruptions such as:
/// - Oculus menu being displayed
/// - VR headset being taken off
/// - Application losing focus
/// - VR tracking being lost
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("Pause Settings")]
    [SerializeField]
    [Tooltip("Should the game automatically pause when VR interruptions occur?")]
    private bool _autoPauseOnVRInterruption = true;

    [SerializeField]
    [Tooltip("Time scale to use when paused (0 = completely paused)")]
    private float _pausedTimeScale = 0f;

    [SerializeField]
    [Tooltip("Should audio be paused when the game is paused?")]
    private bool _pauseAudioOnPause = true;

    [Header("Debug")]
    [SerializeField]
    [Tooltip("Log pause/resume events for debugging")]
    private bool _logPauseEvents = true;

    [Header("Events")]
    [SerializeField] private UnityEvent OnGamePaused = new();
    [SerializeField] private UnityEvent OnGameResumed = new();

    // Private fields
    private bool _isPaused = false;
    private float _originalTimeScale = 1f;
    private bool _wasTrackingActive = true;
    private bool _hadApplicationFocus = true;

    // Properties
    public bool IsPaused => _isPaused;
    public bool AutoPauseEnabled
    {
        get => _autoPauseOnVRInterruption;
        set => _autoPauseOnVRInterruption = value;
    }

    #region Unity Lifecycle
    private void Start()
    {
        // Store original time scale
        _originalTimeScale = Time.timeScale;

        // Initialize tracking state
        _wasTrackingActive = IsVRTrackingActive();
        _hadApplicationFocus = Application.isFocused;

        if (_logPauseEvents)
        {
            Debug.Log("VRPauseManager: Initialized with auto-pause " +
                     (_autoPauseOnVRInterruption ? "enabled" : "disabled"));
        }
    }

    private void Update()
    {
        if (!_autoPauseOnVRInterruption) return;

        CheckVRInterruptions();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!_autoPauseOnVRInterruption) return;

        if (_logPauseEvents)
        {
            Debug.Log($"VRPauseManager: Application focus changed to: {hasFocus}");
        }

        if (hasFocus && !_hadApplicationFocus)
        {
            // Application regained focus
            if (_isPaused)
            {
                ResumeGame();
            }
        }
        else if (!hasFocus && _hadApplicationFocus)
        {
            // Application lost focus
            if (!_isPaused)
            {
                PauseGame();
            }
        }

        _hadApplicationFocus = hasFocus;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!_autoPauseOnVRInterruption) return;

        if (_logPauseEvents)
        {
            Debug.Log($"VRPauseManager: Application pause status changed to: {pauseStatus}");
        }

        if (pauseStatus && !_isPaused)
        {
            PauseGame();
        }
        else if (!pauseStatus && _isPaused)
        {
            ResumeGame();
        }
    }
    #endregion

    #region VR Tracking Detection
    private void CheckVRInterruptions()
    {
        bool isTrackingActive = IsVRTrackingActive();

        // Check if tracking state changed
        if (isTrackingActive != _wasTrackingActive)
        {
            if (_logPauseEvents)
            {
                Debug.Log($"VRPauseManager: VR tracking changed to: {isTrackingActive}");
            }

            if (!isTrackingActive && !_isPaused)
            {
                // VR tracking lost (headset taken off or menu shown)
                PauseGame();
            }
            else if (isTrackingActive && _isPaused)
            {
                // VR tracking restored
                ResumeGame();
            }

            _wasTrackingActive = isTrackingActive;
        }

        // Additional check for OVR-specific states if available
        CheckOVRSpecificStates();
    }
    private bool IsVRTrackingActive()
    {
        // Check if VR is enabled and tracking
        if (!UnityEngine.XR.XRSettings.enabled)
            return false;

        // Get head device
        var headDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.Head, headDevices);

        if (headDevices.Count == 0)
            return false;

        var headDevice = headDevices[0];

        // Check head tracking position validity
        Vector3 headPose = Vector3.zero;
        Quaternion headRotation = Quaternion.identity;

        bool hasPosition = headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out headPose);
        bool hasRotation = headDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out headRotation);

        // If head position is at origin and rotation is identity, likely not tracking
        bool hasValidPosition = hasPosition && headPose != Vector3.zero;
        bool hasValidRotation = hasRotation && headRotation != Quaternion.identity;

        return hasValidPosition && hasValidRotation;
    }

    private void CheckOVRSpecificStates()
    {
        // This method checks for OVR-specific states
        // Note: These checks require the OVR SDK to be available

        try
        {
            // Check if we can access OVRManager (this will fail gracefully if OVR isn't available)
            var ovrManagerType = System.Type.GetType("OVRManager");
            if (ovrManagerType != null)
            {
                // Use reflection to check OVR states safely
                CheckOVRStatesViaReflection(ovrManagerType);
            }
        }
        catch (System.Exception ex)
        {
            // OVR SDK not available or other error - this is fine, we fall back to generic VR checks
            if (_logPauseEvents)
            {
                Debug.Log($"VRPauseManager: OVR-specific checks not available: {ex.Message}");
            }
        }
    }

    private void CheckOVRStatesViaReflection(System.Type ovrManagerType)
    {
        try
        {
            // Check if OVR manager instance exists
            var instanceProperty = ovrManagerType.GetProperty("instance",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            if (instanceProperty != null)
            {
                var ovrInstance = instanceProperty.GetValue(null);
                if (ovrInstance != null)
                {
                    // Check for app focus in OVR
                    var hasVrFocusProperty = ovrManagerType.GetProperty("hasVrFocus",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    if (hasVrFocusProperty != null)
                    {
                        bool hasVrFocus = (bool)hasVrFocusProperty.GetValue(ovrInstance);

                        if (!hasVrFocus && !_isPaused)
                        {
                            if (_logPauseEvents)
                            {
                                Debug.Log("VRPauseManager: OVR VR focus lost - pausing game");
                            }
                            PauseGame();
                        }
                        else if (hasVrFocus && _isPaused)
                        {
                            if (_logPauseEvents)
                            {
                                Debug.Log("VRPauseManager: OVR VR focus regained - resuming game");
                            }
                            ResumeGame();
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            if (_logPauseEvents)
            {
                Debug.Log($"VRPauseManager: Error checking OVR states: {ex.Message}");
            }
        }
    }
    #endregion

    #region Pause/Resume Methods
    /// <summary>
    /// Manually pause the game
    /// </summary>
    public void PauseGame()
    {
        if (_isPaused) return;

        if (_logPauseEvents)
        {
            Debug.Log("VRPauseManager: Pausing game");
        }

        _isPaused = true;

        // Pause time
        Time.timeScale = _pausedTimeScale;

        // Pause audio if enabled
        if (_pauseAudioOnPause)
        {
            AudioListener.pause = true;
        }

        // Trigger event
        OnGamePaused.Invoke();
    }

    /// <summary>
    /// Manually resume the game
    /// </summary>
    public void ResumeGame()
    {
        if (!_isPaused) return;

        if (_logPauseEvents)
        {
            Debug.Log("VRPauseManager: Resuming game");
        }

        _isPaused = false;

        // Restore time scale
        Time.timeScale = _originalTimeScale;

        // Resume audio if it was paused
        if (_pauseAudioOnPause)
        {
            AudioListener.pause = false;
        }

        // Trigger event
        OnGameResumed.Invoke();
    }

    /// <summary>
    /// Toggle pause state
    /// </summary>
    public void TogglePause()
    {
        if (_isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Force resume (useful for manual override)
    /// </summary>
    public void ForceResume()
    {
        if (_logPauseEvents)
        {
            Debug.Log("VRPauseManager: Force resuming game");
        }

        _isPaused = false;
        Time.timeScale = _originalTimeScale;

        if (_pauseAudioOnPause)
        {
            AudioListener.pause = false;
        }

        OnGameResumed.Invoke();
    }
    #endregion

    #region Utility Methods
    /// <summary>
    /// Set the time scale to use when the game is resumed
    /// </summary>
    /// <param name="timeScale">The new time scale (1.0 = normal speed)</param>
    public void SetOriginalTimeScale(float timeScale)
    {
        _originalTimeScale = timeScale;

        // If not paused, apply immediately
        if (!_isPaused)
        {
            Time.timeScale = _originalTimeScale;
        }
    }

    /// <summary>
    /// Get the current original time scale
    /// </summary>
    public float GetOriginalTimeScale()
    {
        return _originalTimeScale;
    }
    #endregion

    #region Debug Methods
#if UNITY_EDITOR
    [System.Serializable]
    public class DebugInfo
    {
        public bool isPaused;
        public bool vrTrackingActive;
        public bool applicationFocused;
        public float currentTimeScale;
        public bool audioListenerPaused;
    }

    [Header("Debug Info (Read Only)")]
    [SerializeField] private DebugInfo debugInfo = new();

    private void LateUpdate()
    {
        // Update debug info
        debugInfo.isPaused = _isPaused;
        debugInfo.vrTrackingActive = IsVRTrackingActive();
        debugInfo.applicationFocused = Application.isFocused;
        debugInfo.currentTimeScale = Time.timeScale;
        debugInfo.audioListenerPaused = AudioListener.pause;
    }
#endif
    #endregion
}
