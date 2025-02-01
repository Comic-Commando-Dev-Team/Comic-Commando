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
    [SerializeField]
    [Tooltip("The distance behind the head to trigger the switch")]
    private float _behindBackThreshold = 0.15f;
    [SerializeField]
    [Tooltip("The maximum distance between the hand and head to trigger the switch")]
    private float _maxDistanceToHead = 0.5f;
    [SerializeField] private float _switchCooldown = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool _showDebugGizmos;

    private List<GameObject> _availableItems = new List<GameObject>();
    private int _currentItemIndex = -1;
    private float _lastSwitchTime;
    private bool _wasReachingBehind;

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

        // Detect the moment when the hand goes behind back
        if (isReachingBehind && !_wasReachingBehind && CanSwitch())
        {
            SwitchToNextItem();
            _lastSwitchTime = Time.time;
        }

        _wasReachingBehind = isReachingBehind;
    }

    private bool IsReachingBehind()
    {
        Vector3 handPosition = transform.position;
        Vector3 headPosition = _headTransform.position;
        Vector3 headForward = _headTransform.forward;

        // Calculate if hand is behind head
        Vector3 handToHead = handPosition - headPosition;
        float dotProduct = Vector3.Dot(headForward, handToHead.normalized);

        return handToHead.magnitude <= _maxDistanceToHead && dotProduct < -_behindBackThreshold;
    }

    private bool CanSwitch()
    {
        return Time.time - _lastSwitchTime >= _switchCooldown;
    }

    private void SwitchToNextItem()
    {
        if (_availableItems.Count == 0) return;

        int nextIndex = (_currentItemIndex + 1) % _availableItems.Count;
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

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.05f);
        Gizmos.DrawLine(_headTransform.position, transform.position);
    }
#endif
    #endregion
}
