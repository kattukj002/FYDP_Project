using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using FYDP.ArmBrace;
using System.Threading; 
using UnityEditor;

public class EchoFramesFromArduino : MonoBehaviour
{
    private BraceSensorReader sensorReader;
    SerialPort arduinoPort;
    public string portName = "/dev/ttyACM0";
    private Thread printThread; 
    private bool quitPrints = false;
    // Start is called before the first frame update
    void Start()
    {
        arduinoPort = new SerialPort(portName, 9600);
        arduinoPort.ReadTimeout = 10;
        arduinoPort.WriteTimeout = 1;
        arduinoPort.ReadTimeout = 1;
        arduinoPort.ReadBufferSize = 8;
        arduinoPort.WriteBufferSize = 8;

        sensorReader = new BraceSensorReader(arduinoPort);
        sensorReader.StartAsyncSensorReads();
        printThread = new Thread(this.PrintSensorData);
        printThread.Start();
        EditorApplication.playmodeStateChanged = () => {
            if(EditorApplication.isPaused){
                sensorReader.StopAsyncSensorReads();
                this.EndThreads();
            }
        };
    }
    void OnApplicationQuit(){
        EndThreads();
    }
    void EndThreads() {
        quitPrints = true;
        printThread.Join();
    }
    void PrintSensorData(){
        while(!quitPrints) {
            bool successfulRead = sensorReader.GetJointAngles(
                out int elbowAngleDeg, out int shoulderAbductionDeg, 
                out int shoulderFlexionDeg);
            Debug.Log("Successful Read: " + successfulRead.ToString() + 
                    ", elbow angle: " + elbowAngleDeg.ToString() + " deg, "  + 
                    "shoulder abduction angle: " + 
                    shoulderAbductionDeg.ToString() + " deg, shoulder flexion " + 
                    "angle: " + shoulderFlexionDeg + " deg");
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey("1")){
            sensorReader.StopAsyncSensorReads();
        } else if (Input.GetKey("2")) {
            sensorReader.StartAsyncSensorReads();
        }
    }
}
