using UnityEngine;

namespace FYDP {
    namespace ArmBrace {
        public struct SensorData {
            private MovingAvgFloat _elbowDegs;
            private MovingAvgVector3 _headsetPositions;
            private MovingAvgVector3 _rightControllerPositions;
            private MovingAvgVector3 _rightControllerVelocities;
            private MovingAvgVector3 _leftControllerPositions;
            private MovingAvgVector3 _elbowPositionEstimates;
            private MovingAvgVector3 _elbowAxisEstimates;
            private MovingAvgVector3 _upperArmAxisEstimates;

            public float ElbowDeg {get {return _elbowDegs.Avg;} private set{}}
            public Quaternion HeadsetRotation {get; private set;}
            public Vector3 HeadsetPosition {get {return _headsetPositions.Avg;} private set{}}
            public Vector3 RightControllerPosition {get {return _rightControllerPositions.Avg;} private set{}}
            public Vector3 RightControllerVelocity {get {return _rightControllerVelocities.Avg;} private set{}}
            public Vector3 LeftControllerPosition {get {return _leftControllerPositions.Avg;} private set{}}
            public Vector3 ElbowPositionEstimate {get {return _elbowPositionEstimates.Avg;} private set{}}
            public Vector3 ElbowAxisEstimate {get {return _elbowAxisEstimates.Avg.normalized;} private set{}}
            public Vector3 UpperArmAxisEstimate {get {return _upperArmAxisEstimates.Avg.normalized;} private set{}}

            public bool RightControllerSecondaryButtonPressed {get; private set;}
            
            public SensorData(int windowSize) {
                HeadsetRotation = new Quaternion();
                RightControllerSecondaryButtonPressed = false;
                _elbowDegs = new MovingAvgFloat(windowSize);
                _headsetPositions = new MovingAvgVector3(windowSize);
                _rightControllerPositions = new MovingAvgVector3(windowSize);
                _rightControllerVelocities = new MovingAvgVector3(windowSize);
                _leftControllerPositions = new MovingAvgVector3(windowSize);
                _elbowPositionEstimates = new MovingAvgVector3(windowSize);
                _elbowAxisEstimates = new MovingAvgVector3(windowSize);
                _upperArmAxisEstimates = new MovingAvgVector3(windowSize);
            }
            public bool MovingAvgsFilled() {
                return _elbowDegs.Filled() && _headsetPositions.Filled() && 
                    _rightControllerPositions.Filled() && _rightControllerVelocities.Filled() &&
                    _leftControllerPositions.Filled() && _elbowPositionEstimates.Filled() && 
                    _elbowAxisEstimates.Filled() && _upperArmAxisEstimates.Filled();
            }
            public void RecordElbowDeg(float elbowDeg) {
                _elbowDegs.AddValue(elbowDeg);
            }
            public void RecordElbowPositionEstimate(Vector3 elbowPositionEstimate) {
                _elbowPositionEstimates.AddValue(elbowPositionEstimate);
            }
            public void RecordElbowAxisEstimate(Vector3 elbowAxisEstimates) {
                _elbowAxisEstimates.AddValue(elbowAxisEstimates);
            }
            public void RecordUpperArmAxisEstimate(Vector3 upperArmAxisEstimates) {
                _upperArmAxisEstimates.AddValue(upperArmAxisEstimates);
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
