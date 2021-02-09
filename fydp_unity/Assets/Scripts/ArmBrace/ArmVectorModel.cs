using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System.IO.Ports;
using FYDP.VR;

namespace FYDP {
    namespace ArmBrace {
        public class ArmVectorModel {
            public struct OffsetPolarVector {
                public float Length;
                public Quaternion Rotation;
            }
            public ArmVectorModel(BraceSensorReader braceSensorReader,
                float upperArmLength, float lowerArmLength, 
                OffsetPolarVector shoulderOffsetFromNeckBase, 
                OffsetPolarVector neckBaseOffsetFromHeadset) {
                
                if(!VRUtils.TryGetInputDevice(
                    VRUtils.DeviceId.Headset, out _headset)) {
                    
                    Debug.Log("Could not access headset.");
                }
                
                _braceSensorReader = braceSensorReader;
                _upperArmLength = upperArmLength;
                _lowerArmLength = lowerArmLength;

                _shoulderOffsetFromNeckBase = shoulderOffsetFromNeckBase;
                _neckBaseOffsetFromHeadset = neckBaseOffsetFromHeadset;

                _braceSensorReader.StartAsyncSensorReads();
            }
            public bool CalculateJointTorques(Vector3 forceAtHand, 
                Vector3 rightControllerLocation, 
                out float elbowTorque, out float shoulderAbductionTorque, 
                out float shoulderFlexionTorque) {
                
                elbowTorque = 0;
                shoulderAbductionTorque = 0;
                shoulderFlexionTorque = 0;
                if (_braceSensorReader.GetJointAngles(
                        out uint elbowAngleDeg, out uint shoulderAbductionDeg, 
                        out uint shoulderFlexionDeg) && 
                    EstimateShoulderJointCoordinates(
                        out Vector3 shoulderLocation, 
                        out Vector3 rightShoulderAxisVector, 
                        out Vector3 forwardAxisVector)  
                    ) {
                    
                    Vector3 shoulderMomentArm = rightControllerLocation -  
                                                shoulderLocation;

                    Vector3 shoulderMoment = Vector3.Cross(forceAtHand, 
                                                           shoulderMomentArm);
                    
                    shoulderAbductionTorque = Vector3.Project(
                        shoulderMoment, forwardAxisVector).magnitude;

                    shoulderFlexionTorque = Vector3.Project(
                        shoulderMoment, rightShoulderAxisVector).magnitude;
                    
                    Vector3 torsoUpUnitVector = Vector3.Cross(
                        forwardAxisVector, rightShoulderAxisVector).normalized;

                    Vector3 upperArmVector = 
                        Quaternion.AngleAxis(shoulderAbductionDeg, 
                            forwardAxisVector) * 
                        Quaternion.AngleAxis(shoulderFlexionDeg, 
                            rightShoulderAxisVector) * 
                        (_upperArmLength * torsoUpUnitVector);
                    
                    Vector3 elbowLocation = upperArmVector + shoulderLocation;
                    
                    // Use saved arm length instead of controller location 
                    // directly.
                    Vector3 lowerArmVector = 
                        _lowerArmLength * 
                        (rightControllerLocation - elbowLocation).normalized;
                    
                    Vector3 elbowAxisVector = Vector3.Cross(
                        lowerArmVector, upperArmVector);
                    
                    elbowTorque = Vector3.Project(
                        Vector3.Cross(forceAtHand, lowerArmVector), 
                        elbowAxisVector).magnitude;

                    return true;
                }
                return false;
            }

            //Need to figure out what vector the headset rotation is relative to
            private bool EstimateShoulderJointCoordinates(
                out Vector3 shoulderLocation,
                out Vector3 rightShoulderAxisVector, 
                out Vector3 forwardAxisVector) {

                if (!_headset.TryGetFeatureValue(
                        CommonUsages.deviceRotation, 
                        out Quaternion headsetRotation) || 
                    !_headset.TryGetFeatureValue(
                        CommonUsages.devicePosition, 
                        out Vector3 headsetPosition)) {
                    
                    shoulderLocation = Vector3.zero;
                    rightShoulderAxisVector = Vector3.zero;
                    forwardAxisVector = Vector3.zero;
                    return false;
                }

                // Projects a vector of the neck offset length forward, 
                // rotates it to align with the headset forward direction, 
                // rotates it by the relative angle of the neck base offset, 
                // then adds the headset position to put the vector into  
                // global coordinates.

                Vector3 headsetToNeckBase = _neckBaseOffsetFromHeadset.Rotation * 
                    headsetRotation *(_neckBaseOffsetFromHeadset.Length * 
                    Vector3.forward);

                Vector3 neckBaseLocation = headsetToNeckBase + headsetPosition;

                // Similar process, but for the shoulder joint location.
                // Assumes shoulder joint doesn't move significantly wrt
                // the base ofthe neck.

                shoulderLocation = _shoulderOffsetFromNeckBase.Rotation *
                (_shoulderOffsetFromNeckBase.Length * 
                headsetToNeckBase.normalized) + neckBaseLocation;

                // Subject to the same assumption. Note, not normalized!
                
                rightShoulderAxisVector = (shoulderLocation - 
                    neckBaseLocation);

                // Assumes the user never bends over or backwards 
                // significantly. Note that Unity uses a left-hand cross 
                // product. Assume we're on the right shoulder. Note, not 
                // normalized!
                forwardAxisVector = Vector3.Cross(
                    rightShoulderAxisVector, Vector3.up);

                return true;
            }
            
            private InputDevice _headset;
            private float _upperArmLength;
            private float _lowerArmLength;
            private OffsetPolarVector _shoulderOffsetFromNeckBase;
            private OffsetPolarVector _neckBaseOffsetFromHeadset;
            private BraceSensorReader _braceSensorReader;
        }
    }
}
