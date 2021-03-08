using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FYDP.ArmBrace;
using FYDP.Utils;
using System;

public class ArmVectorModel_test : MonoBehaviour
{
    SensorReadings sensorReadings;
    SensorData dummyData;
    ArmVectorModel armVectorModel;
    CalibrationValues calibrationValues;
    Vector3 forceAtHand = new Vector3(0, 1, 0);
    void Start() {
        Vector3 rightControllerPosition = new Vector3(0,0,0);
        Vector3 elbowPosition = new Vector3(0, 0, -1);
        Vector3 shoulderPosition = new Vector3(0, 1, -1);
        Vector3 neckBasePosition = new Vector3(-1, 1, -1);
        Vector3 headsetPosition = new Vector3(-1, 2, -1);

        Vector3 upperArmVector = elbowPosition - shoulderPosition;
        Vector3 lowerArmVector = rightControllerPosition - elbowPosition;
        Vector3 neckBaseToShoulderVector = shoulderPosition - neckBasePosition;
        
        Vector3 torsoForwardAxisVector = Vector3.Cross(
                    neckBaseToShoulderVector, Vector3.up).normalized;

        calibrationValues.UpperArmLength = upperArmVector.magnitude;
        calibrationValues.LowerArmLength = lowerArmVector.magnitude;
        calibrationValues.ShoulderDistFromNeckBase = neckBaseToShoulderVector.magnitude; 
        calibrationValues.NeckBaseOffsetFromHeadset = neckBasePosition - headsetPosition;

        dummyData = new SensorData();
        dummyData.OverwriteElbowDeg(Vector3.Angle(lowerArmVector, -upperArmVector));
        dummyData.OverwriteShoulderAbductionDeg(Vector3.Angle(
            Vector3.down, Vector3.ProjectOnPlane(upperArmVector, torsoForwardAxisVector)));
        dummyData.OverwriteShoulderFlexionDeg(Vector3.Angle(
            Vector3.down, Vector3.ProjectOnPlane(upperArmVector, neckBaseToShoulderVector)));

        dummyData.OverwriteHeadsetRotation(Quaternion.FromToRotation(
            Vector3.down, calibrationValues.NeckBaseOffsetFromHeadset));
        dummyData.OverwriteHeadsetPosition(headsetPosition);
        dummyData.OverwriteRightControllerPosition(rightControllerPosition);

        sensorReadings = new SensorReadings(
            new BraceSensorReader(null),
            TimeSpan.MaxValue,
            dummyData);

        sensorReadings.UseDummySensorReadings(true);
    }

    void Update() {
        armVectorModel = new ArmVectorModel(sensorReadings,  
                calibrationValues,
                useDummyInputs: true,
                printIntermediateValues: true);
        
        armVectorModel.CalculateJointTorques(forceAtHand: forceAtHand,  
                out float elbowTorque, out float shoulderAbductionTorque, 
                out float shoulderFlexionTorque);

        Logging.PrintQty("ELBOW_TORQUE", elbowTorque, "N-m");
        Logging.PrintQty("SHOULDER_ABDUCTION_TORQUE", shoulderAbductionTorque, "N-m");
        Logging.PrintQty("SHOULDER_FLEXION_TORQUE", shoulderFlexionTorque, "N-m");
    }
}