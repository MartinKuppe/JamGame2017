using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SwissArmyKnife;

public class XboxController : Singleton<XboxController> {

    private void Update()
    {
        // A
        if(Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            Debug.Log("Joystick Button 0");
        }
        // B
        if (Input.GetKeyDown(KeyCode.Joystick1Button1))
        {
            Debug.Log("Joystick Button 1");
        }
        // X
        if (Input.GetKeyDown(KeyCode.Joystick1Button2))
        {
            Debug.Log("Joystick Button 2");
        }
        // Y
        if (Input.GetKeyDown(KeyCode.Joystick1Button3))
        {
            Debug.Log("Joystick Button 3");
        }
        // LB
        if (Input.GetKeyDown(KeyCode.Joystick1Button4))
        {
            Debug.Log("Joystick Button 4");
        }
        // RB
        if (Input.GetKeyDown(KeyCode.Joystick1Button5))
        {
            Debug.Log("Joystick Button 5");
        }
        // Back
        if (Input.GetKeyDown(KeyCode.Joystick1Button6))
        {
            Debug.Log("Joystick Button 6");
        }
        // Start
        if (Input.GetKeyDown(KeyCode.Joystick1Button7))
        {
            Debug.Log("Joystick Button 7");
        }
        // RJoy Push
        if (Input.GetKeyDown(KeyCode.Joystick1Button8))
        {
            Debug.Log("Joystick Button 8");
        }
        // LJoy Push
        if (Input.GetKeyDown(KeyCode.Joystick1Button9))
        {
            Debug.Log("Joystick Button 9");
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button10))
        {
            Debug.Log("Joystick Button 10");
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button11))
        {
            Debug.Log("Joystick Button 11");
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button12))
        {
            Debug.Log("Joystick Button 12");
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button13))
        {
            Debug.Log("Joystick Button 13");
        }

        if (Input.GetAxis("PadHorizontal") != 0)
        {
            Debug.Log("Joystick PadHorizontal");
        }

        if (Input.GetAxis("PadVertical") != 0)
        {
            Debug.Log("Joystick PadVertical");
        }

        if (Input.GetAxis("Fire3") != 0)
        {
            Debug.Log("Joystick Fire3");
        }

        if (Input.GetAxis("Jump") != 0)
        {
            Debug.Log("Joystick Jump");
        }
    }
}
