
using System.Collections.Generic;

namespace FYDP {
    namespace ArmBrace {
        class MovingAvg<T> {
            
            public MovingAvg(int windowSize) {
                _windowSize = windowSize;
                _buffer = new Queue<T>(_windowSize);
            }

            public bool Filled() {
                return _buffer.Count == _windowSize;
            }

            public void AddValue(T newValue) {
                if (Filled()) {
                    _runningSum -= _buffer.Dequeue();
                }
                _buffer.Enqueue(newValue);
                _runningSum += newValue;
                Avg =  _runningSum/_length;
            }
            
            private Queue<T> _buffer;
            private int _windowSize;
            private T _runningSum;
            public T Avg {get; private set;};
        }
    }
}