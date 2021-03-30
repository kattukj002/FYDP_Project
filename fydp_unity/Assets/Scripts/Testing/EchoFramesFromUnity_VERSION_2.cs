using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading; 
using FYDP.ArmBrace;
using UnityEditor;
using System;

public class EchoFramesFromUnity_VERSION_2 : MonoBehaviour
{
    public string PortName = "COM4";
    public int BaudRate = 115200; 
    private SerialPort arduino;
    private BraceCmd armCmd;
    private Thread sendThread;
    private bool quitThread = false;

    void Start() {
        arduino = new SerialPort(PortName, BaudRate);
        arduino.Open();

        armCmd = new BraceCmd(
                arduino, 
                elbow_:new MotorCmdFormat(
                    torqueRatingNm:1.2f, torqueCmdFullScale:89, gearRatio:5, 
                    stictionEncodedTorque:8),
                shoulderDown_:new MotorCmdFormat(
                    torqueRatingNm:1.89f, torqueCmdFullScale:89, gearRatio:5, 
                    stictionEncodedTorque:8)
                );
        
        armCmd.elbow.SetTorqueMove(1f);
        armCmd.shoulderDown.SetTorqueMove(2f);

        EditorApplication.playModeStateChanged += (PlayModeStateChange state) => {
            if(state == PlayModeStateChange.ExitingPlayMode){
                if (arduino.IsOpen) {
                    arduino.Close();
                }
                
            }
        };
    }

    void OnApplicationQuit() {
        if (arduino.IsOpen) {
                    arduino.Close();
                }
    }

    ~EchoFramesFromUnity_VERSION_2() {
        if (arduino.IsOpen) {
                    arduino.Close();
                }
    }
    //Changed to thread b/c of Oculus libraries stopping program with exceptions otherwise.
    void Update() {
        armCmd.Send();
    }
}
