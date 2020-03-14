using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonClick : MonoBehaviour
{
    // Start is called before the first frame update
    // Derived classes MUST call this base class if overriding start
    protected virtual void Start()
    {
        UnityEngine.UI.Button thisButton = GetComponent<UnityEngine.UI.Button>();
        if (thisButton)
        {
            thisButton.onClick.AddListener(OnClick);
        }
        else
        {
            Debug.Log("ButtonClick derived components can only be attached to buttons!");
        }
    }

    public virtual void OnClick()
    {
        Debug.Log("ButtonClick base implementation of OnClick called!");
    }
}
