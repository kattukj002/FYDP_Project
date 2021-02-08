using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using FYDP.ArmBrace;

public class EchoFramesFromUnity : MonoBehaviour
{
    public string portName = "/dev/ttyACM0";
    private SerialPort arduino;
    private ArmCmd armCmd;
    
    void Start() {
        arduino = new SerialPort(portName, 9600);
        arduino.WriteTimeout = SerialPort.InfiniteTimeout;
        arduino.Open();

        armCmd = new ArmCmd(arduino);
        
        armCmd.elbow.SetTorque(1f);
        armCmd.shoulderAbduction.SetTorque(2f);
        armCmd.shoulderFlexion.SetTorque(3f);
        //StartCoroutine(SpacedWrite());   
    }

    void Update() {
        
    }
    public IEnumerator SpacedWrite() {
        int i = 0;
        while (true){
            Debug.Log("Sent " + i.ToString());
            i+=1;
            yield return null;
        }
        //armCmd.Send();
        //yield return new WaitForSeconds(waitSeconds);
    }
    
}
