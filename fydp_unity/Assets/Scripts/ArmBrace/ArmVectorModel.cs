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
                bool useLeftControllerAsElbowTracker=false) {

                _upperArmLength = calibrationValues.UpperArmLength;
                _lowerArmLength = calibrationValues.LowerArmLength;
                _shoulderDistFromNeckBase = calibrationValues.ShoulderDistFromNeckBase;
                _neckBaseOffsetFromHeadset = calibrationValues.NeckBaseOffsetFromHeadset;

                _useDummySensorReadings = useDummyInputs;
                _printIntermediateValues = printIntermediateValues;
                _useLeftControllerAsElbowTracker = useLeftControllerAsElbowTracker;
               
               _sensorReadings = sensorReadings;
                _cachedElbowAxis = new Vector3(0,0,0);
                _prevNeckBaseToShoulder = Vector3.right * _shoulderDistFromNeckBase;
                _prevShoulderToElbow = Vector3.down * _upperArmLength;
                TryInitSensors();
            }

            public bool TryInitSensors() {
                return _sensorReadings.TryInitSensors();
            }

            public void CalculateJointTorques(Vector3 forceAtHand, 
                out float elbowTorque, 
                out float shoulderAbductionTorque, 
                out float shoulderFlexionTorque) {
                    
                EstimateArmGeometry(out Vector3 shoulderPosition,
                                    out Vector3 rightShoulderAxisVector, 
                                    out Vector3 torsoUpAxisVector,
                                    out Vector3 torsoForwardAxisVector,
                                    out Vector3 elbowPosition,
                                    out Vector3 elbowAxisVector,
                                    out Vector3 lowerArmVector);
                
                Vector3 shoulderMomentArm = _sensorReadings.Data.RightControllerPosition -  
                                            shoulderPosition;

                Vector3 shoulderMoment = Vector3.Cross(shoulderMomentArm, 
                                                       forceAtHand);
                
                shoulderAbductionTorque = Vector3.Dot(
                    shoulderMoment, torsoForwardAxisVector);

                shoulderFlexionTorque = Vector3.Dot(
                    shoulderMoment, rightShoulderAxisVector);

                Vector3 torsoUpUnitVector = Vector3.Cross(
                    torsoForwardAxisVector, rightShoulderAxisVector).normalized;
                
                elbowTorque = Vector3.Dot(
                    Vector3.Cross(lowerArmVector, forceAtHand), 
                    elbowAxisVector);

                //Translate to right-hand moment vector convention.
                shoulderAbductionTorque *= -1;
                shoulderFlexionTorque *= -1;
                elbowTorque *=-1;

                if (_printIntermediateValues) {
                    Logging.PrintQtyScalar("ELBOW_ANGLE", _sensorReadings.Data.ElbowDeg, "deg");
                    Logging.PrintQtyScalar("SHOULDER_ABDUCTION_ANGLE", _sensorReadings.Data.ShoulderAbductionDeg, "deg");
                    Logging.PrintQtyScalar("SHOULDER_FLEXION_ANGLE", _sensorReadings.Data.ShoulderFlexionDeg, "deg");

                    Logging.PrintQtyVector3("SHOULDER_POSITION", shoulderPosition, "m");
                    Logging.PrintQtyVector3("ELBOW_POSITION", elbowPosition, "m");
                    
                    Logging.PrintQtyVector3("SHOULDER_MOMENT: ", shoulderMoment, " N-m");

                    Logging.PrintQtyVector3("SHOULDER_RIGHT_AXIS_VECTOR", rightShoulderAxisVector, "m");
                    Logging.PrintQtyVector3("TORSO_FORWARD_AXIS_VECTOR", torsoForwardAxisVector, "m");
                    Logging.PrintQtyVector3("TORSO_UP_AXIS_VECTOR", torsoUpUnitVector, "m");
                    Logging.PrintQtyVector3("RIGHT_ELBOW_AXIS_VECTOR", elbowAxisVector, "m");
                    
                    Logging.PrintQtyVector3("SHOULDER_MOMENT_ARM_VECTOR", shoulderMomentArm, " N-m");
                    Logging.PrintQtyVector3("LOWER_ARM_VECTOR", lowerArmVector, " m");                    
                }
            }

            private void EstimateArmGeometry(
                out Vector3 shoulderPosition,
                out Vector3 rightShoulderAxisVector, 
                out Vector3 torsoUpAxisVector,
                out Vector3 torsoForwardAxisVector,
                out Vector3 elbowPosition,
                out Vector3 elbowAxisVector,
                out Vector3 lowerArmVector) {
                
                
                rightShoulderAxisVector = _sensorReadings.Data.HeadsetRotation * Vector3.right;
                elbowAxisVector = rightShoulderAxisVector;
                torsoUpAxisVector = Vector3.up;
                torsoForwardAxisVector = _sensorReadings.Data.HeadsetRotation * Vector3.forward;
                
                Quaternion dummyRot = Quaternion.AngleAxis(180 - _sensorReadings.Data.ElbowDeg, elbowAxisVector);
                Vector3 upperArmVector = Vector3.down * _upperArmLength;
                
                if (!_useLeftControllerAsElbowTracker) {
                    lowerArmVector = (dummyRot * upperArmVector).normalized * _lowerArmLength;
                    elbowPosition = _sensorReadings.Data.RightControllerPosition - lowerArmVector;    
                } else {
                    elbowPosition = _sensorReadings.Data.LeftControllerPosition;
                    lowerArmVector = _sensorReadings.Data.RightControllerPosition - elbowPosition;
                }
                 
                shoulderPosition = elbowPosition - upperArmVector;

                if (_printIntermediateValues) {
                    Logging.PrintQtyVector3("HAND_POSITION", _sensorReadings.Data.RightControllerPosition, "m");
                    Logging.PrintQtyVector3("UPPER_ARM_VECTOR", upperArmVector, " m");
                }
            }
            
            private float _upperArmLength;
            private float _lowerArmLength;
            private float _shoulderDistFromNeckBase;
            private Vector3 _neckBaseOffsetFromHeadset;
            private bool _useDummySensorReadings;
            private bool _printIntermediateValues;
            private bool _useLeftControllerAsElbowTracker;
            private Vector3 _prevNeckBaseToShoulder;
            private Vector3 _prevShoulderToElbow;
            private SensorReadings _sensorReadings;
            private Vector3 _cachedElbowAxis;
            private Vector3 _zeroVector;
        }
    }
}
