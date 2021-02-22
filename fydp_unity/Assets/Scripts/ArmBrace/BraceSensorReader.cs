using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading; 
using System;

namespace FYDP {
    namespace ArmBrace {
        public class BraceSensorReader {
            private enum AngleIndex {
                Elbow = 0,
                ShoulderAbduction = 1,
                ShoulderFlexion = 2
            }
            public BraceSensorReader(SerialPort arduinoPort) {
                _arduinoPort = arduinoPort;
                // Hard-coded, expand flexibility if needed.
                int frameSize = 4;
                byte frameStart = 0xFF;

                _frameSize = frameSize;
                _frameStart = frameStart;
                _jointAngleBuffers = new int[2, _frameSize - 1];
                for(int i = 0; i < 2; i++) {
                    for(int j = 0; j <  _frameSize - 1; j++) {
                        _jointAngleBuffers[i, j] = 0;
                    }
                }
                _readThread = new Thread(this.AsyncSensorReads);
            }

            ~BraceSensorReader() {
                StopAsyncSensorReads();
            }

            public void StartAsyncSensorReads() {
                if(!_readThread.IsAlive){
                    _stopThreadNeatly = false;
                    if(!_arduinoPort.IsOpen) {
                        _arduinoPort.Open();
                    }
                    _readThread.Start();   
                }
            }

            public void StopAsyncSensorReads() {
                if(_readThread.IsAlive){
                    _stopThreadNeatly = true;
                    _readThread.Join();   
                }
            }
            public bool GetJointAngles(out int elbowAngleDeg, 
                out int shoulderAbductionDeg, out int shoulderFlexionDeg) {
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
                while (!_stopThreadNeatly) {
                    if (_arduinoPort.IsOpen) {
                        
                        try{
                            bytesRead = _arduinoPort.Read(
                                buffer, 0, _frameSize);
                            byte[] by = new byte[bytesRead];
                            
                            for(int i = 0; i < bytesRead; i++) {
                                by[i] =  buffer[i];
                            }
                            Debug.Log("RECEIVED: " + BitConverter.ToString(by);
                        } catch (TimeoutException) {
                            continue;
                        }
                        
                        for(int i = 0; i < bytesRead; i++) {
                            if(buildingIncompleteFrame) {
                                _jointAngleBuffers[writeBufferIndex,currFrameByte] = buffer[i];
                                currFrameByte += 1;
                                if (currFrameByte >= _frameSize - 1){
                                    break;
                                }
                            }
                            if(buffer[i] == _frameStart) {
                                buildingIncompleteFrame = true;
                                currFrameByte = 0;
                            }   
                        }
                        //buildingIncompleteFrame = false;
                        //currFrameByte = 0;
                        
                        // While C# int read/writes are atomic, an array 
                        // access with a variable index is not atomic.
                        // If the mutex starts slowing this down, try 
                        // removing it; human motion should be slow enough
                        // for angles being a few frames out of sync should
                        // be fine.
                        if (currFrameByte >= _frameSize - 1) {
                            buildingIncompleteFrame = false;
                            if(_readBufferIndexMutex.WaitOne(1)) {
                                _readBufferIndex ^= 1;
                                writeBufferIndex = _readBufferIndex ^ 1;
                                Debug.Log("WRITE, _readBufferIndex: " + _readBufferIndex.ToString());
                                _readBufferIndexMutex.ReleaseMutex();
                                currFrameByte = 0;
                            }
                        }
                    }
                }
            }
            private SerialPort _arduinoPort;
            private int _frameSize;
            private byte _frameStart;
            private uint _readBufferIndex = 0; 
            private int[,] _jointAngleBuffers;
            private Thread _readThread = null;
            private Mutex _readBufferIndexMutex = new Mutex();
            private bool _stopThreadNeatly;
        }
    }
}