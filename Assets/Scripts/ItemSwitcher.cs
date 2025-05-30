using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages item switching functionality for VR hand interactions.
/// Allows cycling through items by reaching behind the player.
/// </summary>
public class ItemSwitcher : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform _headTransform;
    [Tooltip("How far behind the player's back the hand needs to be to trigger switching")]
    private float _behindBackDistance = 0.15f;
    [SerializeField]
    [Tooltip("The maximum distance between the hand and torso to trigger the switch")]
    private float _maxDistanceToTorso = 0.5f;
    [SerializeField]
    [Tooltip("Maximum height difference between hand and head level")]
    private float _maxVerticalOffset = 0.3f;
    [SerializeField]
    [Tooltip("Minimum time between item switches")]
    private float _switchCooldown = 0.5f;
    [SerializeField]
    [Tooltip("How long the hand must stay in position before triggering (helps prevent accidental switches)")]
    private float _dwellTime = 0.1f;

    [Header("Debug")]
    [SerializeField] private bool _showDebugGizmos;
    [SerializeField] private bool _logSwitchEvents;

    private List<GameObject> _availableItems = new();
    private int _currentItemIndex = -1;
    private float _lastSwitchTime;
    private bool _wasReachingBehind;
    private float _dwellStartTime; // Track how long hand has been in position

    #region Properties
    public GameObject CurrentItem =>
        _currentItemIndex >= 0 && _currentItemIndex < _availableItems.Count
            ? _availableItems[_currentItemIndex]
            : null;

    public bool HasItems => _availableItems.Count > 0;
    #endregion

    #region Unity Lifecycle Methods
    private void Start()
    {
        InitializeItemsList();
        EnableInitialItem();
    }

    private void Update()
    {
        HandleItemSwitching();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Adds a new item to the available items list.
    /// </summary>
    /// <param name="item">The item to add</param>
    public void AddItem(GameObject item)
    {
        if (item != null && !_availableItems.Contains(item))
        {
            _availableItems.Add(item);
            item.SetActive(false);

            // If this is our first item, select it
            if (_availableItems.Count == 1)
            {
                SwitchToItem(0);
            }
        }
    }

    /// <summary>
    /// Removes an item from the available items list.
    /// </summary>
    /// <param name="item">The item to remove</param>
    public void RemoveItem(GameObject item)
    {
        int index = _availableItems.IndexOf(item);
        if (index != -1)
        {
            _availableItems.RemoveAt(index);

            // Adjust current index if necessary
            if (_currentItemIndex >= _availableItems.Count)
            {
                _currentItemIndex = _availableItems.Count - 1;
            }

            // Switch to next available item
            if (_availableItems.Count > 0)
            {
                SwitchToItem(_currentItemIndex);
            }
        }
    }

    /// <summary>
    /// Forces a refresh of the detection system. 
    /// This method is kept for backward compatibility but is no longer needed
    /// since the system adapts to current player head orientation automatically.
    /// </summary>
    public void RecalibrateOrientation()
    {
        if (_logSwitchEvents)
        {
            Debug.Log("ItemSwitcher: Recalibration called (automatic head-based adaptation enabled)");
        }
    }
    #endregion

    #region Private Methods
    private void InitializeItemsList()
    {
        _availableItems.Clear();
        foreach (Transform child in transform)
        {
            _availableItems.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }
    }

    private void EnableInitialItem()
    {
        if (_availableItems.Count > 0)
        {
            SwitchToItem(0);
        }
    }

    private void HandleItemSwitching()
    {
        if (!HasItems || !_headTransform) return;

        bool isReachingBehind = IsReachingBehind();

        // Handle dwell time logic
        if (isReachingBehind && !_wasReachingBehind)
        {
            // Hand just entered the trigger zone
            _dwellStartTime = Time.time;
        }
        else if (!isReachingBehind && _wasReachingBehind)
        {
            // Hand left the trigger zone
            _dwellStartTime = 0f;
        }

        // Check if hand has been in position long enough and we can switch
        if (isReachingBehind && _wasReachingBehind && CanSwitch() && HasDwelledLongEnough())
        {
            SwitchToNextItem();
            _lastSwitchTime = Time.time;
            _dwellStartTime = 0f; // Reset dwell time after switch
        }

        _wasReachingBehind = isReachingBehind;
    }

    private bool IsReachingBehind()
    {
        Vector3 handPosition = transform.position;
        Vector3 headPosition = _headTransform.position;

        // Get the current forward direction of the head
        Vector3 currentHeadForward = _headTransform.forward;

        // Project everything onto horizontal plane to ignore pitch
        Vector3 handHorizontal = Vector3.ProjectOnPlane(handPosition, Vector3.up);
        Vector3 headHorizontal = Vector3.ProjectOnPlane(headPosition, Vector3.up);
        Vector3 forwardHorizontal = Vector3.ProjectOnPlane(currentHeadForward, Vector3.up).normalized;

        // Calculate offset from torso center
        Vector3 horizontalOffset = handHorizontal - headHorizontal;
        float horizontalDistance = horizontalOffset.magnitude;

        // Check if hand is behind the player's current facing direction
        float forwardDotProduct = Vector3.Dot(forwardHorizontal, horizontalOffset.normalized);

        // Check vertical constraint - hand should be roughly at torso/back level
        float verticalOffset = Mathf.Abs(handPosition.y - headPosition.y);

        // All conditions must be met:
        // 1. Hand is within reasonable distance of torso
        // 2. Hand is behind the player (negative dot product with current forward)
        // 3. Hand is far enough behind to trigger
        // 4. Hand is at appropriate height level
        bool isWithinRange = horizontalDistance <= _maxDistanceToTorso;
        bool isBehindPlayer = forwardDotProduct < -0.3f; // More restrictive threshold for behind
        bool isFarEnoughBehind = horizontalDistance >= _behindBackDistance;
        bool isAtCorrectHeight = verticalOffset <= _maxVerticalOffset;

        return isWithinRange && isBehindPlayer && isFarEnoughBehind && isAtCorrectHeight;
    }

    private bool CanSwitch()
    {
        return Time.time - _lastSwitchTime >= _switchCooldown;
    }

    private bool HasDwelledLongEnough()
    {
        return _dwellStartTime > 0f && (Time.time - _dwellStartTime) >= _dwellTime;
    }

    private void SwitchToNextItem()
    {
        if (_availableItems.Count == 0) return;

        int nextIndex = (_currentItemIndex + 1) % _availableItems.Count;

        if (_logSwitchEvents)
        {
            Debug.Log($"ItemSwitcher: Switching from item {_currentItemIndex} to {nextIndex}");
        }

        SwitchToItem(nextIndex);
    }

    private void SwitchToItem(int index)
    {
        // Disable current item
        if (_currentItemIndex >= 0 && _currentItemIndex < _availableItems.Count)
        {
            _availableItems[_currentItemIndex].SetActive(false);
        }

        // Enable new item
        _currentItemIndex = index;
        if (_currentItemIndex >= 0 && _currentItemIndex < _availableItems.Count)
        {
            _availableItems[_currentItemIndex].SetActive(true);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_showDebugGizmos || !_headTransform) return;

        Vector3 handPosition = transform.position;
        Vector3 headPosition = _headTransform.position;

        // Draw hand position
        Gizmos.color = IsReachingBehind() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(handPosition, 0.05f);

        // Draw line from head to hand
        Gizmos.color = Color.white;
        Gizmos.DrawLine(headPosition, handPosition);

        // Get current forward direction
        Vector3 currentForward = _headTransform.forward;

        // Draw trigger zone behind player using current forward direction
        Gizmos.color = Color.yellow;
        Vector3 behindPosition = headPosition - currentForward * _behindBackDistance;
        Gizmos.DrawWireSphere(behindPosition, _maxDistanceToTorso);

        // Draw height constraint visualization
        Gizmos.color = Color.blue;
        Vector3 topBound = headPosition + Vector3.up * _maxVerticalOffset;
        Vector3 bottomBound = headPosition - Vector3.up * _maxVerticalOffset;
        Gizmos.DrawLine(topBound, bottomBound);

        // Draw horizontal plane projection
        Vector3 handHorizontal = Vector3.ProjectOnPlane(handPosition, Vector3.up);
        Vector3 headHorizontal = Vector3.ProjectOnPlane(headPosition, Vector3.up);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(headHorizontal, handHorizontal);

        // Draw current forward direction for reference
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(headPosition, currentForward * 0.3f);

        // Draw back direction
        Gizmos.color = Color.red;
        Gizmos.DrawRay(headPosition, -currentForward * 0.3f);

        // Draw dwell progress if hand is in trigger zone
        if (IsReachingBehind() && _dwellStartTime > 0f)
        {
            float dwellProgress = Mathf.Clamp01((Time.time - _dwellStartTime) / _dwellTime);
            Gizmos.color = Color.Lerp(Color.yellow, Color.green, dwellProgress);
            Gizmos.DrawWireSphere(handPosition, 0.1f * dwellProgress);
        }
    }
#endif
    #endregion
}
