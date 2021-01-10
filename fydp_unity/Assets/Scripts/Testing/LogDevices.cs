using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class LogDevices : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);

        foreach (var item in devices)
        {
            Debug.Log(item.name + item.characteristics);
        }
        Debug.Log("Done");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
