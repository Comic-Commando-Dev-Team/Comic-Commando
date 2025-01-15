using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class ReycastGun : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _ReyCastSound;

    [Header("Projectile")]
    [SerializeField] private LineRenderer _ReyCastPrefab;
    [SerializeField] private Transform _barrelLocation;
    [SerializeField] private float _laserLifetime = 5f;

    [SerializeField] private float maxLineDistance = 5f;

    [Header("Gun Settings")]
    [SerializeField] private float _fireRate = 0.05f;
    private float _nextFireTime;

    [Header("Input")]
    [SerializeField] private InputActionProperty _shootAction;
    [SerializeField] private XRNode _controllerNode = XRNode.LeftHand;


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
         if (_audioSource != null && _ReyCastSound != null)
         {
             _audioSource.PlayOneShot(_ReyCastSound);
         }

         LineRenderer line = Instantiate(_ReyCastPrefab);
         line.positionCount = 2;
         line.SetPosition(0, _barrelLocation.position);

         Vector3 endPosition = _barrelLocation.position + _barrelLocation.forward * maxLineDistance;

         line.SetPosition(1, endPosition);

         Destroy(line.gameObject, _laserLifetime);

    }
}
