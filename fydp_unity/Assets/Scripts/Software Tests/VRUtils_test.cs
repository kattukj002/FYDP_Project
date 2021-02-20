using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using FYDP.VR;
using UnityEngine.XR;

public class VRUtils_test : MonoBehaviour
{
    private InputDevice _headset;
    private InputDevice _rightController;

    void Start()
    {
        VRUtils.TryGetInputDevice(VRUtils.DeviceId.Headset, out _headset);
        VRUtils.TryGetInputDevice(VRUtils.DeviceId.RightController, out _rightController);
    }

    void Update()
    {
        Vector3 outPosition;
        Quaternion outRotation;

        _headset.TryGetFeatureValue(
                CommonUsages.devicePosition, out outPosition);
        Debug.Log("Headset location: " + outPosition.ToString());

        _headset.TryGetFeatureValue(
                CommonUsages.deviceRotation, out outRotation);
        Debug.Log("Headset rotation: " + outRotation.ToString());

        _rightController.TryGetFeatureValue(
                CommonUsages.devicePosition, out outPosition);
        Debug.Log("Right controller  location: " + outPosition.ToString());

        _rightController.TryGetFeatureValue(
                CommonUsages.deviceRotation, out outRotation);
        Debug.Log("Right controller rotation: " + outRotation.ToString());
    }
}