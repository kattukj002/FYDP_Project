using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;

namespace FYDP {
    namespace ArmBrace {
        public class BraceCmd {

            public BraceCmd(SerialPort arduinoPort) {
                byte frameHeader = 0xC0;

                _cmdFrameLength = 8;
                _cmdFrame = new byte[_cmdFrameLength];
                _cmdFrame[0] = frameHeader;
                _cmdFrame[1] = frameHeader;

                _arduinoPort = arduinoPort;
                if (!_arduinoPort.IsOpen) {
                    _arduinoPort.Open();
                }
            }

            public void Send() {

                _cmdFrame[2] = (byte)elbow.Id;
                _cmdFrame[3] = elbow.Data;
                _cmdFrame[4] = (byte)shoulderAbduction.Id;
                _cmdFrame[5] = shoulderAbduction.Data;
                _cmdFrame[6] = (byte)shoulderFlexion.Id;
                _cmdFrame[7] = shoulderFlexion.Data;

                if(_arduinoPort.IsOpen) {
                    _arduinoPort.Write(_cmdFrame, 0, _cmdFrameLength);
                } else {
                    Debug.Log("Cannot send arm command: arduinoPort is" + 
                              " not open.");
                }
            }
            private SerialPort _arduinoPort;
            private byte[] _cmdFrame;
            private int _cmdFrameLength;

            public MotorCmdFormat elbow = new MotorCmdFormat();
            public MotorCmdFormat shoulderAbduction = new MotorCmdFormat();
            public MotorCmdFormat shoulderFlexion = new MotorCmdFormat();
        }
    }
}
