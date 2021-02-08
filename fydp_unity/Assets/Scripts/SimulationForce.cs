using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using FYDP.ArmBrace;
using FYDP.Controllers;

public class SimulationForce : MonoBehaviour
{
    private DigitalController _elbowController;
    private DigitalController _shoulderAbductionController;
    private DigitalController _shoulderFlexionController;

    public float _elbowAngle;
    public float _shoulderFlexion;
    public float _shoulderAbduction;
    public float _upperArmLength = 0.3f;
    public float _lowerArmLength = 0.4f;
    
    private Vector3 _simForce;
    private float _cachedMass = 0f;
    private Vector3 _collisionForce = new Vector3(0,0,0);
    
    public string arduinoPortName = "/dev/ttyACM0";
    private ArmCmd ArmCmd;

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

        SerialPort arduinoPort = new SerialPort("/dev/ttyACM0");
        arduinoPort.WriteTimeout = 1;
        arduinoPort.ReadTimeout = 1;

        ArmCmd = new ArmCmd(arduinoPort);
    }

    void FixedUpdate()
    {
        _simForce = Physics.gravity*_cachedMass + _collisionForce;
        //CalcJointTorques(_simForce);
        //applyTorques();
        _collisionForce.Set(0,0,0);
    }

    void OnCollisionEnter(Collision collision)
    {
        //Assume all collisions happen over one Time.fixedDeltaTime unit.
        _collisionForce = collision.impulse/Time.fixedDeltaTime;
    }

    void applyTorques(float elbowTorque, float shoulderAbductionTorque, 
                      float shoulderFlexionTorque)
    {
        ArmCmd.elbow.SetTorque(_elbowController.controlEffort(elbowTorque));
        ArmCmd.shoulderAbduction.SetTorque(
            _shoulderAbductionController.controlEffort(shoulderAbductionTorque));
        
        ArmCmd.shoulderFlexion.SetTorque( 
            _shoulderFlexionController.controlEffort(shoulderFlexionTorque));

        ArmCmd.Send();
    }

}