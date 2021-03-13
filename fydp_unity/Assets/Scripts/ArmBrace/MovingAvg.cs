
using System.Collections.Generic;
using UnityEngine;

namespace FYDP {
    namespace ArmBrace {
        class MovingAvgVector3 {
            
            public MovingAvgVector3(int windowSize) {
                _windowSize = windowSize;
                _buffer = new Queue<Vector3>(_windowSize);
            }

            public bool Filled() {
                return _buffer.Count == _windowSize;
            }

            public void AddValue(Vector3 newValue) {
                if (Filled()) {
                    _runningSum -= _buffer.Dequeue();
                }
                _buffer.Enqueue(newValue);
                _runningSum += newValue;
                Avg =  _runningSum/_windowSize;
            }
            
            private Queue<Vector3> _buffer;
            private int _windowSize;
            private Vector3 _runningSum;
            public Vector3 Avg {get; private set;}
        }

        class MovingAvgFloat {
            
            public MovingAvgFloat(int windowSize) {
                _windowSize = windowSize;
                _buffer = new Queue<float>(_windowSize);
            }

            public bool Filled() {
                return _buffer.Count == _windowSize;
            }

            public void AddValue(float newValue) {
                if (Filled()) {
                    _runningSum -= _buffer.Dequeue();
                }
                _buffer.Enqueue(newValue);
                _runningSum += newValue;
                Avg =  _runningSum/_windowSize;
            }
            
            private Queue<float> _buffer;
            private int _windowSize;
            private float _runningSum;
            public float Avg {get; private set;}
        }
    }
}