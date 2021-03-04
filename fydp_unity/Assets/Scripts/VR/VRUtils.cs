using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace FYDP {
    namespace VR {
        public class VRUtils {
            public enum DeviceId {
                Headset = (int)InputDeviceCharacteristics.HeadMounted,
                LeftController = (int)(InputDeviceCharacteristics.Controller | 
                        InputDeviceCharacteristics.TrackedDevice | 
                        InputDeviceCharacteristics.Right),
                RightController  = (int)(InputDeviceCharacteristics.Controller | 
                        InputDeviceCharacteristics.TrackedDevice | 
                        InputDeviceCharacteristics.Left)
            }

            public static bool TryGetInputDevice(
                DeviceId deviceId, out InputDevice device){
                
                List<InputDevice> deviceList = new List<InputDevice>();
                InputDevices.GetDevicesWithCharacteristics(
                    (InputDeviceCharacteristics)deviceId, deviceList);

                if (deviceList.Count > 0) {
                    if (deviceList.Count > 1) {
                        Debug.Log("Expected only one matching input device, " + 
                              "received " + deviceList.Count.ToString());              
                    }
                    device = deviceList[0];
                    return true;
                }
                device = new InputDevice();
                return false;
            } 
        }
    }
}