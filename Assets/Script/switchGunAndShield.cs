using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class switchGunAndShield : MonoBehaviour {
    public GameObject head;
    public bool rightController;
    public bool leftController;
    private float switchingRate = 1f;
    private float nextSwitch = 1.0f;

    void Start()
    {

    }

    void Update()
    {
        CheckIfSwitch();
    }
    
    private void CheckIfSwitch()
    {
        Vector3 controllerPos = Vector3.zero;

        if (rightController)
        {
            controllerPos = InputTracking.GetLocalPosition(XRNode.RightHand);
        }
        else if (leftController)
        {
            controllerPos = InputTracking.GetLocalPosition(XRNode.LeftHand);
        }

        if (Vector3.Angle(transform.up, Vector3.up) > 80 && 
            controllerPos.y > head.transform.position.y - 0.4f && 
            controllerPos.x < head.transform.position.x && 
            Time.time > nextSwitch)
        {
            nextSwitch = Time.time + switchingRate;
            var Gun = this.gameObject.transform.GetChild(0);
            var shield = this.gameObject.transform.GetChild(1);
            if (Gun.gameObject.activeSelf == true)
            {
                Gun.gameObject.SetActive(false);
                shield.gameObject.SetActive(true);
            }
            else if (Gun.gameObject.activeSelf == false)
            {
                shield.gameObject.SetActive(false);
                Gun.gameObject.SetActive(true);
            }
        }
    }
}
