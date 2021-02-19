using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using FYDP.ArmBrace;

public class ArmVectorModel_test : MonoBehaviour
{
    ArmVectorModel armVectorModel;
    public float upperArmLength = 0.4899f;
    public float lowerArmLength = 0.5f;
    public Vector3 forceAtHand = Vector3.down*9.81f;
    public Vector3 rightControllerLocation = new Vector3(-0.5f, -0.4899f, 0f); 
    
    public int _elbowAngleDeg = 90, _shoulderAbductionDeg = 90, _shoulderFlexionDeg = 0;
    public Vector3 _headsetPosition = new Vector3(0, 0, 0.3f);
    public Vector3 headSetFromDirection = Vector3.forward;
    public Vector3 headSetToDirection = Vector3.forward;

    public float shoulderOffsetFromNeckBaseLength = 0.2f;

    public Vector3 shoulderOffsetFromNeckBaseFromDirection = Vector3.down;
    public Vector3 shoulderOffsetFromNeckBaseToDirection = Vector3.right;
    public float neckBaseOffsetFromHeadsetLength = 0.25f;
    public Vector3 neckBaseOffsetFromHeadsetFromDirection = Vector3.forward;
    public Vector3 neckBaseOffsetFromHeadsetToDirection = Vector3.down;
    void Start() {
    }

    ArmVectorModel.OffsetPolarVector CalcOffsetVector(float length, Vector3 fromDirection, 
                                      Vector3 toDirection) {
        ArmVectorModel.OffsetPolarVector offsetVector= new ArmVectorModel.OffsetPolarVector();
        offsetVector.Length = length;
        offsetVector.Rotation = new Quaternion(0,0,0,0);
        offsetVector.Rotation.SetFromToRotation(fromDirection, toDirection);
        return offsetVector;
    }
    void Update() {
        armVectorModel = new ArmVectorModel(new BraceSensorReader(null),
            upperArmLength: upperArmLength, lowerArmLength: lowerArmLength, 
            shoulderOffsetFromNeckBase: CalcOffsetVector(shoulderOffsetFromNeckBaseLength, 
                shoulderOffsetFromNeckBaseFromDirection, shoulderOffsetFromNeckBaseToDirection), 
            neckBaseOffsetFromHeadset:CalcOffsetVector(neckBaseOffsetFromHeadsetLength, 
                neckBaseOffsetFromHeadsetFromDirection, neckBaseOffsetFromHeadsetToDirection), 
            debug:true);

        armVectorModel._headsetRotation = new Quaternion(0,0,0,0);
        armVectorModel._headsetRotation.SetFromToRotation(headSetFromDirection, headSetToDirection);
        armVectorModel._headsetPosition = _headsetPosition;
        armVectorModel._elbowAngleDeg = _elbowAngleDeg;
        armVectorModel._shoulderAbductionDeg = _shoulderAbductionDeg;
        armVectorModel._shoulderFlexionDeg = _shoulderFlexionDeg;

        armVectorModel.CalculateJointTorques(forceAtHand:forceAtHand, 
                rightControllerLocation: rightControllerLocation, 
                out float elbowTorque, out float shoulderAbductionTorque, 
                out float shoulderFlexionTorque);

        Debug.Log("Elbow Torque: " + elbowTorque.ToString() + "N, Shoulder " + 
                  "Abduction Torque: " + shoulderAbductionTorque.ToString() + 
                  "N, Shoulder Flexion Torque: " + 
                  shoulderFlexionTorque.ToString() + "N");
    }


}