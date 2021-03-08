using UnityEngine;

namespace FYDP {
    namespace ArmBrace {
        public struct SensorData {
            public float ElbowDeg {get; private set;}
            public float ShoulderAbductionDeg {get; private set;} 
            public float ShoulderFlexionDeg {get; private set;}
            public Quaternion HeadsetRotation {get; private set;}
            public Vector3 HeadsetPosition {get; private set;}
            public Vector3 RightControllerPosition {get; private set;}
            public void OverwriteElbowDeg(float elbowDeg) {
                ElbowDeg = elbowDeg;
            }
            public void OverwriteShoulderAbductionDeg(float shoulderAbductionDeg) {
                ShoulderAbductionDeg = shoulderAbductionDeg;
            }
            public void OverwriteShoulderFlexionDeg(float shoulderFlexionDeg) {
                ShoulderFlexionDeg = shoulderFlexionDeg;
            }
            public void OverwriteHeadsetRotation(Quaternion headsetRotation) {
                HeadsetRotation = headsetRotation;
            }
            public void OverwriteHeadsetPosition(Vector3 headsetPosition) {
                HeadsetPosition = headsetPosition;
            }
            public void OverwriteRightControllerPosition(Vector3 rightControllerPosition) {
                RightControllerPosition = rightControllerPosition;
            }
        }
    }
}
