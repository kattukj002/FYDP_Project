using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class HandPresence : MonoBehaviour
{
    public bool showController = false;
    public InputDeviceCharacteristics controllerCharacteristics;
    public List<GameObject> controllerPrefabs;
    public GameObject handModelPrefab;

    private XRDirectInteractor interactor;

    //Define the Pose Class
    public class Pose
    {
        private UnityEngine.Vector3 position;
        private UnityEngine.Quaternion rotation;

        //constructor
        public Pose(UnityEngine.Vector3 position, UnityEngine.Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
        
        public UnityEngine.Vector3 getPosition()
        {
            return this.position;
        }

        public UnityEngine.Quaternion getRotation()
        {
            return this.rotation;
        }

        public void logPoseData()
        {
            Debug.Log("Position: " + this.position);
            //Debug.Log("Rotation: " + this.rotation);
        }
    }


    private InputDevice targetDevice;
    private GameObject spawnedController;
    private GameObject spawnedHandModel;
    private Animator handAnimator;


    // Start is called before the first frame update
    void Start()
    {
        TryInitialize();
        interactor = GetComponentInParent<XRDirectInteractor>();
    }

    void UpdateHandAnimation()
    {
        //Updating Hand Animation based on Trigger Button
        if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            handAnimator.SetFloat("Trigger", triggerValue);
        }
        else
        {
            handAnimator.SetFloat("Trigger", 0);
        }


        //Updating Hand Animation based on Grip Button
        if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            handAnimator.SetFloat("Grip", gripValue);
        }
        else
        {
            handAnimator.SetFloat("Grip", 0);
        }
    }

    //Function to initialize everything
    void TryInitialize()
    {
        List<InputDevice> devices = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);

        if (devices.Count > 0)
        {

            targetDevice = devices[0];
            GameObject prefab = controllerPrefabs.Find(controller => controller.name == targetDevice.name);

            if (prefab)
            {
                spawnedController = Instantiate(prefab, transform);
            }
            else
            {
                Debug.LogError("Did not find corresponding controller model");
                spawnedController = Instantiate(controllerPrefabs[0], transform);
            }

            spawnedHandModel = Instantiate(handModelPrefab, transform);
            handAnimator = spawnedHandModel.GetComponent<Animator>();
        }
    }

    public Pose GetHeadPose()
    {
        InputDeviceCharacteristics headCharacteristics = InputDeviceCharacteristics.HeadMounted;
        List<InputDevice> head_mounted = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(headCharacteristics, head_mounted);

        if (head_mounted.Count > 0)
        {
            InputDevice head_device = head_mounted[0];
            if (head_device.TryGetFeatureValue(CommonUsages.devicePosition, out UnityEngine.Vector3 position)
                && head_device.TryGetFeatureValue(CommonUsages.deviceRotation, out UnityEngine.Quaternion rotation))
            {
                return new Pose(position, rotation);
            }
            else
            {
                Debug.LogError("Unable to get position and/or rotation of head mounted device");
                return new Pose(new UnityEngine.Vector3(0, 0, 0), new UnityEngine.Quaternion(0, 0, 0, 0));
            }
        }
        else
        {
            Debug.LogError("Unable to find a head mounted device");
            return new Pose(new UnityEngine.Vector3(0, 0, 0), new UnityEngine.Quaternion(0, 0, 0, 0));
        }
    }

    public Pose getLeftControllerPose()
    {
        InputDeviceCharacteristics leftControllerCharacteristics = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Left;
        List<InputDevice> left_controller_device  = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(leftControllerCharacteristics, left_controller_device);


        if (left_controller_device.Count > 0)
        {
            InputDevice left_controller = left_controller_device[0];
            if (left_controller.TryGetFeatureValue(CommonUsages.devicePosition, out UnityEngine.Vector3 position) &&
                left_controller.TryGetFeatureValue(CommonUsages.deviceRotation, out UnityEngine.Quaternion rotation))
            {
                return new Pose(position, rotation);
            }
            else
            {
                Debug.LogError("Unable to get position and/or rotation of left controller");
                return new Pose(new UnityEngine.Vector3(0, 0, 0), new UnityEngine.Quaternion(0, 0, 0, 0));
            }
        }
        else
        {
            Debug.LogError("Unable to find a left controller");
            return new Pose(new UnityEngine.Vector3(0, 0, 0), new UnityEngine.Quaternion(0, 0, 0, 0));
        }
    }

    public Pose getRightControllerPose()
    {
        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Right;
        List<InputDevice> right_controller_device = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, right_controller_device);

        if (right_controller_device.Count > 0)
        {
            InputDevice right_controller = right_controller_device[0];
            if (right_controller.TryGetFeatureValue(CommonUsages.devicePosition, out UnityEngine.Vector3 position) &&
                right_controller.TryGetFeatureValue(CommonUsages.deviceRotation, out UnityEngine.Quaternion rotation))
            {
                return new Pose(position, rotation);
            }
            else
            {
                Debug.LogError("Unable to get position and/or rotation of right controller");
                return new Pose(new UnityEngine.Vector3(0, 0, 0), new UnityEngine.Quaternion(0, 0, 0, 0));
            }
        }
        else
        {
            Debug.LogError("Unable to find a right controller");
            return new Pose(new UnityEngine.Vector3(0, 0, 0), new UnityEngine.Quaternion(0, 0, 0, 0));
        }
    }


    void controllerGrabCallback(XRBaseInteractable interactable)
    {

        //TODO: need to make sure the type of the interactable is UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable

        List<Collider> colliderList = interactable.colliders;
        Debug.Log("This is the list of colliders: " + colliderList[0].attachedRigidbody.mass);

    }

    // Update is called once per frame
    void Update()
    {
        //Check if there is a valid controller
        if (!targetDevice.isValid)
        {
            TryInitialize();
        }
        else
        {
            if (showController)
            {
                spawnedHandModel.SetActive(false);
                spawnedController.SetActive(true);
            }
            else
            {
                spawnedHandModel.SetActive(true);
                spawnedController.SetActive(false);
                UpdateHandAnimation();

                Pose test = getRightControllerPose();
                //test.logPoseData();


                interactor.onSelectEntered.AddListener(controllerGrabCallback);

            }
        }
    }
}
