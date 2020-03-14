using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeyboardMouseInput : MonoBehaviour
{
    //Enforce itself as a singleton
    public static KeyboardMouseInput Instance { get; private set; }
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    //Readability function for checking if the cursor is in the game window
    private bool IsMouseCursorInWindow()
    {
        if (Input.mousePosition.x >= 0 && Input.mousePosition.x < Screen.width && Input.mousePosition.y >= 0 && Input.mousePosition.y < Screen.height)
        {
            return true;
        }
        else
        {
            Debug.Log("Mouse outside window!");
            return false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //User click on something
        if(Input.GetButtonDown("Fire1") && IsMouseCursorInWindow() && !EventSystem.current.IsPointerOverGameObject())
        {
            //TODO if not UI click...
            //ELSE... world space hit
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                ClickableWorldObject ClickedObject = hit.collider.gameObject.GetComponent<ClickableWorldObject>();
                if(ClickedObject)
                {
                    ClickedObject.OnClick();
                }
            }
        }
    }
}
