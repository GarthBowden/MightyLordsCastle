using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This button method rotates the player world view
public class CompassClick : ButtonClick
{
    
    public override void OnClick()
    {
        CameraControl cameraControl = Camera.main.gameObject.GetComponent<CameraControl>();
        if(cameraControl)
        {
            cameraControl.RotateViewCW();
            gameObject.transform.RotateAround(gameObject.transform.position, Vector3.forward, 90);
        }
        else
        {
            Debug.Log("Camera Control not found!");
        }
    }
}
