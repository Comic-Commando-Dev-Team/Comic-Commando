using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class Gun : MonoBehaviour 
{
    public AudioSource source;
    public AudioClip laserSound;
    public GameObject laser;
    public Transform barrelLocation;

    private float fireRate = 0.05f;
    private float nextFire = 0.0f;

    public float shootPower;
    private bool triggerDown = false;

    private ActionBasedController controller;

    void Start()
    {
        if (barrelLocation == null)
            barrelLocation = transform;

        // Get the ActionBasedController (assuming it's on the same GameObject or can be assigned)
        controller = GetComponent<ActionBasedController>();
    }

    void Update()
    {
        CheckTrigger();
        Shoot();
    }

    private void CheckTrigger()
    {
        if (controller != null && controller.selectAction.action != null)
        {
            triggerDown = controller.selectAction.action.ReadValue<float>() > 0.1f;
        }
    }

    private void Shoot()
    {
        if (triggerDown && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;

            Debug.Log("fire");
            // Tworzenie lasera
            GameObject instantiatedLaser = Instantiate(laser, barrelLocation.position, barrelLocation.rotation * Quaternion.Euler(90f, 0f, 0f));
            Rigidbody rb = instantiatedLaser.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(barrelLocation.forward * shootPower);
                Debug.Log("laser");
            }

            // Odtwarzanie dźwięku
            if (source != null && laserSound != null)
            {
                source.PlayOneShot(laserSound);
            }

            // Haptics (jeśli chcesz dodać integrację)
            if (controller != null && controller.SendHapticImpulse(0.5f, 0.1f))
            {
                Debug.Log("Haptic feedback sent.");
            }
        }
    }
}
