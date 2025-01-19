using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class ReycastGun : MonoBehaviour
{
    [SerializeField] private InputActionProperty _shootAction;
    [SerializeField] private XRNode _controllerNode = XRNode.LeftHand;
    [SerializeField] private Transform _barrelLocation;
    [SerializeField] private LineRenderer _ReyCastPrefab;
    [SerializeField] private AudioClip _ReyCastSound;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float _fireRate = 0.1f;
    [SerializeField] private float _laserLifetime = 0.2f;
    [SerializeField] private float maxLineDistance = 100f;

    private float _nextFireTime;

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
        if (_shootAction.action?.ReadValue<float>() > 0 && Time.time >= _nextFireTime)
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