using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    //Enforce itself as a singleton
    public static CameraControl Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("Duplicate " + this + " detected. Deleting myself!");
            Destroy(this);
        }
    }

    //Camera Rotation. Center Axis is the camera's focus point in the scene
    public Vector3 CameraCenterAxis;
    public void RotateViewCW()
    {
        gameObject.transform.RotateAround(CameraCenterAxis, Vector3.up, 90);
    }

}
