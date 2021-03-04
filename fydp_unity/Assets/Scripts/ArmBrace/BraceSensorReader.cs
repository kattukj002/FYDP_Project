using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading; 
using System;

namespace FYDP {
    namespace ArmBrace {
        public class BraceSensorReader {
            private struct SensorDataBuffer {
                public float ElbowDeg;
                public float ShoulderAbductionDeg;
                public float ShoulderFlexionDeg;
            }
            public BraceSensorReader(SerialPort arduinoPort) {
                _arduinoPort = arduinoPort;
                // Hard-coded, expand flexibility if needed.
                _frameHeader = new byte[2] {0xC0, 0xC0};
                _frameMsgLength = 10;
                
                _sensorDataBuffers = new SensorDataBuffer[2];
                for(int i = 0; i < 2; i++) {
                    _sensorDataBuffers[i] = new SensorDataBuffer();
                }
                _readThread = new Thread(this.AsyncSensorReads);
            }

            private float DecodeAngleBytes(byte msb, byte lsb) {
                return (((msb << 8) | lsb) * 180/(float)Int16.MaxValue);
            }

            private void ProcessInputByteArray(
                byte[] byteArray, out SensorDataBuffer sensorDataBuffer) {
                
                sensorDataBuffer.ElbowDeg = DecodeAngleBytes(
                    byteArray[1], byteArray[0]);

                sensorDataBuffer.ShoulderAbductionDeg = DecodeAngleBytes(
                    byteArray[3], byteArray[2]);

                sensorDataBuffer.ShoulderFlexionDeg = DecodeAngleBytes(
                    byteArray[5], byteArray[4]);
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
            public bool GetJointAngles(out float elbowDeg, 
                out float shoulderAbductionDeg, out float shoulderFlexionDeg) {
                if(_readBufferIndexMutex.WaitOne(1)) {
                    elbowDeg = 
                        _sensorDataBuffers[_readBufferIndex].ElbowDeg;
                    shoulderAbductionDeg = 
                        _sensorDataBuffers[_readBufferIndex].ShoulderAbductionDeg;
                    shoulderFlexionDeg = 
                        _sensorDataBuffers[_readBufferIndex].ShoulderFlexionDeg;
                    
                    _readBufferIndexMutex.ReleaseMutex();

                    return true;
                }
                elbowDeg = 0;
                shoulderAbductionDeg = 0;
                shoulderFlexionDeg = 0;
                return false;
            }
            private void AsyncSensorReads() {
                int readLength = _frameHeader.Length + _frameMsgLength;

                byte[] buffer = new byte[readLength];
                byte[] msgBytes = new byte[_frameMsgLength];
                int currFrameMsgByte = 0;
                int currFrameHeaderByte = 0;

                bool buildingIncompleteFrame = false;

                uint writeBufferIndex = _readBufferIndex ^ 1;
                int bytesRead = 0;

                while (!_stopThreadNeatly) {
                    if (_arduinoPort.IsOpen) {
                        
                        try{
                            bytesRead = _arduinoPort.Read(buffer, 
                                                          0, 
                                                          readLength);
                        } catch (TimeoutException) {
                            continue;
                        }
                        
                        for(int i = 0; i < bytesRead; i += 1) {

                            if(buildingIncompleteFrame) {    
                                msgBytes[currFrameMsgByte] = buffer[i];
                                currFrameMsgByte += 1;
                                if (currFrameMsgByte >= _frameMsgLength) {
                                    break;
                                }
                            } else if(buffer[i] == _frameHeader[currFrameHeaderByte]) {
                                currFrameHeaderByte += 1;

                                if (currFrameHeaderByte >= _frameHeader.Length) { 
                                    buildingIncompleteFrame = true;
                                    currFrameHeaderByte = 0;
                                    currFrameMsgByte = 0;
                                } 
                            } else {
                                currFrameHeaderByte = 0;
                            }
                        }
                                                
                        // While C# int read/writes are atomic, an array 
                        // access with a variable index is not atomic.
                        // If the mutex starts slowing this down, try 
                        // removing it; human motion should be slow enough
                        // for angles being a few frames out of sync should
                        // be fine.
                        if (currFrameMsgByte >= _frameMsgLength) {
                            buildingIncompleteFrame = false;
                            
                            //Debug.Log("MSG: " + BitConverter.ToString(msgBytes));
                            ProcessInputByteArray(
                                msgBytes, out _sensorDataBuffers[writeBufferIndex]);

                            if(_readBufferIndexMutex.WaitOne(1)) {
                                _readBufferIndex ^= 1;
                                writeBufferIndex = _readBufferIndex ^ 1;
                                _readBufferIndexMutex.ReleaseMutex();
                                currFrameMsgByte = 0;
                            }
                        }
                    }
                }
            }
            private SerialPort _arduinoPort;
            private int _frameMsgLength;
            private byte[] _frameHeader;
            private uint _readBufferIndex = 0; 
            private SensorDataBuffer[] _sensorDataBuffers;
            private Thread _readThread = null;
            private Mutex _readBufferIndexMutex = new Mutex();
            private bool _stopThreadNeatly;
        }
    }
}