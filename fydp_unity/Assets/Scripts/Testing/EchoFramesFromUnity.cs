using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading; 
using FYDP.ArmBrace;
using UnityEditor;
using System;

public class EchoFramesFromUnity : MonoBehaviour
{
    public string PortName = "COM4";
    public int BaudRate = 115200; 
    private SerialPort arduino;
    private BraceCmd armCmd;
    private Thread sendThread;
    private bool quitThread = false;

    void Start() {
        arduino = new SerialPort(PortName, BaudRate);
        arduino.WriteTimeout = SerialPort.InfiniteTimeout;
        arduino.Open();

        armCmd = new BraceCmd(arduino);
        
        armCmd.elbow.SetTorque(1f);
        armCmd.shoulderAbduction.SetTorque(2f);
        armCmd.shoulderFlexion.SetTorque(3f);   
        
        EditorApplication.playModeStateChanged += (PlayModeStateChange state) => {
            if(state == PlayModeStateChange.ExitingPlayMode){
                this.EndThreads();
            }
        };
        sendThread = new Thread(this.TxThreadFcn);
        sendThread.Start();
    }

    void OnApplicationQuit() {
        EndThreads();
    }
    void EndThreads() {
        quitThread = true;
        sendThread.Join();
    }

    //Changed to thread b/c of Oculus libraries stopping program with exceptions otherwise.
    void TxThreadFcn() {
        DateTime startTime = DateTime.Now;
        TimeSpan interval = TimeSpan.FromMilliseconds(1000);

        while(!quitThread) {
            if ((DateTime.Now - startTime) >= interval) {
                Debug.Log("SENT");
                armCmd.Send();
                startTime = DateTime.Now;
            }
        }
        Debug.Log("QUIT");
    }
}
