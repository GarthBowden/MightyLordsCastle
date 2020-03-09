using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used as a passthrough to the click functionality of this gameObject
//This allows other components active on this gameObject to remain hidden from the UI / Cursor script
//Existence of this component is usable as a test for the clickability of the gameobject
public class ClickableWorldObject : MonoBehaviour
{

    //For situations where the location of the click matters, such as picking up or putting down units
    [SerializeField] private Vector3 ClickRegisterOffset;
    public Vector3 GetClickRegisterWorld()
    {
        return ClickRegisterOffset + gameObject.transform.position;
    }

    //This delegate should be set by other attached components, which will implement logic of clicking on the gameObject
    public delegate void ClickActionC();
    public ClickActionC clickAction;

    //Calls the delegate set by other scripts. The UI / Cursor script should call THIS, not the other scripts directly!
    public void OnClick()
    {
        //Sanity check
        if (clickAction.GetInvocationList().Length < 1)
        {
            Debug.Log("On Click called for " + gameObject.name + " without any delegates assigned!");
        }

        clickAction();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

}
