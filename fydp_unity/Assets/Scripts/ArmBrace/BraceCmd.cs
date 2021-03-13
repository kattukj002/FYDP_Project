using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;
using FYDP.Utils;

namespace FYDP {
    namespace ArmBrace {
        public class BraceCmd {

            public BraceCmd(SerialPort arduinoPort,
                            MotorCmdFormat elbow_,
                            MotorCmdFormat shoulderDown_) {
                byte frameHeader = 0xC0;

                elbow = elbow_;
                shoulderDown = shoulderDown_;

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
                _cmdFrame[4] = (byte)shoulderDown.Id;
                _cmdFrame[5] = shoulderDown.Data;
                // Zeroing out motor command until the embedded SW changes
                _cmdFrame[6] = (byte)MotorCmdFormat.CmdTypeId.NoCmd;
                _cmdFrame[7] = 0;

                if(_arduinoPort.IsOpen) {
                    _arduinoPort.Write(_cmdFrame, 0, _cmdFrameLength);
                    Logging.PrintQty("ELBOW_CMD_ID", elbow.Id);
                    Logging.PrintQty("ELBOW_CMD_TORQUE", elbow.Data, "N");
                } else {
                    Debug.Log("Cannot send arm command: arduinoPort is" + 
                              " not open.");
                }
            }
            private SerialPort _arduinoPort;
            private byte[] _cmdFrame;
            private int _cmdFrameLength;

            public MotorCmdFormat elbow;
            public MotorCmdFormat shoulderDown;
        }
    }
}
