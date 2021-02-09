using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using FYDP.ArmBrace;

public class EchoFramesFromUnity : MonoBehaviour
{
    public string portName = "/dev/ttyACM0";
    private SerialPort arduino;
    private BraceCmd armCmd;
    
    void Start() {
        arduino = new SerialPort(portName, 9600);
        arduino.WriteTimeout = SerialPort.InfiniteTimeout;
        arduino.Open();

        armCmd = new BraceCmd(arduino);
        
        armCmd.elbow.SetTorque(1f);
        armCmd.shoulderAbduction.SetTorque(2f);
        armCmd.shoulderFlexion.SetTorque(3f);
        StartCoroutine(SpacedWrite(1));   
    }

    void Update() {
        
    }
    public IEnumerator SpacedWrite(int waitSeconds) {
        int i = 0;
        while (true){
            armCmd.Send();
            Debug.Log(i);
            i+=1;
            yield return new WaitForSeconds(waitSeconds);;
        }
    }
    
}
