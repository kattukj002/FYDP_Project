using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;

namespace FYDP {
    namespace ArmBrace {
        class MotionEstimatorVector3 {
            private Vector3[] _positionMemory;
            private int _currIndex;
            public const int _positionMemoryLength = 3;
            private float _timestepSeconds;
            public bool filled {get; private set;}

            public MotionEstimatorVector3(float timestepSeconds) {
                _positionMemory = new Vector3[_positionMemoryLength];
                _currIndex = 0;
                _timestepSeconds = timestepSeconds;

                for (int i = 0; i < _positionMemoryLength; i++) {
                    _positionMemory[i] = new Vector3();   
                }
             }
            public void UpdateNewPosition(Vector3 position){
                _currIndex = ((_currIndex + _positionMemoryLength) - 1) % _positionMemoryLength;
                if (!filled && _currIndex != 0){
                    filled = true;
                }
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
                    2*_positionMemory[(_currIndex + 1) % _positionMemoryLength] +
                    _positionMemory[(_currIndex + 2) % _positionMemoryLength]) /
                    _timestepSeconds/_timestepSeconds;
            }

            public Vector3 EstimateVelocity(){
                return (_positionMemory[_currIndex] - 
                    _positionMemory[(_currIndex + 1) % _positionMemoryLength])/
                    _timestepSeconds;
            }

        }
        class MotionEstimatorFloat {
            private float[] _positionMemory;
            private int _currIndex;
            public const int _positionMemoryLength = 3;
            private float _timestepSeconds;
            public bool filled {get; private set;}

            public MotionEstimatorFloat(float timestepSeconds) {
                _positionMemory = new float[_positionMemoryLength];
                _currIndex = 0;
                _timestepSeconds = timestepSeconds;

                for (int i = 0; i < _positionMemoryLength; i++) {
                    _positionMemory[i] = new float();   
                }
             }
            public void UpdateNewPosition(float position){
                _currIndex = ((_currIndex + _positionMemoryLength) - 1) % _positionMemoryLength;
                if (!filled && _currIndex != 0){
                    filled = true;
                }
                _positionMemory[_currIndex] = position;
            }
            // If no input, assume the arm has stayed stationary. Add a 
            // kinematic model if better performance is needed.
            public void EstimateUnobtainableNewPosition(){
                UpdateNewPosition(_positionMemory[_currIndex]);
            }

            // Just a backwards finite difference equation. Use more advanced 
            // techniques if necessary.
            public float EstimateAcceleration(){
                return (_positionMemory[_currIndex] - 
                    2*_positionMemory[(_currIndex + 1) % _positionMemoryLength] +
                    _positionMemory[(_currIndex + 2) % _positionMemoryLength]) /
                    _timestepSeconds/_timestepSeconds;
            }

            public float EstimateVelocity(){
                return (_positionMemory[_currIndex] - 
                    _positionMemory[(_currIndex + 1) % _positionMemoryLength])/
                    _timestepSeconds;
            }

        }
    }
}