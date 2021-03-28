using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using FYDP.ArmBrace;
using System.Threading; 
using UnityEditor;

public class EchoFramesFromArduino : MonoBehaviour
{
    // private BraceSensorReader sensorReader;
    // SerialPort arduinoPort;
    // public string PortName = "COM4";
    // public int BaudRate = 115200;
    // private Thread printThread; 
    // private bool quitPrints = false;
    // // Start is called before the first frame update
    // void Start()
    // {
    //     arduinoPort = new SerialPort(PortName, BaudRate);
    //     arduinoPort.ReadTimeout = 10;
    //     arduinoPort.WriteTimeout = 1;
    //     arduinoPort.ReadTimeout = 1;
    //     arduinoPort.ReadBufferSize = 24;
    //     arduinoPort.WriteBufferSize = 24;

    //     sensorReader = new BraceSensorReader(arduinoPort);
    //     sensorReader.StartAsyncSensorReads();
    //     printThread = new Thread(this.PrintSensorData);
    //     printThread.Start();
    //     EditorApplication.playModeStateChanged += (PlayModeStateChange state) => {
    //         if(state == PlayModeStateChange.ExitingPlayMode){
    //             this.EndThreads();
    //         }
    //     };
    // }
    // void OnApplicationQuit() {
    //     EndThreads();
    // }
    // void EndThreads() {
    //     quitPrints = true;
    //     sensorReader.StopAsyncSensorReads();
    //     printThread.Join();
    // }
    // void PrintSensorData() {
    //     while(!quitPrints) {
    //         bool successfulRead = sensorReader.GetJointAngles(
    //             out float elbowAngleDeg, out float shoulderAbductionDeg, 
    //             out float shoulderFlexionDeg);
    //         Debug.Log("Successful Read: " + successfulRead.ToString() + 
    //                 ", elbow angle: " + elbowAngleDeg.ToString() + " deg, "  + 
    //                 "shoulder abduction angle: " + 
    //                 shoulderAbductionDeg.ToString() + " deg, shoulder flexion " + 
    //                 "angle: " + shoulderFlexionDeg + " deg");
    //     }
    // }
}
