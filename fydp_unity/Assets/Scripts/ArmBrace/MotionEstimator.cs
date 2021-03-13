using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;

namespace FYDP {
    namespace ArmBrace {
        class MotionEstimator<T> {
            private T[] _positionMemory;
            private int _currIndex;
            public const int _positionMemoryLength = 3;
            private float _timestepSeconds;
            public bool filled {get; private set;};

            public MotionEstimator(float timestepSeconds) {
                _positionMemory = new T[_positionMemoryLength];
                _currIndex = 0;
                _timestepSeconds = timestepSeconds;

                for (int i = 0; i < _positionMemoryLength; i++) {
                    _positionMemory[i] = new T();   
                }
             }
            public void UpdateNewPosition(T position){
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
            public T EstimateAcceleration(){
                return (_positionMemory[_currIndex] - 
                    2*_positionMemory[(_currIndex + 1) % _positionMemoryLength] +
                    _positionMemory[(_currIndex + 2) % _positionMemoryLength]) /
                    _timestepSeconds/_timestepSeconds;
            }

            public T EstimateVelocity(){
                return (_positionMemory[_currIndex] - 
                    _positionMemory[(_currIndex + 1) % _positionMemoryLength])/
                    _timestepSeconds;
            }

        }
    }
}