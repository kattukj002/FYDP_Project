using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;

namespace FYDP {
    namespace ArmBrace {
        public class BraceSensorReader {
            private enum AngleIndex {
                Elbow = 0,
                ShoulderAbduction = 1,
                ShoulderFlexion = 2
            }
            public BraceSensorReader(
                SerialPort arduinoPort) {
                _arduinoPort = arduinoPort;

                // Hard-coded, expand flexibility if needed.
                int frameSize = 4;
                byte frameStart = 0xFF;

                _frameSize = frameSize;
                _frameStart = frameStart;
                _readThread = new Thread(this.AsyncSensorReads);
            }

            public void StartAsyncSensorReads() {
                if(!_readThread.IsAlive){
                    _stopThreadNeatly = false;
                    _readThread.Start();   
                }
            }

            public void StopAsyncSensorReads() {
                if(!_readThread.IsAlive){
                    _stopThreadNeatly = true;
                    _readThread.Join();   
                }
            }
            public bool GetJointAngles(out uint elbowAngleDeg, 
                out uint shoulderAbductionDeg, out uint shoulderFlexionDeg) {
                
                if(_readBufferIndexMutex.WaitOne(1)) {
                    elbowAngleDeg = 
                        _jointAngleBuffers[_readBufferIndex,(int)AngleIndex.Elbow];
                    shoulderAbductionDeg = 
                        _jointAngleBuffers[_readBufferIndex,(int)AngleIndex.ShoulderAbduction];
                    shoulderFlexionDeg = 
                        _jointAngleBuffers[_readBufferIndex,(int)AngleIndex.ShoulderFlexion];
                    _readBufferIndexMutex.ReleaseMutex();

                    return true;
                } 
                elbowAngleDeg = 0;
                shoulderAbductionDeg = 0;
                shoulderFlexionDeg = 0;
                return false;
            }
            private void AsyncSensorReads() {
                byte[] buffer = new byte[_frameSize];
                int currFrameByte = 0;
                bool buildingIncompleteFrame = false;

                uint writeBufferIndex = _readBufferIndex ^ 1;
                int bytesRead = 0;
                uint iterator = 0;

                while (!_stopThreadNeatly) {
                    if (_arduinoPort.IsOpen) {
                    
                        bytesRead = _arduinoPort.Read(
                            buffer, 0, _frameSize);
                        
                        for(iterator = 0; iterator < bytesRead; iterator += 1) {
                            if(buffer[iterator] == _frameStart) {
                                buildingIncompleteFrame = true;
                                currFrameByte = 0;
                                continue;
                            }
                            if(buildingIncompleteFrame) {
                                _jointAngleBuffers[writeBufferIndex,currFrameByte] = buffer[iterator];
                                currFrameByte += 1;
                                if (currFrameByte >= _frameSize){
                                    buildingIncompleteFrame = false;
                                    break;
                                }
                            }   
                        }
                        // While C# int read/writes are atomic, an array 
                        // access with a variable index is not atomic.
                        // If the mutex starts slowing this down, try 
                        // removing it; human motion should be slow enough
                        // for angles being a few frames out of sync should
                        // be fine.
                        if (currFrameByte >= _frameSize) {
                            if(_readBufferIndexMutex.WaitOne(1)) {
                                _readBufferIndex ^= 1;
                                writeBufferIndex = _readBufferIndex ^ 1;
                                _readBufferIndexMutex.ReleaseMutex();
                            }
                        }
                    }
                }
            }
            private SerialPort _arduinoPort;
            private int _frameSize;
            private byte _frameStart;
            private uint _readBufferIndex = 0; 
            private uint[,] _jointAngleBuffers = new uint[2,3];
            private Thread _readThread = null;
            private Mutex _readBufferIndexMutex;
            private bool _stopThreadNeatly;
        }
    }
}