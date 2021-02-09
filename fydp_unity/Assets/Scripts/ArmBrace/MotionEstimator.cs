using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;

namespace FYDP {
    namespace ArmBrace {
        class MotionEstimator {
            private Vector3[] _positionMemory;
            private int _currIndex;
            private int _positionMemoryLength;
            private float _timestepSeconds;

            public MotionEstimator(float timestepSeconds) {
                _positionMemoryLength = 3;
                _positionMemory = new Vector3[_positionMemoryLength];
                _currIndex = 0;
                _timestepSeconds = timestepSeconds;
            }
            public void UpdateNewPosition(Vector3 position){
                _currIndex = (_currIndex + 1) % _positionMemoryLength;
                _positionMemory[_currIndex] = position;
            }
            // If no input, assume the arm has stayed stationary. Add a 
            // kinematic model if better performance is needed.
            public void EstimateUnobtainableNewPosition(){
                UpdateNewPosition(_positionMemory[_currIndex]);
            }

            // Just a backwards finite difference equation. Use more advanced 
            // techniques if necessary.
            public Vector3 EstimateAcceleration(){
                return (_positionMemory[_currIndex] - 
                    2*_positionMemory[(_currIndex - 1) % _positionMemoryLength] +
                    _positionMemory[(_currIndex - 2) % _positionMemoryLength]) /
                    _timestepSeconds/_timestepSeconds;
            }

            public Vector3 EstimateVelocity(){
                return (_positionMemory[_currIndex] - 
                    _positionMemory[(_currIndex - 1) % _positionMemoryLength])/
                    _timestepSeconds;
            }

        }
    }
}