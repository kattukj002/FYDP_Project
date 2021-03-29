using UnityEngine;
using FYDP.VR;
using FYDP.Utils;
using System;

namespace FYDP {
    namespace ArmBrace {
        public class ArmVectorModel {
            
            public ArmVectorModel(SensorReadings sensorReadings,  
                CalibrationValues calibrationValues,
                bool useDummyInputs=false,
                bool printIntermediateValues=false,
                bool useLeftControllerAsElbowTracker=false,
                bool ignoreImu=false) {

                _upperArmLength = calibrationValues.UpperArmLength;
                _lowerArmLength = calibrationValues.LowerArmLength;
                _shoulderDistFromNeckBase = calibrationValues.ShoulderDistFromNeckBase;
                _neckBaseOffsetFromHeadset = calibrationValues.NeckBaseOffsetFromHeadset;
                _cableMotorOffsetfromShoulder = calibrationValues.CableMotorOffsetfromShoulder;
                _cableWinchRadius = calibrationValues.CableWinchRadius;

                _useDummySensorReadings = useDummyInputs;
                _printIntermediateValues = printIntermediateValues;
                _useLeftControllerAsElbowTracker = useLeftControllerAsElbowTracker;
               
               _ignoreImu = ignoreImu;
                _sensorReadings = sensorReadings;
                _sensorReadings.TryInitSensors(_upperArmLength, _neckBaseOffsetFromHeadset, _shoulderDistFromNeckBase, calibrationValues.ImuSensorMsgFreq);
            }

            public void CalculateMotorTorques(Vector3 forceAtHand, 
                                              out float elbowTorque, 
                                              out float cableMotorTorque) {
                    
                EstimateArmGeometry(
                    out Vector3 elbowPosition,
                    out Vector3 elbowAxisVector,
                    out Vector3 shoulderPosition, 
                    out Vector3 upperArmVector,
                    out Vector3 lowerArmVector);
                
                Vector3 shoulderMomentArm = _sensorReadings.Data.RightControllerPosition - shoulderPosition;

                Vector3 shoulderMoment = Vector3.Cross(shoulderMomentArm, 
                                                       forceAtHand);
                
                Vector3 cableVector = (shoulderPosition + _cableMotorOffsetfromShoulder) - elbowPosition;

                Vector3 cableTensionPlaneNormal = Vector3.Cross(_cableMotorOffsetfromShoulder, cableVector).normalized;
                
                elbowTorque = Vector3.Dot(
                    Vector3.Cross(lowerArmVector, forceAtHand), 
                    elbowAxisVector);
                
                float cableTorque = Vector3.Dot(shoulderMoment, cableTensionPlaneNormal);

                float cableTension = 0;
                if (cableTorque <= 0) {
                    cableMotorTorque = 0;
                } else {
                    cableTension = Vector3.Cross(upperArmVector, cableTorque * cableTensionPlaneNormal).magnitude;
                    cableMotorTorque = cableTension * _cableWinchRadius;
                }

                //Translate to right-hand moment vector convention.
                elbowTorque *=-1;

                if (_printIntermediateValues) {
                    Logging.PrintQtyScalar("ELBOW_ANGLE", _sensorReadings.Data.ElbowDeg, "deg");
                    Logging.PrintQtyVector3("UPPER_ARM_VECTOR", upperArmVector, " m");
                    Logging.PrintQtyVector3("LOWER_ARM_VECTOR", lowerArmVector, " m");

                    Logging.PrintQtyVector3("SHOULDER_POSITION", shoulderPosition, "m");
                    Logging.PrintQtyVector3("ELBOW_POSITION", elbowPosition, "m");
                    
                    Logging.PrintQtyVector3("SHOULDER_MOMENT: ", shoulderMoment, " N-m");

                    Logging.PrintQtyVector3("CABLE_VECTOR: ", cableVector, "m");
                    Logging.PrintQtyVector3("CABLE_TENSION_PLANE_NORMAL: ", cableTensionPlaneNormal,  "m");
                    

                    Logging.PrintQtyScalar("CABLE_TORQUE", cableTorque, "N-m");
                    Logging.PrintQtyScalar("CABLE_TENSION", cableTension, "N");
                    
                    Logging.PrintQtyVector3("SHOULDER_MOMENT_ARM_VECTOR", shoulderMomentArm, " N-m");
                                        
                }
            }

            private void EstimateArmGeometry(
                out Vector3 elbowPosition,
                out Vector3 elbowAxisVector,
                out Vector3 shoulderPosition, 
                out Vector3 upperArmVector,
                out Vector3 lowerArmVector) {

                if (_ignoreImu) {
                    elbowAxisVector = _sensorReadings.Data.HeadsetRotation * Vector3.right;    
                    upperArmVector = Vector3.down * _upperArmLength;
                    
                    if (!_useLeftControllerAsElbowTracker) {
                        Quaternion upperToLowerArmRotation = Quaternion.AngleAxis(180 - _sensorReadings.Data.ElbowDeg, elbowAxisVector);
                        lowerArmVector = (upperToLowerArmRotation * upperArmVector).normalized * _lowerArmLength;
                        elbowPosition = _sensorReadings.Data.RightControllerPosition - lowerArmVector;    
                    } else {
                        elbowPosition = _sensorReadings.Data.LeftControllerPosition;
                        lowerArmVector = _sensorReadings.Data.RightControllerPosition - elbowPosition;
                    }
                    
                    shoulderPosition = elbowPosition - upperArmVector;    
                } else {
                    elbowPosition = _sensorReadings.Data.ElbowPositionEstimate;
                    elbowAxisVector = _sensorReadings.Data.ElbowAxisEstimate.normalized;

                    upperArmVector =  _upperArmLength * _sensorReadings.Data.UpperArmAxisEstimate.normalized;
                    lowerArmVector = _sensorReadings.Data.RightControllerPosition - elbowPosition;
                    shoulderPosition = elbowPosition - upperArmVector;
                }
                

                if (_printIntermediateValues) {
                    Logging.PrintQtyVector3("HAND_POSITION", _sensorReadings.Data.RightControllerPosition, "m");
                }
            }
            
            private float _upperArmLength;
            private float _lowerArmLength;
            private float _shoulderDistFromNeckBase;
            private Vector3 _neckBaseOffsetFromHeadset;
            private Vector3 _cableMotorOffsetfromShoulder;
            private float _cableWinchRadius;
            private bool _useDummySensorReadings;
            private bool _printIntermediateValues;
            private bool _useLeftControllerAsElbowTracker;
            private bool _ignoreImu;
            
            private SensorReadings _sensorReadings;
            private MotionIntegratorVector3 _imuAccelIntegrator;
            private MotionIntegratorVector3 _imuAngleIntegrator;
        }
    }
}
