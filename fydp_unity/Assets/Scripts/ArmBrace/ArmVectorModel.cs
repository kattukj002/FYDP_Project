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
                bool printIntermediateValues=false) {

                _upperArmLength = calibrationValues.UpperArmLength;
                _lowerArmLength = calibrationValues.LowerArmLength;
                _shoulderDistFromNeckBase = calibrationValues.ShoulderDistFromNeckBase;
                _neckBaseOffsetFromHeadset = calibrationValues.NeckBaseOffsetFromHeadset;

                _useDummySensorReadings = useDummyInputs;
                _printIntermediateValues = printIntermediateValues;
               
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
                    Logging.PrintQty("ELBOW_ANGLE", _sensorReadings.Data.ElbowDeg, "deg");
                    Logging.PrintQty("SHOULDER_ABDUCTION_ANGLE", _sensorReadings.Data.ShoulderAbductionDeg, "deg");
                    Logging.PrintQty("SHOULDER_FLEXION_ANGLE", _sensorReadings.Data.ShoulderFlexionDeg, "deg");

                    Logging.PrintQty("SHOULDER_POSITION", shoulderPosition, "m");
                    Logging.PrintQty("ELBOW_POSITION", elbowPosition, "m");
                    
                    Logging.PrintQty("SHOULDER_MOMENT: ", shoulderMoment, " N-m");

                    Logging.PrintQty("SHOULDER_RIGHT_AXIS_VECTOR", rightShoulderAxisVector, "m");
                    Logging.PrintQty("TORSO_FORWARD_AXIS_VECTOR", torsoForwardAxisVector, "m");
                    Logging.PrintQty("TORSO_UP_AXIS_VECTOR", torsoUpUnitVector, "m");
                    Logging.PrintQty("RIGHT_ELBOW_AXIS_VECTOR", elbowAxisVector, "m");
                    
                    Logging.PrintQty("SHOULDER_MOMENT_ARM_VECTOR", shoulderMomentArm, " N-m");
                    Logging.PrintQty("LOWER_ARM_VECTOR", lowerArmVector, " m");                    
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

                Vector3 neckBasePosition = 
                    _sensorReadings.Data.HeadsetRotation * _neckBaseOffsetFromHeadset + 
                    _sensorReadings.Data.HeadsetPosition;

                Vector3 neckBaseToHand = 
                    _sensorReadings.Data.RightControllerPosition - neckBasePosition;
                
                // Cosine Law
                float shoulderToHandLength = 
                    Formulas.CosineLawCalcLength(
                        _upperArmLength, _lowerArmLength, 
                        Units.DegreesToRadians(_sensorReadings.Data.ElbowDeg));

                float cos_NeckBaseHand_neckBaseShoulder_Angle = 
                    Formulas.CosineLawCosAngle(neckBaseToHand.magnitude, 
                                      _shoulderDistFromNeckBase,
                                      shoulderToHandLength);
                
                Vector3 neckBaseToShoulder = Formulas.YPlaneLockedTwoBarStartMidVector(
                    startEndVector: neckBaseToHand, 
                    startMidLength: _shoulderDistFromNeckBase, 
                    midEndLength: shoulderToHandLength, 
                    cosLinkageAngle: cos_NeckBaseHand_neckBaseShoulder_Angle,
                    prevStartMid: _prevNeckBaseToShoulder);

                _prevNeckBaseToShoulder = neckBaseToShoulder;

                shoulderPosition = neckBasePosition + neckBaseToShoulder;

                rightShoulderAxisVector = neckBaseToShoulder.normalized;

                torsoUpAxisVector = Vector3.up;
                torsoForwardAxisVector = Vector3.Cross(
                    neckBaseToShoulder, torsoUpAxisVector).normalized;

                Vector3 upperArmVector = Vector3.Cross(
                        Quaternion.AngleAxis(_sensorReadings.Data.ShoulderFlexionDeg + 90, 
                            rightShoulderAxisVector) * -torsoUpAxisVector,
                        Quaternion.AngleAxis(_sensorReadings.Data.ShoulderAbductionDeg + 90, 
                            torsoForwardAxisVector) * -torsoUpAxisVector).normalized
                        * _upperArmLength;

                if (Vector3.Equals(upperArmVector, Constants.ZeroVector)) {
                    upperArmVector = Formulas.YPlaneLockedTwoBarStartMidVector(
                        startEndVector: _sensorReadings.Data.RightControllerPosition - shoulderPosition, 
                        startMidLength: _upperArmLength, 
                        midEndLength: _lowerArmLength, 
                        cosLinkageAngle: (float)Math.Cos(Units.DegreesToRadians(_sensorReadings.Data.ElbowDeg)),
                        prevStartMid: _prevShoulderToElbow
                    );
                }
                 _prevShoulderToElbow = upperArmVector;
                
                elbowPosition = shoulderPosition + upperArmVector;
                
                lowerArmVector = _lowerArmLength * 
                    (_sensorReadings.Data.RightControllerPosition - elbowPosition).normalized;

                elbowAxisVector = Vector3.Cross(
                    lowerArmVector, upperArmVector).normalized;
                
                if(Vector3.Equals(elbowAxisVector, Constants.ZeroVector)) {
                    if(_cachedElbowAxis != Constants.ZeroVector) {
                        elbowAxisVector = _cachedElbowAxis;
                    } else {
                        elbowAxisVector = rightShoulderAxisVector;
                        _cachedElbowAxis = elbowAxisVector;
                    }
                } else {
                    _cachedElbowAxis = elbowAxisVector;
                }

                if (_printIntermediateValues) {
                    Logging.PrintQty("HEAD_POSITION", _sensorReadings.Data.HeadsetPosition);
                    Logging.PrintQty("NECK_BASE_POSITION", neckBasePosition, "m");
                    Logging.PrintQty("HAND_POSITION", _sensorReadings.Data.RightControllerPosition, "m");

                    Logging.PrintQty("NECK_BASE_TO_HAND", neckBaseToHand, "m");
                    Logging.PrintQty("SHOULDER_TO_HAND_LENGTH", shoulderToHandLength, "m");
                    Logging.PrintQty("NECK_BASE_HAND_NECK_BASE_SHOULDER_ANGLE", 
                        Units.RadiansToDegrees((float)Math.Acos(cos_NeckBaseHand_neckBaseShoulder_Angle)), "deg");
                    Logging.PrintQty("NECK_BASE_TO_SHOULDER", neckBaseToShoulder, "m");
                    Logging.PrintQty("UPPER_ARM_VECTOR", upperArmVector, " m");
                }
            }
            
            private float _upperArmLength;
            private float _lowerArmLength;
            private float _shoulderDistFromNeckBase;
            private Vector3 _neckBaseOffsetFromHeadset;
            private bool _useDummySensorReadings;
            private bool _printIntermediateValues;
            private Vector3 _prevNeckBaseToShoulder;
            private Vector3 _prevShoulderToElbow;
            private SensorReadings _sensorReadings;
            private Vector3 _cachedElbowAxis;
            private Vector3 _zeroVector;
        }
    }
}
