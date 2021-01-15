using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class PingArduino : MonoBehaviour
{
    public string portName = "/dev/ttyACM0";
    SerialPort arduino;
    // Start is called before the first frame update
    void Start()
    {
        arduino = new SerialPort(portName, 9600);
        arduino.Open();
    }

    // Update is called once per frame
    void Update()
    {
        if (arduino.IsOpen){
            if(Input.GetKey("1")){
                arduino.Write("1");
                arduino.BaseStream.Flush();
                Debug.Log(1);
            }
        }
    }
}
