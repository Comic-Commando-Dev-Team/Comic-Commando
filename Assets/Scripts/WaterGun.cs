using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.VFX;

public class WaterGun : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _waterStartSound;
    [SerializeField] private AudioClip _waterLoopSound;
    [SerializeField] private AudioClip _waterStopSound;

    [Header("Input")]
    [SerializeField] private InputActionProperty _shootAction;
    [SerializeField] private XRNode _controllerNode = XRNode.RightHand;

    [Header("Projectile")]
    [SerializeField] private int _maxDamage = 34;
    [SerializeField] private int _minDamage = 10;
    [SerializeField] private float _maxRange = 6f;
    [SerializeField] private VisualEffect _waterVisualEffect;
    [SerializeField] public Transform _barrelLocation;

    [Header("Visual Effects")]
    [SerializeField] private LineRenderer _rangeIndicator;
    [SerializeField] private ParticleSystem _splashEffect;
    [SerializeField] private LayerMask _targetLayers = -1;

    [Header("Gun Settings")]
    [SerializeField] private float _fireRate = 3f; // Shots per second
    [SerializeField] private bool _continuousMode = true;
    [SerializeField] private float _accuracySpread = 0.1f; // Degrees of random spread


    private RaycastHit hit;

    private float nextFireTime = 0f;
    private bool _isShooting = false;

    private void Start()
    {
        // Ensure range indicator is initially disabled
        if (_rangeIndicator != null)
        {
            _rangeIndicator.enabled = false;
        }
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
        if (_shootAction.action?.triggered == true)
        {
            Shoot();
        }

        if (_shootAction.action?.ReadValue<float>() > 0)
        {
            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + (1f / _fireRate);

                Vector3 rayDirection = transform.TransformDirection(Vector3.forward);

                // Add accuracy spread for more realistic water stream
                if (_accuracySpread > 0f)
                {
                    Vector3 spread = Random.insideUnitSphere * _accuracySpread;
                    rayDirection = (rayDirection + spread).normalized;
                }

                if (Physics.Raycast(_barrelLocation.position, rayDirection, out hit, _maxRange, _targetLayers))
                {
                    Debug.DrawRay(_barrelLocation.position, rayDirection * hit.distance, Color.blue, 0.1f);

                    // Handle enemy damage
                    if (hit.collider.TryGetComponent(out HealthController healthController))
                    {
                        // Calculate damage based on distance
                        float damageMultiplier = 1f - (hit.distance / _maxRange);
                        int calculatedDamage = Mathf.RoundToInt(Mathf.Lerp(_minDamage, _maxDamage, damageMultiplier));
                        healthController.Damage(calculatedDamage);

                        // Create splash effect at hit point
                        CreateSplashEffect(hit.point, hit.normal);
                    }
                }
                else
                {
                    // Draw full range when no hit
                    Debug.DrawRay(_barrelLocation.position, rayDirection * _maxRange, Color.cyan, 0.1f);
                }

                // Update range indicator
                UpdateRangeIndicator(rayDirection);
            }
        }

        if (_shootAction.action?.ReadValue<float>() == 0 && _isShooting)
        {
            StopShooting();
        }
    }

    private void Shoot()
    {
        if (!_isShooting)
        {
            _isShooting = true;

            // Play start sound and transition to loop
            if (_audioSource != null)
            {
                if (_waterStartSound != null)
                {
                    _audioSource.PlayOneShot(_waterStartSound);
                }

                if (_waterLoopSound != null)
                {
                    _audioSource.clip = _waterLoopSound;
                    _audioSource.loop = true;
                    _audioSource.PlayDelayed(_waterStartSound != null ? _waterStartSound.length : 0f);
                }
            }
        }

        // Enable the visual effect
        if (_waterVisualEffect != null)
        {
            _waterVisualEffect.SetFloat("SpawnRate", 500f);
        }

        // Add haptic feedback
        SendHapticImpulse(0.3f, 0.1f);
    }

    private void StopShooting()
    {
        if (_isShooting)
        {
            _isShooting = false;

            if (_audioSource != null)
            {
                _audioSource.Stop();
                if (_waterStopSound != null)
                {
                    _audioSource.loop = false;
                    _audioSource.PlayOneShot(_waterStopSound);
                }
            }
        }

        // Disable the visual effect
        if (_waterVisualEffect != null)
        {
            _waterVisualEffect.SetFloat("SpawnRate", 0f);
        }
    }

    private void SendHapticImpulse(float amplitude, float duration)
    {
        UnityEngine.XR.InputDevice device = InputDevices.GetDeviceAtXRNode(_controllerNode);
        if (device.isValid)
        {
            device.SendHapticImpulse(0, amplitude, duration);
        }
    }

    private void CreateSplashEffect(Vector3 position, Vector3 normal)
    {
        if (_splashEffect != null)
        {
            // Position splash effect at hit point
            _splashEffect.transform.position = position;
            _splashEffect.transform.LookAt(position + normal);
            _splashEffect.Play();
        }
    }

    private void UpdateRangeIndicator(Vector3 direction)
    {
        if (_rangeIndicator != null)
        {
            _rangeIndicator.positionCount = 2;
            _rangeIndicator.SetPosition(0, _barrelLocation.position);
            _rangeIndicator.SetPosition(1, _barrelLocation.position + direction * _maxRange);

            // Make indicator visible only when shooting
            _rangeIndicator.enabled = _isShooting;
        }
    }
}