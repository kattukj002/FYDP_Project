using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO.Ports;
using System;
using System.Threading;
using FYDP.ArmBrace;
using FYDP.VR;
using FYDP.Utils;
using UnityEditor;
using UnityEngine.UI;

public class SimulationForce : MonoBehaviour
{

    public Text txt;
    public bool display_values = true;

    [SerializeField]
    private float ArmMass = 1f;
    [SerializeField]
    private bool UseDummyInputs = false;
    [SerializeField]
    private bool PrintIntermediateValues = true;
    [SerializeField]
    private string ArduinoPortName = "COM4";
    [SerializeField]
    private int ArduinoBaudRate = 115200;
    [SerializeField]
    private int SerialWriteTimeout = 10;
    [SerializeField]
    private int SerialReadTimeout = 10;
    [SerializeField]
    private int SerialReadBufferSize = 24;
    [SerializeField]
    private int SerialWriteBufferSize = 8;
    [SerializeField]
    private int sensorDataRelevanceLifetimeMs = 500;
    [SerializeField]
    private float UpperArmLength = 0.3f;
    [SerializeField]
    private float LowerArmLength = 0.4f;
    [SerializeField]
    private float ShoulderDistFromNeckBase = 0.25f;
    [SerializeField]
    private Vector3 NeckBaseOffsetFromHeadset = new Vector3(0, -0.22f, 0);
    [SerializeField]
    private Vector3 CableMotorOffsetfromShoulder = new Vector3(0, -0.3f, 0);
    [SerializeField]
    public float CableWinchRadius = 0.07f;
    [SerializeField]
    private float ShoulderGearRatio = 1f;
    [SerializeField]
    private float MotorPowerFraction = 0.5f;
    [SerializeField]
    private bool RemoveHoldCommands = false;
    [SerializeField]
    private bool UseLeftControllerAsElbowTracker = false;
    [SerializeField]
    private bool IgnoreImu = false;
    [SerializeField]
    private float RightControllerVelocityThreshold = 0.2f;
    [SerializeField]
    private float ImuSensorMsgFreq = 0.005f;
    [SerializeField]
    private bool FinalTestDisable = false;

    // private class ArmMotionEstimators {
    //     public MotionEstimatorFloat ElbowDeg;
    //     public MotionEstimatorVector3 RightControllerPosition;

    //     public ArmMotionEstimators(float timestepSeconds) {
    //         ElbowDeg = new MotionEstimatorFloat(timestepSeconds);
    //         RightControllerPosition = new MotionEstimatorVector3(timestepSeconds);
    //     }
    //     public void EstimateUnobtainableNewPosition() {
    //         ElbowDeg.EstimateUnobtainableNewPosition();
    //         RightControllerPosition.EstimateUnobtainableNewPosition();
    //     }

    //     public void UpdateNewPosition(SensorData sensorData) {
    //         ElbowDeg.UpdateNewPosition(sensorData.ElbowDeg);
    //         RightControllerPosition.UpdateNewPosition(sensorData.RightControllerPosition);
    //     }

    //     public bool Filled(){
    //         return ElbowDeg.filled && RightControllerPosition.filled;
    //     }
    // }
    // private bool _started = false;
    // private Mutex _portMutex = new Mutex();

    private Thread sendThread;
    private bool quitThread = false;
    void Start()
    {
        // if(!FinalTestDisable) {
        //     XRDirectInteractor controllerInteractor = GetComponentInParent<XRDirectInteractor>();
        //     controllerInteractor.onSelectEntered.AddListener(GetHeldObjectMass);
        //     controllerInteractor.onSelectExited.AddListener(ZeroHeldObjectMass);
        // }
        
        // if(!UseDummyInputs) {
        
            // if(!_started) {
                _arduinoPort = new SerialPort(ArduinoPortName, ArduinoBaudRate);
                //Will need to look into the correct values for this.
                // _arduinoPort.WriteTimeout = SerialWriteTimeout;
                // _arduinoPort.ReadTimeout = SerialReadTimeout;
                // _arduinoPort.ReadBufferSize = SerialReadBufferSize;
                // _arduinoPort.WriteBufferSize = SerialWriteBufferSize;
                
                _arduinoPort.Open();
                    // _arduinoPort.DiscardInBuffer();
                    // _arduinoPort.DiscardOutBuffer();
                if (!_arduinoPort.IsOpen) {
                    throw new Exception("Unable to open port");
                }
                // DateTime startime =  DateTime.Now;
                // TimeSpan dur = TimeSpan.FromMilliseconds(3000);
                // while (DateTime.Now - startime < dur) {}
            //     _started = true;
            // }

            // if (_arduinoPort.BreakState){
            //     throw new Exception("Broken port!");
            // }

            _armCmd = new BraceCmd(
                _arduinoPort, 
                elbow_:new MotorCmdFormat(
                    torqueRatingNm:1.2f, torqueCmdFullScale:MotorPowerFraction * 89, gearRatio:5, 
                    stictionEncodedTorque:8),
                shoulderDown_:new MotorCmdFormat(
                    torqueRatingNm:1.89f, torqueCmdFullScale:MotorPowerFraction * 89, gearRatio:ShoulderGearRatio, 
                    stictionEncodedTorque:8)//, isCableMotor:true)
                );
        // }
        EditorApplication.playModeStateChanged += (PlayModeStateChange state) => {
            if(state == PlayModeStateChange.ExitingPlayMode){
                this.EndThreads();
            }
        };

        float elbowTorque = 0.2f;
        float cableMotorTorque = 0.2f;
        _armCmd.elbow.SetTorqueMove(-elbowTorque);
        _armCmd.shoulderDown.SetTorqueMove(-cableMotorTorque);

        sendThread = new Thread(this.TxThreadFcn);
        sendThread.Start();

        // CalibrationValues calibrationValues = new CalibrationValues();
        // calibrationValues.UpperArmLength = UpperArmLength;
        // calibrationValues.LowerArmLength = LowerArmLength;
        // calibrationValues.ShoulderDistFromNeckBase = ShoulderDistFromNeckBase;
        // calibrationValues.NeckBaseOffsetFromHeadset = NeckBaseOffsetFromHeadset;
        // calibrationValues.CableMotorOffsetfromShoulder = CableMotorOffsetfromShoulder;
        // calibrationValues.CableWinchRadius = CableWinchRadius;
        // calibrationValues.ImuSensorMsgFreq = ImuSensorMsgFreq;

        _sensorReadings = new SensorReadings(
            new BraceSensorReader(_arduinoPort);//, _portMutex), 
            TimeSpan.FromMilliseconds(sensorDataRelevanceLifetimeMs));

        // _armModel = new ArmVectorModel(_sensorReadings,
        //         calibrationValues, 
        //         useDummyInputs: UseDummyInputs,
        //         printIntermediateValues: PrintIntermediateValues,
        //         useLeftControllerAsElbowTracker: UseLeftControllerAsElbowTracker,
        //         ignoreImu:IgnoreImu,
        //         FinalTestDisable:FinalTestDisable);

        // _armMotionEstimators = new ArmMotionEstimators(Time.fixedDeltaTime);

        // EditorApplication.playModeStateChanged += (PlayModeStateChange state) => {
        //     if(state == PlayModeStateChange.ExitingPlayMode){
        //         this.ReleaseResources();
        //         if(_arduinoPort != null && _arduinoPort.IsOpen) {
        //             _arduinoPort.DiscardInBuffer();
        //             _arduinoPort.DiscardOutBuffer();
        //             _arduinoPort.Close();
        //         }
        //     }
        // };
    }

    // void OnApplicationQuit() {
    //     EndThreads();
    // }
    void EndThreads() {
        quitThread = true;
        if (sendThread.IsAlive) {
            sendThread.Join();
        }
    }

    // ~SimulationForce(){
    //     ReleaseResources();
    //     if(_arduinoPort != null && _arduinoPort.IsOpen) {
    //         _arduinoPort.DiscardInBuffer();
    //         _arduinoPort.DiscardOutBuffer();
    //         _arduinoPort.Close();
    //     }
    // }
    // void ReleaseResources() {
    //     _sensorReadings.ReleaseResources();
    // }

    // void GetHeldObjectMass(XRBaseInteractable interactable){
    //     List<Collider> colliderList = interactable.colliders;
    //     _cachedMass = colliderList[0].attachedRigidbody.mass;
    // }

    // void ZeroHeldObjectMass(XRBaseInteractable interactable) {
    //     _cachedMass = 0;
    // }

    // int count = 0;
    // int period = 1;
    // void FixedUpdate()
    // {
        // if(_sensorReadings == null) {
        //     return;
        // }
        // if(!_sensorReadings.Update()) {
        //     Debug.Log("Could not get updated sensor readings.");
        //     _armMotionEstimators.EstimateUnobtainableNewPosition();
        // } else {
        //     count += 1;
        //     if (count % period != 0) {
        //         return;
        //     }
        //     count = 0;
        //     _armMotionEstimators.UpdateNewPosition(_sensorReadings.Data); 
        // }
        // if (!FinalTestDisable) {
        //     if (!_sensorReadings.Data.MovingAvgsFilled() || !_armMotionEstimators.Filled()) {
        //         return;
        //     }
        // }
        
        // if (_sensorReadings.Data.RightControllerSecondaryButtonPressed) {
        //     ReleaseResources();
        //     Start();
        //     return;
        // }
        // _simForce = Physics.gravity*_cachedMass + _collisionForce;
    
        // if (_cachedMass > 0){
        //     _simForce += _armMotionEstimators.RightControllerPosition.EstimateAcceleration() * ArmMass * 
        //         _cachedMass / (ArmMass + _cachedMass);
        // }

        // _armModel.CalculateMotorTorques(_simForce, 
        //                                 out float elbowTorque, 
        //                                 out float cableMotorTorque);
        
        // if (!FinalTestDisable) {
        //     if (display_values)
        //     {
        //         txt.text = "Elbow Torque: " + elbowTorque.ToString() + " N";
        //         //txt.text = System.DateTime.Now.ToString();
        //     }
        // }

        // if(PrintIntermediateValues) {
        //     Logging.PrintQtyVector3("SIM_FORCE", _simForce, "N");
        //     Logging.PrintQtyScalar("ELBOW_TORQUE", elbowTorque, "N-m");
        //     Logging.PrintQtyVector3("HAND_ACCEL", _armMotionEstimators.RightControllerPosition.EstimateAcceleration(), "m/s_sqr");
        //     Logging.PrintQtyScalar("ELBOW_DEG_VELOCITY", _armMotionEstimators.ElbowDeg.EstimateVelocity(), "deg/s");
        //     Logging.PrintQtyScalar("CABLE_MOTOR_TORQUE", cableMotorTorque, "N-m");
        // }
        
        
        //applyTorques(elbowTorque, cableMotorTorque);
        // _collisionForce.Set(0,0,0);
    // }

    // void OnCollisionEnter(Collision collision){
        //Assume all collisions happen over one Time.fixedDeltaTime unit.

        //Note to self: Add the hold collision force later. 
        // _collisionForce = collision.impulse/Time.fixedDeltaTime;
    // }

    // void OnCollisionExit(Collision collision) {
    //     _collisionForce.Set(0,0,0);
    // }

    // void applyTorques(float elbowTorque, float cableMotorTorque)
    // {
        // if(armCmdMutex.WaitOne(1)) {
            // bool movementInSameDirAsTorque = (Math.Abs(_armMotionEstimators.ElbowDeg.EstimateVelocity()) >= (1 << 5)/Time.fixedDeltaTime && 
            //     Math.Sign(_armMotionEstimators.ElbowDeg.EstimateVelocity()) == Math.Sign(elbowTorque) &&
            //     Math.Sign(_sensorReadings.Data.RightControllerVelocity.y) == Math.Sign(elbowTorque) && 
            //     Math.Abs(_sensorReadings.Data.RightControllerVelocity.y) >= RightControllerVelocityThreshold);

            // bool notMoving = Math.Abs(_sensorReadings.Data.RightControllerVelocity.y) <= RightControllerVelocityThreshold;
            
            // if (RemoveHoldCommands || movementInSameDirAsTorque || notMoving) {
                
            //     elbowTorque = -elbowTorque;
            //     _armCmd.elbow.SetTorqueMove(elbowTorque);
            // } else {
            //     elbowTorque = -elbowTorque;
            //     _armCmd.elbow.SetTorqueHold(elbowTorque);
            // }

            // _armCmd.shoulderDown.SetTorqueMove(-cableMotorTorque);
            
            // if (_portMutex.WaitOne(1)) {
            //     _armCmd.Send();
            //     _portMutex.ReleaseMutex();
            // }
            
            // newCmdReady = true;
        //     armCmdMutex.ReleaseMutex();
        // }
    // }
    void TxThreadFcn() {

        DateTime startTime = DateTime.Now;
        TimeSpan interval = TimeSpan.FromMilliseconds(1000);

        while(!quitThread) {
            if ((DateTime.Now - startTime) >= interval) {
                Debug.Log("SENDING!!!!");
                _armCmd.Send();
                startTime = DateTime.Now;
            }
        }
        // if(newCmdReady && armCmdMutex.WaitOne(1)) {
        //     // _armCmd.Send();
        //     newCmdReady = false;
        //     armCmdMutex.ReleaseMutex();
        // }
    }
    // private Mutex armCmdMutex = new Mutex();
    // private bool newCmdReady;
    // private Vector3 _simForce;
    // public float _cachedMass = 0f;
    // private Vector3 _collisionForce = new Vector3(0,0,0);
    
    
    private BraceCmd _armCmd;
    // private ArmVectorModel _armModel;
    // private ArmMotionEstimators _armMotionEstimators;
    private SerialPort _arduinoPort;
    // private SensorReadings _sensorReadings = null;
}