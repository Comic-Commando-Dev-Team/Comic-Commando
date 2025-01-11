using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class Gun : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _laserSound;

    [Header("Projectile")]
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private Transform _barrelLocation;
    [SerializeField] private float _shootPower = 100f;
    [SerializeField] private float _projectileLifetime = 5f;

    [Header("Gun Settings")]
    [SerializeField] private float _fireRate = 0.05f;
    private float _nextFireTime;

    [Header("Input")]
    [SerializeField] private InputActionProperty _shootAction;
    [SerializeField] private XRNode _controllerNode = XRNode.RightHand;

    // Properties for external access if needed
    public float ShootPower
    {
        get => _shootPower;
        set => _shootPower = value;
    }

    public float FireRate
    {
        get => _fireRate;
        set => _fireRate = Mathf.Max(0.01f, value);
    }

    private void OnEnable()
    {
        _shootAction.action?.Enable();
    }

    private void OnDisable()
    {
        _shootAction.action?.Disable();
    }

    private void Update()
    {
        if (_shootAction.action?.triggered == true && Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + _fireRate;
            Shoot();
        }
    }

    private void Shoot()
    {
        // Play sound
        if (_audioSource != null && _laserSound != null)
        {
            _audioSource.PlayOneShot(_laserSound);
        }

        // Spawn projectile
        if (_laserPrefab != null && _barrelLocation != null)
        {
            GameObject spawnedLaser = Instantiate(_laserPrefab, _barrelLocation.position, _barrelLocation.rotation);
            if (spawnedLaser.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.AddForce(_barrelLocation.forward * _shootPower);
            }

            Destroy(spawnedLaser, _projectileLifetime);
        }

        SendHapticImpulse(0.5f, 0.2f);
    }

    private void SendHapticImpulse(float amplitude, float duration)
    {
        UnityEngine.XR.InputDevice device = InputDevices.GetDeviceAtXRNode(_controllerNode);
        if (device.isValid)
        {
            device.SendHapticImpulse(0, amplitude, duration);
        }
    }
}