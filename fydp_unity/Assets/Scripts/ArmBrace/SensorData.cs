using UnityEngine;

namespace FYDP {
    namespace ArmBrace {
        public struct SensorData {
            private MovingAvg<float> _elbowDegs;
            private MovingAvg<float> _shoulderAbductionDegs;
            private MovingAvg<float> _shoulderFlexionDegs;
            private MovingAvg<Vector3> _headsetPositions;
            private MovingAvg<Vector3> _rightControllerPositions;

            public float ElbowDeg {get {return _elbowDegs.Avg;} private set;}
            public float ShoulderAbductionDeg {get {return _shoulderAbductionDegs.Avg;} private set;}
            public float ShoulderFlexionDeg {get {return _shoulderFlexionDegs.Avg;} private set;}
            public Quaternion HeadsetRotation {get; private set;}
            public Vector3 HeadsetPosition {get {return _headsetPositions.Avg;} private set;}
            public Vector3 RightControllerPosition {get {return _rightControllerPositions.Avg;} private set;}
            public bool RightControllerSecondaryButtonPressed {get; private set;}
            
            public SensorData(int windowSize) {
                _elbowDegs = new MovingAvg<float>(windowSize);
                _shoulderAbductionDegs = new MovingAvg<float>(windowSize);
                _shoulderFlexionDegs = new MovingAvg<float>(windowSize);
                _headsetPositions = new MovingAvg<Vector3>(windowSize);
                _rightControllerPositions = new MovingAvg<Vector3>(windowSize);
            }
            public bool MovingAvgsFilled() {
                return _elbowDegs.Filled() && _shoulderAbductionDegs.Filled() && 
                    _shoulderFlexionDegs.Filled() && _headsetPositions.Filled() &&
                    _rightControllerPositions.Filled();
            }
            public void RecordElbowDeg(float elbowDeg) {
                _elbowDegs.AddValue(elbowDeg);
            }
            public void RecordShoulderAbductionDeg(float shoulderAbductionDeg) {
                _shoulderAbductionDegs.AddValue(shoulderAbductionDeg);
            }
            public void RecordShoulderFlexionDeg(float shoulderFlexionDeg) {
                _shoulderFlexionDegs.AddValue(shoulderFlexionDeg);
            }
            public void RecordHeadsetRotation(Quaternion headsetRotation) {
                HeadsetRotation = headsetRotation;
            }
            public void RecordHeadsetPosition(Vector3 headsetPosition) {
                _headsetPositions.AddValue(headsetPosition);
            }
            public void RecordRightControllerPosition(Vector3 rightControllerPosition) {
                _rightControllerPositions.AddValue(rightControllerPosition);
            }
            public void RecordRightControllerSecondaryButtonPressed(bool rightControllerSecondaryButtonPressed) {
                RightControllerSecondaryButtonPressed = rightControllerSecondaryButtonPressed;
            }
        }
    }
}
