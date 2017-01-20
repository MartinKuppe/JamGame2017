using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelZoom : MonoBehaviour {

    public float zoomSpeed = 1f;
    public float maxSize = 15f;
    public float minSize = 2.5f;

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update () {
        float wheel = Input.GetAxis("Mouse ScrollWheel");

        _cam.orthographicSize = Mathf.Min(maxSize, Mathf.Max(minSize, _cam.orthographicSize + wheel * zoomSpeed));
	}
}
