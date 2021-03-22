using UnityEngine;

namespace FYDP {
    namespace ArmBrace {
        public struct SensorData {
            private MovingAvgFloat _elbowDegs;
            private MovingAvgFloat _shoulderAbductionDegs;
            private MovingAvgFloat _shoulderFlexionDegs;
            private MovingAvgVector3 _headsetPositions;
            private MovingAvgVector3 _rightControllerPositions;
            private MovingAvgVector3 _rightControllerVelocities;
            private MovingAvgVector3 _leftControllerPositions;

            public float ElbowDeg {get {return _elbowDegs.Avg;} private set{}}
            public float ShoulderAbductionDeg {get {return 0;} private set{}}
            public float ShoulderFlexionDeg {get {return 0;} private set{}}
            public Quaternion HeadsetRotation {get; private set;}
            public Vector3 HeadsetPosition {get {return _headsetPositions.Avg;} private set{}}
            public Vector3 RightControllerPosition {get {return _rightControllerPositions.Avg;} private set{}}
            public Vector3 RightControllerVelocity {get {return _rightControllerVelocities.Avg;} private set{}}
            public Vector3 LeftControllerPosition {get {return _leftControllerPositions.Avg;} private set{}}
            public bool RightControllerSecondaryButtonPressed {get; private set;}
            
            public SensorData(int windowSize) {
                HeadsetRotation = new Quaternion();
                RightControllerSecondaryButtonPressed = false;
                _elbowDegs = new MovingAvgFloat(windowSize);
                _shoulderAbductionDegs = new MovingAvgFloat(windowSize);
                _shoulderFlexionDegs = new MovingAvgFloat(windowSize);
                _headsetPositions = new MovingAvgVector3(windowSize);
                _rightControllerPositions = new MovingAvgVector3(windowSize);
                _rightControllerVelocities = new MovingAvgVector3(windowSize);
                _leftControllerPositions = new MovingAvgVector3(windowSize);
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
            public void RecordRightControllerVelocity(Vector3 rightControllerVelocity) {
                _rightControllerVelocities.AddValue(rightControllerVelocity);
            }
            public void RecordLeftControllerPosition(Vector3 leftControllerPosition) {
                _leftControllerPositions.AddValue(leftControllerPosition);
            }
            public void RecordRightControllerSecondaryButtonPressed(bool rightControllerSecondaryButtonPressed) {
                RightControllerSecondaryButtonPressed = rightControllerSecondaryButtonPressed;
            }
        }
    }
}
