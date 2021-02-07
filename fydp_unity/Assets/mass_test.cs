using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class mass_test : MonoBehaviour
{

    private XRGrabInteractable interactor;
    private Rigidbody rb;

    private void printMass(XRBaseInteractor interactable)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Debug.Log("Object Grabbed! Here's the mass: " + rb.mass);
    }


    // Start is called before the first frame update
    void Start()
    {
        interactor = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (interactor != null && interactor.isSelected)
        {
            //Debug.Log("Object Grabbed! Here's the mass: " + rb.mass);
        }
        else
        {
            //Debug.Log("The object is not grabbed");
        }
    }
}
