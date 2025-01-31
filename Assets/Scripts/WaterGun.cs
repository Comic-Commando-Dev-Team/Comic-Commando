using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.VFX;

public class WaterGun : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _WaterSound;

    [Header("Input")]
    [SerializeField] private InputActionProperty _shootAction;
    [SerializeField] private XRNode _controllerNode = XRNode.LeftHand;

    [Header("Projectile")]
    [SerializeField] private int _damage = 1;
    [SerializeField] private VisualEffect _waterVisualEffect;
    [SerializeField] public Transform _barrelLocation;


    private RaycastHit hit;

    private float fireRate = 1f / 3f; // 3 razy na sekundę
    private float nextFireTime = 0f;
    private int hitCount = 0;

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
                nextFireTime = Time.time + fireRate;

                if (Physics.Raycast(_barrelLocation.position, transform.TransformDirection(Vector3.forward), out hit, 4))
                {
                    Debug.DrawRay(_barrelLocation.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
                    Debug.Log("Did Hit");

                    if (hit.collider.TryGetComponent(out HealthController healthController))
                    {
                        healthController.Damage(_damage);
                    }
                }
            }
        }

        if (_shootAction.action?.ReadValue<float>() == 0)
        {
            StopShooting();
        }
    }

    private void Shoot()
    {
        // Play sound
        if (_audioSource != null && _WaterSound != null)
        {
            _audioSource.PlayOneShot(_WaterSound);
        }

        // Enable the visual effect
        if (_waterVisualEffect != null)
        {
            _waterVisualEffect.SetFloat("SpawnRate", 15000f);
        }
    }

    private void StopShooting()
    {
        // Disable the visual effect
        if (_waterVisualEffect != null)
        {
            _waterVisualEffect.SetFloat("SpawnRate", 0f);
        }

        // Stop the audio source
        if (_audioSource != null)
        {
            _audioSource.Stop();
        }
    }
}