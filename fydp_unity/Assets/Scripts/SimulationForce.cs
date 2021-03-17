using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO.Ports;
using System;
using FYDP.ArmBrace;
using FYDP.VR;
using FYDP.Utils;
using UnityEditor;

public class SimulationForce : MonoBehaviour
{
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
    private int SerialWriteTimeout = 1;
    [SerializeField]
    private int SerialReadTimeout = 1;
    [SerializeField]
    private int SerialReadBufferSize = 16;
    [SerializeField]
    private int SerialWriteBufferSize = 16;
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
    private float ShoulderGearRatio = 4;
    [SerializeField]
    private float MotorPowerFraction = 0.5;

    private class ArmMotionEstimators {
        public MotionEstimatorFloat ElbowDeg;
        public MotionEstimatorVector3 RightControllerPosition;

        public ArmMotionEstimators(float timestepSeconds) {
            ElbowDeg = new MotionEstimatorFloat(timestepSeconds);
            RightControllerPosition = new MotionEstimatorVector3(timestepSeconds);
        }
        public void EstimateUnobtainableNewPosition() {
            ElbowDeg.EstimateUnobtainableNewPosition();
            RightControllerPosition.EstimateUnobtainableNewPosition();
        }

        public void UpdateNewPosition(SensorData sensorData) {
            ElbowDeg.UpdateNewPosition(sensorData.ElbowDeg);
            RightControllerPosition.UpdateNewPosition(sensorData.RightControllerPosition);
        }

        public bool Filled(){
            return ElbowDeg.filled && RightControllerPosition.filled;
        }
    }
    void Start()
    {
        XRDirectInteractor controllerInteractor = GetComponentInParent<XRDirectInteractor>();
        controllerInteractor.onSelectEntered.AddListener(GetHeldObjectMass);
        controllerInteractor.onSelectExited.AddListener(ZeroHeldObjectMass);

        if(!UseDummyInputs) {
            _arduinoPort = new SerialPort(ArduinoPortName, ArduinoBaudRate);
        
            //Will need to look into the correct values for this.
            _arduinoPort.WriteTimeout = SerialWriteTimeout;
            _arduinoPort.ReadTimeout = SerialReadTimeout;
            _arduinoPort.ReadBufferSize = SerialReadBufferSize;
            _arduinoPort.WriteBufferSize = SerialWriteBufferSize;

            _armCmd = new BraceCmd(
                _arduinoPort, 
                elbow_:new MotorCmdFormat(
                    torqueRatingNm:1.2f, torqueCmdFullScale:MotorPowerFraction * 89, gearRatio:5, 
                    stictionEncodedTorque:8),
                shoulderDown_:new MotorCmdFormat(
                    torqueRatingNm:1.89f, torqueCmdFullScale:MotorPowerFraction * 89, gearRatio:ShoulderGearRatio, 
                    stictionEncodedTorque:8)
                );
        }

        CalibrationValues calibrationValues = new CalibrationValues();
        calibrationValues.UpperArmLength = UpperArmLength;
        calibrationValues.LowerArmLength = LowerArmLength;
        calibrationValues.ShoulderDistFromNeckBase = ShoulderDistFromNeckBase;
        calibrationValues.NeckBaseOffsetFromHeadset = NeckBaseOffsetFromHeadset;
        
        _sensorReadings = new SensorReadings(
            new BraceSensorReader(_arduinoPort), 
            TimeSpan.FromMilliseconds(sensorDataRelevanceLifetimeMs));

        _armModel = new ArmVectorModel(_sensorReadings,
                calibrationValues, 
                useDummyInputs: UseDummyInputs,
                printIntermediateValues: PrintIntermediateValues);

        _armMotionEstimators = new ArmMotionEstimators(Time.fixedDeltaTime);

        EditorApplication.playModeStateChanged += (PlayModeStateChange state) => {
            if(state == PlayModeStateChange.ExitingPlayMode){
                this.ReleaseResources();
            }
        };
    }

    ~SimulationForce(){
        ReleaseResources();
    }
    void OnApplicationQuit() {
        ReleaseResources();
    }
    void ReleaseResources() {
        _arduinoPort.Close();
    }

    void GetHeldObjectMass(XRBaseInteractable interactable){
        List<Collider> colliderList = interactable.colliders;
        _cachedMass = colliderList[0].attachedRigidbody.mass;
    }

    void ZeroHeldObjectMass(XRBaseInteractable interactable) {
        _cachedMass = 0;
    }

    int count = 0;
    int period = 3;
    void FixedUpdate()
    {
        if(!_sensorReadings.Update()) {
            Debug.Log("Could not get updated sensor readings.");
            _armMotionEstimators.EstimateUnobtainableNewPosition();
        } else {
            count += 1;
            if (count % period != 0) {
                return;
            }
            count = 0;
            _armMotionEstimators.UpdateNewPosition(_sensorReadings.Data); 
        }
        
        if (!_sensorReadings.Data.MovingAvgsFilled() || !_armMotionEstimators.Filled()) {
            return;
        }
    
        if (_sensorReadings.Data.RightControllerSecondaryButtonPressed) {
            Start();
            return;
        }

        _simForce = Physics.gravity*_cachedMass + _collisionForce;
    
        if (_cachedMass > 0){
            _simForce += _armMotionEstimators.RightControllerPosition.EstimateAcceleration() * ArmMass * 
                _cachedMass / (ArmMass + _cachedMass);
        }

        _armModel.CalculateJointTorques(
            forceAtHand: _simForce,
            out float elbowTorque, 
            out float shoulderAbductionTorque, 
            out float shoulderFlexionTorque);

        if(PrintIntermediateValues) {
            Logging.PrintQtyVector3("SIM_FORCE", _simForce, "N");
            Logging.PrintQtyScalar("ELBOW_TORQUE", elbowTorque, "N-m");
            Logging.PrintQtyVector3("HAND_ACCEL", _armMotionEstimators.RightControllerPosition.EstimateAcceleration(), "m/s_sqr");
            Logging.PrintQtyScalar("ELBOW_DEG_VELOCITY", _armMotionEstimators.ElbowDeg.EstimateVelocity(), "deg/s");
            Logging.PrintQtyScalar("SHOULDER_ABDUCTION_TORQUE", shoulderAbductionTorque, "N-m");
            Logging.PrintQtyScalar("SHOULDER_FLEXION_TORQUE", shoulderFlexionTorque, "N-m");
        }

        applyTorques(elbowTorque, shoulderAbductionTorque, shoulderFlexionTorque);
        _collisionForce.Set(0,0,0);
    }

    void OnCollisionEnter(Collision collision){
        //Assume all collisions happen over one Time.fixedDeltaTime unit.

        //Note to self: Add the hold collision force later. 
        _collisionForce = collision.impulse/Time.fixedDeltaTime;
    }

    void OnCollisionExit(Collision collision) {
        _collisionForce.Set(0,0,0);
    }

    void applyTorques(float elbowTorque, float shoulderAbductionTorque, 
                      float shoulderFlexionTorque)
    {
        //elbowTorque = -elbowTorque;
        if (_armMotionEstimators.ElbowDeg.EstimateVelocity() != 0 && 
            (_armMotionEstimators.ElbowDeg.EstimateVelocity() > 0) == (elbowTorque > 0)) {
            
            _armCmd.elbow.SetTorqueMove(elbowTorque);
        } else {
            _armCmd.elbow.SetTorqueHold(elbowTorque);
        }
        
        _armCmd.Send();
    }
    
    private Vector3 _simForce;
    public float _cachedMass = 0f;
    private Vector3 _collisionForce = new Vector3(0,0,0);
    
    
    private BraceCmd _armCmd;
    private ArmVectorModel _armModel;
    private ArmMotionEstimators _armMotionEstimators;
    private SerialPort _arduinoPort;
    private SensorReadings _sensorReadings;
}