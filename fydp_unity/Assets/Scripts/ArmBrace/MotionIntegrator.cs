using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;

namespace FYDP {
    namespace ArmBrace {
        class MotionIntegratorVector3 {
            class Integrand{
                private Vector3[] Samples;
                private int _index;
                private int maxSampleLength;
                private float _timestepSeconds;
                public bool enoughDataPoints {get; private set;}

                public Integrand(float timestepSeconds) {
                    _timestepSeconds = timestepSeconds;

                    _index = 0;
                    maxSampleLength = 5;
                    Samples = new Vector3[maxSampleLength];
                    for (int i = 0;  i < maxSampleLength; i++) {
                        Samples[i] = new Vector3();
                    } 
                    enoughDataPoints = false;
                }
                public void AddSample(Vector3 sample, out bool fullSampleBuffer) {
                    fullSampleBuffer = false;
                    if (_index > maxSampleLength) { 
                        throw new Exception("Index exceeds bounds. Current index: " + _index.ToString());
                    }
                    Samples[_index] = sample;
                    _index++;
                    if (_index >= maxSampleLength) {
                        fullSampleBuffer = true;
                    }
                    if(_index >= 2 && !enoughDataPoints) {
                        enoughDataPoints = true;
                    }
                }

                public Vector3 LatestSample() {
                    if (_index > 0) {
                        return Samples[_index - 1];
                    } else {
                        return Vector3.zero;
                    }
                    
                }
                public void ClearSamples() {
                    if (_index > 0) {
                        Samples[0] = Samples[maxSampleLength - 1];
                        _index = 1;    
                    }
                }
                
                public Vector3 EstimateIntegral(out bool clearedSamples ) {
                    Vector3 segmentIntegral;

                    clearedSamples = false;

                    switch(_index) {
                        case 5:
                            //Boole's rule
                            segmentIntegral = 2 * _timestepSeconds / 45 * (7 * (Samples[0] + Samples[4]) + 32 * (Samples[1] + Samples[3]) + 12 * Samples[2]);
                            ClearSamples();
                            clearedSamples = true;
                            break;
                        case 4:
                            //Simpson's 3/8 rule
                            segmentIntegral = 3 * _timestepSeconds / 8 * (Samples[0] + 3 * (Samples[1] + Samples[2]) + Samples[3]);
                            break;
                        case 3:
                            //Simpson's rule
                            segmentIntegral = _timestepSeconds / 3 * (Samples[0] + 4 * Samples[1] + Samples[2]);
                            break;
                        case 2:
                            //Trapazoidal rule
                            segmentIntegral = _timestepSeconds / 2 * (Samples[0] + Samples[1]);
                            break;
                        case 1:
                            segmentIntegral = Vector3.zero;
                            break;
                        case 0:
                            segmentIntegral = Vector3.zero;
                            break;
                        default:
                            throw new ArgumentException("Invalid integrand size", _index.ToString());
                            break;
                    }
                    return segmentIntegral;
                }
            }
            private Integrand _rateBuffer;
            private Integrand _firstIntegralBuffer;
            private Vector3 _secondIntegral;
            private bool _needSecondIntegral;
            public MotionIntegratorVector3(float timestepSeconds, bool needSecondIntegral=true) {
                _rateBuffer = new Integrand(timestepSeconds);
                _firstIntegralBuffer = new Integrand(timestepSeconds);
                if (needSecondIntegral) {
                    _secondIntegral = Vector3.zero;
                }

                _needSecondIntegral = needSecondIntegral;
             }
            public void UpdateNewRate(Vector3 rate) {
                _rateBuffer.AddSample(rate, out bool fullRateBuffer);
                if (fullRateBuffer) {                    
                    _firstIntegralBuffer.AddSample(_rateBuffer.EstimateIntegral(out bool clearedRateBuffer), out bool fullFirstIntegralBuffer);
                    Debug.Assert(clearedRateBuffer, "_rateBuffer was not cleared after full integral estimate.");

                    if (fullFirstIntegralBuffer) {
                        if (_needSecondIntegral) {
                            _secondIntegral += _firstIntegralBuffer.EstimateIntegral(out bool clearedFirstIntegralBuffer);
                            Debug.Assert(clearedFirstIntegralBuffer, "_firstIntegralBuffer was not cleared after full integral estimate.");
                        } else {
                            _firstIntegralBuffer.ClearSamples();
                        }
                        
                    }
                }                
            }

            // If no input, assume the arm has stayed stationary. Add a 
            // kinematic model if better performance is needed.
            public void EstimateUnobtainableNewRate() {
                UpdateNewRate(Vector3.zero);
            }

            public Vector3 FirstIntegral(out bool enoughDataPoints) {
                enoughDataPoints = _rateBuffer.enoughDataPoints;
                if (_rateBuffer.enoughDataPoints) {
                    Vector3 integral = _firstIntegralBuffer.LatestSample() + _rateBuffer.EstimateIntegral(out bool clearedRateBuffer);
                    Debug.Assert(!clearedRateBuffer, "_rateBuffer cleared without saving integral.");
                    return integral;
                } else {
                    return new Vector3();
                }
                
            }
            public Vector3 SecondIntegral(out bool enoughDataPoints) {
                enoughDataPoints = _firstIntegralBuffer.enoughDataPoints;
                if (_firstIntegralBuffer.enoughDataPoints) {
                    Vector3 integral = _secondIntegral + _firstIntegralBuffer.EstimateIntegral(out bool clearedFirstIntegralBuffer);
                    Debug.Assert(!clearedFirstIntegralBuffer, "_firstIntegralBuffer cleared without saving integral.");
                    return integral;
                } else {
                    return new Vector3();
                }
            }
        }
    }
}