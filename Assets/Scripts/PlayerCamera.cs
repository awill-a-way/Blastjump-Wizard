using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CameraInput
{
    public Vector2 Look;
}

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float horizontalSensitivity = 0.1f;
    [SerializeField] private float verticalSensitivity = 0.1f;
    private Vector3 _eulerAngles;
    public void Initialize(Transform target)
    {
        transform.position = target.position;
        transform.eulerAngles = _eulerAngles = target.eulerAngles;
    }

    public void UpdateRotation(CameraInput input)
    {
        _eulerAngles += new Vector3(-input.Look.y*verticalSensitivity, input.Look.x*horizontalSensitivity);
        _eulerAngles.x = Mathf.Clamp(_eulerAngles.x, -89.9f, 89.9f);
        transform.eulerAngles = _eulerAngles; 
    }

    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }
}


