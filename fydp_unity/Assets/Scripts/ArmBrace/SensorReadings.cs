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
                TimeSpan dataRelevanceLifetime,
                SensorData dummyData = new SensorData()) {
                
                _dummyData = dummyData;
                
                _braceSensorReader = braceSensorReader;
                _useDummySensorReadings = false;
                _currDataTimeStamp = DateTime.MinValue;
                _dataRelevanceLifetime = dataRelevanceLifetime;
            }
            
            // Initializes the data to dummy values. Edit this function
            // for custom dummy values.
            // TODO: Allow passing in custom dummy values for testing.
            private void InitDummyReadings() {
                Data = _dummyData;
                _currDataTimeStamp = DateTime.MaxValue;
            }

            // Sets the class to use dummy sensor readings 
            public void UseDummySensorReadings (
                bool useDummySensorReadings) {
                
                _useDummySensorReadings = useDummySensorReadings;
                if (_useDummySensorReadings) {
                    InitDummyReadings();
                }
            }

            public bool TryInitSensors() {
                if(_useDummySensorReadings) {
                    return true;
                }
                bool initializedAllSensors = true;

                if(!VRUtils.TryGetInputDevice(
                    VRUtils.DeviceId.Headset, out _headset)) {

                    Debug.Log("Could not access headset.");
                    initializedAllSensors = false;
                }
                if(!VRUtils.TryGetInputDevice(
                    VRUtils.DeviceId.RightController, 
                    out _rightController)) {
                    
                    Debug.Log("Could not access right hand controller.");
                    initializedAllSensors = false;
                }
                _braceSensorReader.StartAsyncSensorReads();
                return initializedAllSensors;
            }

            // Tries to grab the updated sensor readings. If any sensors 
            // reads fail, reuse previous values if not stale.
            public bool Update() {
                if(_useDummySensorReadings) {
                    return true;
                }
                bool readFromAllSensors = true;

                Quaternion tempHeadsetRotation;
                Vector3 tempHeadsetPosition;   

                if (!_headset.TryGetFeatureValue(
                        CommonUsages.deviceRotation, 
                        out tempHeadsetRotation) || 
                    !_headset.TryGetFeatureValue(
                        CommonUsages.devicePosition, 
                        out tempHeadsetPosition)) {

                    Debug.Log("Could not read from headset sensors.");
                    
                    readFromAllSensors =  false;
                } else {
                    Data.OverwriteHeadsetRotation(tempHeadsetRotation);
                    Data.OverwriteHeadsetPosition(tempHeadsetPosition);
                }

                Vector3 tempRightControllerPosition;
                float tempRightControllerTrigger;
                if (!_rightController.TryGetFeatureValue(
                        CommonUsages.devicePosition, 
                        out tempRightControllerPosition) || 
                    !_rightController.TryGetFeatureValue(
                        CommonUsages.trigger, 
                        out tempRightControllerTrigger)) {

                    Debug.Log("Could not read from right controller sensors.");
                    
                    // Assume button is not pressed if can't get the controller.
                    Data.OverwriteRightControllerTrigger(0);
                    readFromAllSensors =  false;
                } else {
                    Data.OverwriteRightControllerPosition(tempRightControllerPosition);
                    Data.OverwriteRightControllerTrigger(tempRightControllerTrigger);
                }

                float tempElbowDeg; 
                float tempShoulderAbductionDeg; 
                float tempShoulderFlexionDeg;
                
                // if(!_braceSensorReader.GetJointAngles(
                //     out tempElbowDeg, out tempShoulderAbductionDeg, 
                //     out tempShoulderFlexionDeg)) {

                //     readFromAllSensors =  false;
                // } else {
                //     Data.OverwriteElbowDeg(tempElbowDeg);
                //     Data.OverwriteShoulderAbductionDeg(tempShoulderAbductionDeg);
                //     Data.OverwriteShoulderFlexionDeg(tempShoulderFlexionDeg);
                // }

                Data.OverwriteShoulderAbductionDeg(0);
                Data.OverwriteShoulderFlexionDeg(0);

                if(readFromAllSensors) {
                    _currDataTimeStamp = DateTime.Now;
                } else if ((DateTime.Now - _currDataTimeStamp) < _dataRelevanceLifetime) {
                    return true;
                }

                return readFromAllSensors;
            }

            public SensorData Data;
            public SensorData _dummyData;

            private bool _useDummySensorReadings;
            private DateTime _currDataTimeStamp;
            private TimeSpan _dataRelevanceLifetime;
            private InputDevice _headset;
            private InputDevice _rightController;
            private BraceSensorReader _braceSensorReader;
        }
    }
}
