using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO.Ports;
using FYDP.ArmBrace;
using FYDP.Controllers;
using FYDP.VR;

public class SimulationForce : MonoBehaviour
{
    private DigitalController _elbowController;
    private DigitalController _shoulderAbductionController;
    private DigitalController _shoulderFlexionController;

    public float UpperArmLength = 0.3f;
    public float LowerArmLength = 0.4f;
    public float ArmMass = 2f;
    
    private InputDevice _rightController;
    private Vector3 _simForce;
    //Temp, testing purposes only.
    public float _cachedMass = 0f;
    private Vector3 _collisionForce = new Vector3(0,0,0);
    
    public string arduinoPortName = "/dev/ttyACM0";
    private BraceCmd _armCmd;
    private ArmVectorModel _armModel;
    private MotionEstimator motionEstimator;
    public bool _debug = false;
    public Vector3 _rightControllerLocation;

    void Start()
    {
        _elbowController = new PidController(
            pGain: 5, iGain: 0.01f, dGain: 2, 
            samplingPeriod: Time.fixedDeltaTime, derivativeRollOffPole: -40);
        _shoulderAbductionController = new PidController(
            pGain: 5, iGain: 0.01f, dGain: 2, 
            samplingPeriod: Time.fixedDeltaTime, derivativeRollOffPole: -40);
        _shoulderFlexionController = new PidController(
            pGain: 5, iGain: 0.01f, dGain: 2, 
            samplingPeriod: Time.fixedDeltaTime, derivativeRollOffPole: -40);

        SerialPort arduinoPort = null;

        XRDirectInteractor controllerInteractor = GetComponentInParent<XRDirectInteractor>();
        controllerInteractor.onSelectEntered.AddListener(GetHeldObjectMass);
        controllerInteractor.onSelectExited.AddListener(ZeroHeldObjectMass);

        if(!_debug) {
            arduinoPort = new SerialPort("/dev/ttyACM0");
        
            //Will need to look into the correct values for this.
            arduinoPort.WriteTimeout = 1;
            arduinoPort.ReadTimeout = 1;
            arduinoPort.ReadBufferSize = 16;
            arduinoPort.WriteBufferSize = 16;


            _armCmd = new BraceCmd(arduinoPort);

            if(!VRUtils.TryGetInputDevice(
                VRUtils.DeviceId.RightController, out _rightController)) {
                
                Debug.Log("Could not access right controller.");
            }
        }
        
        ArmVectorModel.OffsetPolarVector shoulderOffsetFromNeckBase = new ArmVectorModel.OffsetPolarVector();
        shoulderOffsetFromNeckBase.Length = 0.1f;
        shoulderOffsetFromNeckBase.Rotation = Quaternion.AngleAxis(90, Vector3.right);
        ArmVectorModel.OffsetPolarVector neckBaseOffsetFromHeadset = new ArmVectorModel.OffsetPolarVector();
        neckBaseOffsetFromHeadset.Length = 0.2f;
        neckBaseOffsetFromHeadset.Rotation = Quaternion.AngleAxis(90, Vector3.right);

        _armModel = new ArmVectorModel(new BraceSensorReader(arduinoPort),
                upperArmLength: UpperArmLength, lowerArmLength: LowerArmLength, 
                shoulderOffsetFromNeckBase: shoulderOffsetFromNeckBase, 
                neckBaseOffsetFromHeadset: neckBaseOffsetFromHeadset, debug:_debug);

        motionEstimator = new MotionEstimator(Time.fixedDeltaTime);
    }

    void GetHeldObjectMass(XRBaseInteractable interactable){

        //TODO: need to make sure the type of the interactable is UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable

        List<Collider> colliderList = interactable.colliders;
        _cachedMass = colliderList[0].attachedRigidbody.mass;
    }

    void ZeroHeldObjectMass(XRBaseInteractable interactable) {
        _cachedMass = 0;
    }

    void FixedUpdate()
    {
        bool couldReadRightController;
        Vector3 rightControllerLocation;
        if (!_debug) {
            couldReadRightController = _rightController.TryGetFeatureValue(
                CommonUsages.devicePosition, out rightControllerLocation);
        } else {
            couldReadRightController = true;
            rightControllerLocation = _rightControllerLocation;
        }
        

        if(couldReadRightController){
            motionEstimator.UpdateNewPosition(rightControllerLocation);
        } else {
            motionEstimator.EstimateUnobtainableNewPosition();
        }

        _simForce = Physics.gravity*_cachedMass + _collisionForce;
        
        if(_debug) {
            Debug.Log("SIM_FORCE = " + _simForce);
        }

        if (_cachedMass > 0){
            // The "magic product" is the inertial force, may need to be modified.
            _simForce += motionEstimator.EstimateAcceleration() * ArmMass * 
                _cachedMass / (ArmMass + _cachedMass);
        }

        if(!_debug) {
            if(_armModel.CalculateJointTorques(forceAtHand: _simForce,
                    rightControllerLocation, out float elbowTorque, 
                    out float shoulderAbductionTorque, 
                    out float shoulderFlexionTorque)) {
                
                applyTorques(elbowTorque, 
                            shoulderAbductionTorque, 
                            shoulderFlexionTorque);
                _collisionForce.Set(0,0,0);
            }
        }
        
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
        _armCmd.elbow.SetTorque(_elbowController.controlEffort(elbowTorque));
        _armCmd.shoulderAbduction.SetTorque(
            _shoulderAbductionController.controlEffort(shoulderAbductionTorque));
        _armCmd.shoulderFlexion.SetTorque( 
            _shoulderFlexionController.controlEffort(shoulderFlexionTorque));

        _armCmd.Send();
    }

}