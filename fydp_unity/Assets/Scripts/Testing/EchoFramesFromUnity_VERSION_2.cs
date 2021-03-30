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
    [SerializeField]
    private string ArduinoPortName = "COM4";
    [SerializeField]
    private int ArduinoBaudRate = 115200;
    private SerialPort arduino;
    private BraceCmd armCmd;
    private Thread sendThread;
    private bool quitThread = false;

    void Start() {
        arduino = new SerialPort(ArduinoPortName, ArduinoBaudRate);
        if (!arduino.IsOpen){
            arduino.Open();
        }
        
        DateTime startime =  DateTime.Now;
        TimeSpan dur = TimeSpan.FromMilliseconds(3000);
        while (DateTime.Now - startime < dur) {}
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
                if (arduino != null && arduino.IsOpen) {
                    arduino.Close();
                }       
            }
        };
    }

    ~EchoFramesFromUnity_VERSION_2() {
        if (arduino != null && arduino.IsOpen) {
            arduino.Close();
        }
    }
    //Changed to thread b/c of Oculus libraries stopping program with exceptions otherwise.
    void Update() {
        armCmd.Send();
    }
}
