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
                public float ImuXAcceleration;
                public float ImuYAcceleration;
                public float ImuZAcceleration;
                public float ImuXAngularVelocity;
                public float ImuYAngularVelocity;
                public float ImuZAngularVelocity; 
            }
            public BraceSensorReader(SerialPort arduinoPort) {
                _arduinoPort = arduinoPort;
                // Hard-coded, expand flexibility if needed.
                _frameHeader = new byte[2] {0xC0, 0xC0};
                _frameMsgLength = 22;
                _imuEstimator = null;

                _sensorDataBuffers = new SensorDataBuffer[2];
                for(int i = 0; i < 2; i++) {
                    _sensorDataBuffers[i] = new SensorDataBuffer();
                }
                _readThread = new Thread(this.AsyncSensorReads);
                _imuThread = new Thread(this.UpdateImuEstimates);
                
            }
            public void SetImuEstimator(ImuEstimator imuEstimator) {
                _imuEstimator = imuEstimator;
            }

            private float DecodeAngleBytes(byte msb, byte lsb) {
                return (float)(((msb << 8) | lsb) * 100/4096.0f);
            }

            private float DecodeImuAccelerationBytes(byte msb, byte lsb) {
                return (float) (((msb << 8) | lsb) / (1 << 16 - 1) * 4 * 9.81f);
            }

            private float DecodeImuAngularVelocityBytes(byte msb, byte lsb) {
                return (float) (((msb << 8) | lsb) / (1 << 16 - 1) * 500);
            }

            private void ProcessInputByteArray(
                byte[] byteArray, out SensorDataBuffer sensorDataBuffer) {
                
                sensorDataBuffer.ElbowDeg = DecodeAngleBytes(
                    byteArray[1], byteArray[0]);

                sensorDataBuffer.ImuXAcceleration = DecodeImuAccelerationBytes(
                    byteArray[10], byteArray[9]);
                sensorDataBuffer.ImuYAcceleration = DecodeImuAccelerationBytes(
                    byteArray[12], byteArray[11]);
                sensorDataBuffer.ImuZAcceleration  = DecodeImuAccelerationBytes(
                    byteArray[14], byteArray[13]);
                sensorDataBuffer.ImuXAngularVelocity = DecodeImuAngularVelocityBytes(
                    byteArray[16], byteArray[15]);
                sensorDataBuffer.ImuYAngularVelocity = DecodeImuAngularVelocityBytes(
                    byteArray[18], byteArray[17]);
                sensorDataBuffer.ImuZAngularVelocity = DecodeImuAngularVelocityBytes(
                    byteArray[20], byteArray[19]); 
            }

            ~BraceSensorReader() {
                StopAsyncSensorReads();
            }

            public void StartAsyncSensorReads() {
                if (_imuEstimator == null) {
                    throw new Exception("BraceSensorReader started without ImuEstimator");
                }
                _stopThreadNeatly = false;
                if(!_readThread.IsAlive){
                    if(!_arduinoPort.IsOpen) {
                        _arduinoPort.Open();
                    }
                    _readThread.Start();   
                }
                if (!_imuThread.IsAlive) {
                    _imuThread.Start();
                }
            }

            public void StopAsyncSensorReads() {
                _stopThreadNeatly = true;

                if(_readThread.IsAlive){
                    _readThread.Join();   
                }
                if (_imuThread.IsAlive) {    
                    _imuThread.Join();
                }
            }

            public bool GetBraceSensorData(out float elbowDeg, 
                out Vector3 positionEstimate, out Vector3 elbowAxisEstimate,
                out Vector3 upperArmAxisEstimate) {
                
                elbowDeg = 0;

                positionEstimate = _imuEstimator.PositionEstimate;
                elbowAxisEstimate = _imuEstimator.ElbowAxisEstimate;
                upperArmAxisEstimate = _imuEstimator.UpperArmAxisEstimate;
                
                if(_readBufferIndexMutex.WaitOne(1)) {
                    elbowDeg = 
                        _sensorDataBuffers[_readBufferIndex].ElbowDeg;
                    _readBufferIndexMutex.ReleaseMutex();

                    return true;
                }
                
                return false;
            }

            public void UpdateImuEstimates() {
                while(!_stopThreadNeatly) {
                    if(unreadImuData && _readBufferIndexMutex.WaitOne(1)) {
                        _imuEstimator.getNewImuData(
                            new Vector3(
                                _sensorDataBuffers[_readBufferIndex].ImuXAcceleration, 
                                _sensorDataBuffers[_readBufferIndex].ImuYAcceleration, 
                                _sensorDataBuffers[_readBufferIndex].ImuZAcceleration
                            ), 
                            new Vector3(
                                _sensorDataBuffers[_readBufferIndex].ImuXAngularVelocity, 
                                _sensorDataBuffers[_readBufferIndex].ImuYAngularVelocity, 
                                _sensorDataBuffers[_readBufferIndex].ImuZAngularVelocity
                            ));
                        unreadImuData = false;
                        _readBufferIndexMutex.ReleaseMutex();
                    }
                    _imuEstimator.UpdateEstimates(); // No mutex, assume rate is fast enough that values in between timesteps is negligible
                }
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
                            byte[] bufcp = new byte[bytesRead];
                            for(int i = 0; i < bytesRead; i++) {
                                bufcp[i] = buffer[i];
                            }                         
                            Debug.Log("RAW_SERIAL:" + BitConverter.ToString(bufcp));
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
                            
                            ProcessInputByteArray(
                                msgBytes, out _sensorDataBuffers[writeBufferIndex]);

                            if(_readBufferIndexMutex.WaitOne(1)) {
                                _readBufferIndex ^= 1;
                                writeBufferIndex = _readBufferIndex ^ 1;
                                unreadImuData = true;
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
            private Thread _imuThread = null;
            private Mutex _readBufferIndexMutex = new Mutex();
            private Mutex _readImuEstimatorMutex = new Mutex();
            private bool _stopThreadNeatly;
            private bool unreadImuData;
            private ImuEstimator _imuEstimator;
        }
    }
}