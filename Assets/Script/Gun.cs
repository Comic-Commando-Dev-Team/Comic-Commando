using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class Gun : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource source;
    public AudioClip laserSound;

    [Header("Projectile")]
    public GameObject laser;
    public Transform barrelLocation;
    public float shootPower = 100f;
    public float projectileLifetime = 5f;

    [Header("Gun Settings")]
    public float fireRate = 0.05f;
    private float nextFire = 0.0f;

    [Header("Input")]
    public InputActionProperty shootAction;
    public XRNode controllerNode = XRNode.RightHand;

    private void OnEnable()
    {
        shootAction.action?.Enable();
    }

    private void OnDisable()
    {
        shootAction.action?.Disable();
    }

    private void Update()
    {
        if (shootAction.action?.triggered == true && Time.time >= nextFire)
        {
            nextFire = Time.time + fireRate;
            Shoot();
        }
    }

    private void Shoot()
    {
        // Play sound
        if (source != null && laserSound != null)
        {
            source.PlayOneShot(laserSound);
        }

        // Spawn projectile
        if (laser != null && barrelLocation != null)
        {
            GameObject spawnedLaser = Instantiate(laser, barrelLocation.position, barrelLocation.rotation);
            if (spawnedLaser.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.AddForce(barrelLocation.forward * shootPower);
            }

            // Destroy projectile after lifetime
            Destroy(spawnedLaser, projectileLifetime);
        }

        // Send haptic feedback
        SendHapticImpulse(0.5f, 0.2f);
    }

    private void SendHapticImpulse(float amplitude, float duration)
    {
        UnityEngine.XR.InputDevice device = InputDevices.GetDeviceAtXRNode(controllerNode);
        if (device.isValid)
        {
            device.SendHapticImpulse(0, amplitude, duration);
        }
    }
}