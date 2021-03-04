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
            //Debugging Only
            private bool _debug = false; 

            public int _elbowDeg;
            public int _shoulderAbductionDeg; 
            public int _shoulderFlexionDeg;

            public Quaternion _headsetRotation;
            public Vector3 _headsetPosition;

            public ArmVectorModel(BraceSensorReader braceSensorReader,
                float upperArmLength, float lowerArmLength, 
                OffsetPolarVector shoulderOffsetFromNeckBase, 
                OffsetPolarVector neckBaseOffsetFromHeadset, bool debug=false) {
                
                _braceSensorReader = braceSensorReader;
                _upperArmLength = upperArmLength;
                _lowerArmLength = lowerArmLength;

                _shoulderOffsetFromNeckBase = shoulderOffsetFromNeckBase;
                _neckBaseOffsetFromHeadset = neckBaseOffsetFromHeadset;

                _debug = debug;
                if(!_debug) {
                    if(!VRUtils.TryGetInputDevice(
                        VRUtils.DeviceId.Headset, out _headset)) {
                        
                        Debug.Log("Could not access headset.");
                    }

                    _braceSensorReader.StartAsyncSensorReads();
                }
            }

            public bool CalculateJointTorques(Vector3 forceAtHand, 
                Vector3 rightControllerLocation, 
                out float elbowTorque, out float shoulderAbductionTorque, 
                out float shoulderFlexionTorque) {
                
                elbowTorque = 0;
                shoulderAbductionTorque = 0;
                shoulderFlexionTorque = 0;

                float elbowDeg = _elbowDeg;
                float shoulderAbductionDeg = _shoulderAbductionDeg; 
                float shoulderFlexionDeg = _shoulderFlexionDeg;

                bool couldCalcShoulderJoint = EstimateShoulderJointCoordinates(
                    out Vector3 shoulderLocation, 
                    out Vector3 rightShoulderAxisVector, 
                    out Vector3 forwardAxisVector);

                if(!_debug){
                    bool couldGetAngles = _braceSensorReader.GetJointAngles(
                        out elbowDeg, out shoulderAbductionDeg, 
                        out shoulderFlexionDeg);
                    
                    if (!couldGetAngles || !couldCalcShoulderJoint) {
                        Debug.Log("Could not read from the devices.");
                        return false;
                    } 
                }      
                Debug.Log("ELBOW_DEG: " + elbowDeg.ToString());
                Debug.Log("SHOULDER_ABDUCTION_DEG: " + shoulderAbductionDeg.ToString());
                Debug.Log("SHOULDER_FLEXION_DEG: " + shoulderFlexionDeg.ToString());
                Vector3 shoulderMomentArm = rightControllerLocation -  
                                            shoulderLocation;

                Vector3 shoulderMoment = Vector3.Cross(shoulderMomentArm, 
                                                       forceAtHand);
                
                shoulderAbductionTorque = Vector3.Dot(
                    shoulderMoment, forwardAxisVector);

                shoulderFlexionTorque = Vector3.Dot(
                    shoulderMoment, rightShoulderAxisVector);
                
                Vector3 torsoUpUnitVector = Vector3.Cross(
                    forwardAxisVector, rightShoulderAxisVector).normalized;

                Vector3 upperArmVector = 
                    Quaternion.AngleAxis(shoulderFlexionDeg, 
                        rightShoulderAxisVector) *
                    Quaternion.AngleAxis(shoulderAbductionDeg, 
                        forwardAxisVector) * 
                    (_upperArmLength * rightShoulderAxisVector);
                Debug.Log("upperArmVector: " + upperArmVector.ToString());

                Vector3 elbowLocation = upperArmVector + shoulderLocation;
                
                // Use saved arm length instead of controller location 
                // directly.
                Vector3 lowerArmVector = 
                    _lowerArmLength * 
                    (rightControllerLocation - elbowLocation).normalized;
                
                Debug.Log("lowerArmVector: " + lowerArmVector.ToString());

                Vector3 elbowAxisVector = Vector3.Cross(
                    lowerArmVector, upperArmVector).normalized;
                
                Debug.Log("elbowAxisVector: " + elbowAxisVector.ToString());

                elbowTorque = Vector3.Dot(
                    Vector3.Cross(lowerArmVector, forceAtHand), 
                    elbowAxisVector);

                if(_debug) {
                    /*Debug.Log("shoulderMomentArm: " + 
                        shoulderMomentArm.ToString());
                    Debug.Log("shoulderMoment: " + 
                        shoulderMoment.ToString());
                    Debug.Log("shoulderAbductionTorque: " + 
                        shoulderAbductionTorque.ToString());
                    Debug.Log("shoulderFlexionTorque: " + 
                        shoulderFlexionTorque.ToString());
                    Debug.Log("torsoUpUnitVector: " + 
                        torsoUpUnitVector.ToString());
                    Debug.Log("upperArmVector: " + 
                        upperArmVector.ToString());
                    Debug.Log("elbowLocation: " + 
                        elbowLocation.ToString());
                    Debug.Log("lowerArmVector: " + 
                        lowerArmVector.ToString());
                    Debug.Log("elbowAxisVector: " + 
                        elbowAxisVector.ToString());
                    Debug.Log("elbowTorque" + 
                        elbowTorque.ToString());*/
                }
                //Translate to right-hand moment vector convention.
                shoulderAbductionTorque *= -1;
                shoulderFlexionTorque *= -1;
                elbowTorque *=-1;
                return true;
            }

            //Need to figure out what vector the headset rotation is relative to
            private bool EstimateShoulderJointCoordinates(
                out Vector3 shoulderLocation,
                out Vector3 rightShoulderAxisVector, 
                out Vector3 forwardAxisVector) {
                
                Quaternion headsetRotation = _headsetRotation;
                Vector3 headsetPosition = _headsetPosition;

                if (!_debug && 
                    (!_headset.TryGetFeatureValue(
                        CommonUsages.deviceRotation, 
                        out headsetRotation) || 
                    !_headset.TryGetFeatureValue(
                        CommonUsages.devicePosition, 
                        out headsetPosition))) {
                        
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

                Vector3 headsetToNeckBase = headsetRotation * 
                    _neckBaseOffsetFromHeadset.Rotation * 
                    (_neckBaseOffsetFromHeadset.Length * 
                    Vector3.forward);

                Vector3 neckBaseLocation = headsetToNeckBase + headsetPosition;

                // Similar process, but for the shoulder joint location.
                // Assumes shoulder joint doesn't move significantly wrt
                // the base ofthe neck.

                shoulderLocation = _shoulderOffsetFromNeckBase.Rotation *
                (_shoulderOffsetFromNeckBase.Length * 
                headsetToNeckBase.normalized) + neckBaseLocation;

                rightShoulderAxisVector = (shoulderLocation - 
                    neckBaseLocation).normalized;

                // Assumes the user never bends over or backwards 
                // significantly. Note that Unity uses a left-hand cross 
                // product. Assume we're on the right shoulder. Note, not 
                // normalized!
                forwardAxisVector = Vector3.Cross(
                    rightShoulderAxisVector, Vector3.up).normalized;
                if(_debug) {
                    /*
                    Debug.Log("headsetToNeckBase: " + 
                        headsetToNeckBase.ToString());
                    Debug.Log("neckBaseLocation: " + 
                        neckBaseLocation.ToString());
                    Debug.Log("shoulderLocation: " + 
                        shoulderLocation.ToString());
                    Debug.Log("rightShoulderAxisVector: " + 
                        rightShoulderAxisVector.ToString());
                    Debug.Log("forwardAxisVector: " + 
                        forwardAxisVector.ToString());
                    */
                }
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
