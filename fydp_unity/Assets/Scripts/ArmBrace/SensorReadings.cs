using UnityEngine;
using UnityEngine.XR;
using FYDP.VR;
using FYDP.Utils;
using System;

namespace FYDP {
    namespace ArmBrace {
        // Caches sensor readings between reads
        public class SensorReadings {
            
            public SensorReadings(
                BraceSensorReader braceSensorReader,
                TimeSpan dataRelevanceLifetime) {
                
                Data = new SensorData(1);
                _braceSensorReader = braceSensorReader;
                _currDataTimeStamp = DateTime.MinValue;
                _dataRelevanceLifetime = dataRelevanceLifetime;
            }

            public void ReleaseResources() {
                _braceSensorReader.StopAsyncSensorReads();
            }
            

            public bool TryInitSensors(float upperArmLength, Vector3 neckBaseOffsetFromHeadset, float shoulderDistFromNeckBase, float imuSensorMsgFreq) {
                bool initializedAllSensors = true;

                bool gotHeadset = VRUtils.TryGetInputDevice(
                    VRUtils.DeviceId.Headset, out _headset); 
                
                if(!gotHeadset || !_headset.isValid) {

                    Debug.Log("Could not access headset.");
                    initializedAllSensors = false;
                }

                bool gotRightController = VRUtils.TryGetInputDevice(
                    VRUtils.DeviceId.RightController, out _rightController);

                if(!gotRightController || !_rightController.isValid) {
                    
                    Debug.Log("Could not access right hand controller.");
                    initializedAllSensors = false;
                }

                bool gotLeftController = VRUtils.TryGetInputDevice(
                    VRUtils.DeviceId.LeftController, out _leftController);

                if(!gotLeftController || !_leftController.isValid) {
                    
                    Debug.Log("Could not access left hand controller.");
                    initializedAllSensors = false;
                }

                Quaternion headsetRotation = new Quaternion();
                Vector3 headsetPosition = new Vector3();   
                if(!_headset.TryGetFeatureValue(
                        CommonUsages.devicePosition, 
                        out headsetPosition) ||
                    !_headset.TryGetFeatureValue(
                        CommonUsages.deviceRotation, 
                        out headsetRotation)) {
                    Debug.Log("Could not read from headset sensors.");
                    initializedAllSensors = false;
                }

                Vector3 neckBase = (headsetRotation * neckBaseOffsetFromHeadset) + headsetPosition;
                Vector3 shoulderPosition = neckBase + headsetRotation * (shoulderDistFromNeckBase * Vector3.right);

                Logging.PrintQtyVector3("HEADSET_ROTATION_IMU", headsetRotation.eulerAngles, "deg");
                Logging.PrintQtyVector3("HEADSET_POSITION_IMU", headsetPosition, "m"); 
                Logging.PrintQtyVector3("NECK_BASE_IMU", neckBase, "m");
                Logging.PrintQtyVector3("SHOULDER_POSITION_IMU", shoulderPosition, "m");

                _braceSensorReader.SetImuEstimator(new ImuEstimator(
                    initialPosition: shoulderPosition + Vector3.down * upperArmLength,
                    globalToInitialCorrected: headsetRotation,
                    timestepSeconds:imuSensorMsgFreq));
                _braceSensorReader.StartAsyncSensorReads();
                return initializedAllSensors;
            }

            // Tries to grab the updated sensor readings. If any sensors 
            // reads fail, reuse previous values if not stale.
            public bool Update() {
                bool readFromAllSensors = true;

                Quaternion tempHeadsetRotation = new Quaternion();
                Vector3 tempHeadsetPosition = new Vector3();   
                
                if (!_headset.TryGetFeatureValue(
                        CommonUsages.deviceRotation, 
                        out tempHeadsetRotation) || 
                    !_headset.TryGetFeatureValue(
                        CommonUsages.devicePosition, 
                        out tempHeadsetPosition)) {

                    Debug.Log("Could not read from headset sensors.");
                    
                    readFromAllSensors =  false;
                } else {
                    Data.RecordHeadsetRotation(tempHeadsetRotation);
                    Data.RecordHeadsetPosition(tempHeadsetPosition);
                }

                Vector3 tempRightControllerPosition = new Vector3();
                Vector3 tempRightControllerVelocity = new Vector3();
                bool tempRightControllerSecondaryButtonPressed;
                if (!_rightController.TryGetFeatureValue(
                        CommonUsages.devicePosition, 
                        out tempRightControllerPosition) || 
                    !_rightController.TryGetFeatureValue(
                        CommonUsages.secondaryButton, 
                        out tempRightControllerSecondaryButtonPressed) ||
                    !_rightController.TryGetFeatureValue(
                        CommonUsages.deviceVelocity, 
                        out tempRightControllerVelocity)) {

                    Debug.Log("Could not read from right controller sensors.");
                    
                    Data.RecordRightControllerSecondaryButtonPressed(false);
                    readFromAllSensors =  false;
                } else {
                    Data.RecordRightControllerPosition(tempRightControllerPosition);
                    Data.RecordRightControllerVelocity(tempRightControllerVelocity);
                    Data.RecordRightControllerSecondaryButtonPressed(tempRightControllerSecondaryButtonPressed);
                }

                Vector3 tempLeftControllerPosition = new Vector3();
                if (!_leftController.TryGetFeatureValue(
                        CommonUsages.devicePosition, 
                        out tempLeftControllerPosition)) {

                    Debug.Log("Could not read from left controller sensors.");
                    
                    readFromAllSensors =  false;
                } else {
                    Data.RecordLeftControllerPosition(tempLeftControllerPosition);
                }

                float tempElbowDeg;  
                Vector3 tempPositionEstimate; 
                Vector3 tempElbowAxisEstimate;
                Vector3 tempUpperArmAxisEstimate;

                if(!_braceSensorReader.GetBraceSensorData(out tempElbowDeg, 
                out tempPositionEstimate, out tempElbowAxisEstimate,
                out tempUpperArmAxisEstimate)) {

                    readFromAllSensors =  false;
                } else {
                    Data.RecordElbowDeg(tempElbowDeg);
                }

                Data.RecordElbowPositionEstimate(tempPositionEstimate);
                Data.RecordElbowAxisEstimate(tempElbowAxisEstimate);
                Data.RecordUpperArmAxisEstimate(tempUpperArmAxisEstimate);

                if(readFromAllSensors) {
                    _currDataTimeStamp = DateTime.Now;
                } else if ((DateTime.Now - _currDataTimeStamp) < _dataRelevanceLifetime) {
                    return true;
                }

                return readFromAllSensors;
            }

            public SensorData Data;

            private DateTime _currDataTimeStamp;
            private TimeSpan _dataRelevanceLifetime;
            private InputDevice _headset;
            private InputDevice _rightController;
            private InputDevice _leftController;
            private BraceSensorReader _braceSensorReader;
        }
    }
}
