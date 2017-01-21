using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelZoom : MonoBehaviour {

    public float zoomSpeed = 1f;
    public float maxSize = 15f;
    public float minSize = 2.5f;
    public float controllerSpeed = 0.03f;

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update () {
        float wheel = Input.GetAxis("Mouse ScrollWheel");

        if(Input.GetKey(KeyCode.Joystick1Button4))
        {
            wheel -= controllerSpeed;
        }

        if (Input.GetKey(KeyCode.Joystick1Button5))
        {
            wheel += controllerSpeed;
        }

        _cam.orthographicSize = Mathf.Min(maxSize, Mathf.Max(minSize, _cam.orthographicSize + wheel * zoomSpeed));
	}
}
