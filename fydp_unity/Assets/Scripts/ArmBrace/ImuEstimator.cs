using UnityEngine;
using FYDP.VR;
using FYDP.Utils;
using System;



namespace FYDP {
    namespace ArmBrace {
        public class ImuEstimator {
            
            public Vector3 PositionEstimate {get; private set;}
            private Vector3  _initialPosition;
            public Vector3 ElbowAxisEstimate {get; private set;}
            public Vector3 UpperArmAxisEstimate {get; private set;}
            private Quaternion _initialCorrectedToInitialUncorrected;
            private Quaternion _initialCorrectedToGlobal;
            private Quaternion _globalToInitialCorrected;
            private Quaternion _initialUncorrectedToCurrent;
            
            private Vector3 _initialElbowAxis;
            private Vector3 _initialUpperArmAxis;

            private Quaternion _initialCorrectedToCurrent;
            private Vector3 globalCoordsAcceleration;

            private Vector3 acceleration;
            private Vector3 xyzAnglularVelocity;
            private bool newDataAvailable;
            public bool needToInitInitialRotation {get; private set;}
            
            private Vector3 RhsToLhsCoordinates(Vector3 rhsCoordinateVector){

                return new Vector3(rhsCoordinateVector.x, rhsCoordinateVector.y, -rhsCoordinateVector.z);
            }

            public ImuEstimator(Vector3 initialPosition, Quaternion globalToInitialCorrected, float timestepSeconds) {
                _angularVelocities = new MotionIntegratorVector3(timestepSeconds, needSecondIntegral:false);
                _accelerations = new MotionIntegratorVector3(timestepSeconds);
                _initialPosition = initialPosition;
                
                PositionEstimate = initialPosition;
                ElbowAxisEstimate = globalToInitialCorrected * Vector3.right;
                UpperArmAxisEstimate = globalToInitialCorrected * Vector3.down;

                needToInitInitialRotation = true; 
                
                _initialCorrectedToGlobal = Quaternion.Inverse(globalToInitialCorrected);
                _globalToInitialCorrected = globalToInitialCorrected;
                
                _initialElbowAxis = _globalToInitialCorrected * Vector3.right;
                _initialUpperArmAxis = _globalToInitialCorrected * Vector3.down;
                _initialUncorrectedToCurrent = new Quaternion();
                newDataAvailable = false;
            }

            public void getNewImuData(Vector3 newAcceleration, Vector3 newXyzAnglularVelocity) {
                acceleration = newAcceleration;
                xyzAnglularVelocity = newXyzAnglularVelocity;
                newDataAvailable = true;
            }

            public void UpdateEstimates() {
                if (newDataAvailable) {
                    _angularVelocities.UpdateNewRate(RhsToLhsCoordinates(xyzAnglularVelocity));

                    _initialUncorrectedToCurrent.eulerAngles = _angularVelocities.FirstIntegral(out bool valid);

                    if (!valid) {
                        newDataAvailable = false;
                        return;    
                    } else if (needToInitInitialRotation) {
                        _initialCorrectedToInitialUncorrected = Quaternion.Inverse(_initialUncorrectedToCurrent);
                        needToInitInitialRotation = false;
                    }
                    //Relative rotation algorithm source: https://math.stackexchange.com/questions/615891/relative-rotation-between-quaternions
                    //Note: Seems to contradict which should be inverted https://stackoverflow.com/questions/57020163/how-to-get-the-relative-rotation-of-2-quaternions-in-unity 
                    //I need to learn more about Quaternions, clearly
                    _initialCorrectedToCurrent = _initialCorrectedToInitialUncorrected * _initialUncorrectedToCurrent;
                    
                    ElbowAxisEstimate = _initialCorrectedToCurrent * _initialElbowAxis;
                    UpperArmAxisEstimate = _initialCorrectedToCurrent * _initialUpperArmAxis;
                    globalCoordsAcceleration =  Quaternion.Inverse(_globalToInitialCorrected) * Quaternion.Inverse(_initialCorrectedToCurrent) * RhsToLhsCoordinates(acceleration);
                    
                    //Subtract gravity
                    globalCoordsAcceleration.y -= 9.81f;

                    _accelerations.UpdateNewRate(globalCoordsAcceleration);

                    Vector3 displacement = _accelerations.SecondIntegral(out valid);
                    if (!valid) {
                        newDataAvailable = false;
                        return;    
                    }

                    PositionEstimate = displacement + _initialPosition;
                    newDataAvailable = false;
                } 
            }

            private MotionIntegratorVector3 _angularVelocities;
            private MotionIntegratorVector3 _accelerations;            
        }
    }
}