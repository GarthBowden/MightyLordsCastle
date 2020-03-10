using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used as a passthrough to the click functionality of this gameObject
//This allows other components active on this gameObject to remain hidden from the UI / Cursor script
//Existence of this component is usable as a test for the clickability of the gameObject
public class ClickableWorldObject : MonoBehaviour
{

    //This will return the origin of the gameObject + offset, not the click location
    //For situations where the location of the gameObject matters, such as picking up or putting down units
    [SerializeField] private Vector3 ClickRegisterOffset;
    public Vector3 GetClickRegisterWorld()
    {
        return ClickRegisterOffset + gameObject.transform.position;
    }

    //This delegate should be set by other attached components, which will implement logic of clicking on the gameObject
    public Action clickAction;

    //Calls the delegate set by other scripts. The UI / Cursor script should call THIS, not the other scripts directly!
    public void OnClick()
    {
        //Sanity check
        if (clickAction == null || clickAction.GetInvocationList().Length < 1)
        {
            Debug.Log("On Click called for " + gameObject.name + " without any delegates assigned!");
            return;
        }
        clickAction();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

}
