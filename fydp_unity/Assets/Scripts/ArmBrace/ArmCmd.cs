using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

namespace FYDP {
    namespace ArmBrace {
        public class ArmCmd {

            public ArmCmd(SerialPort arduinoPort) {
                byte frameHeader = 0xC0;

                _armCmdLength = 8;
                _armCmd = new byte[8];
                _armCmd[0] = frameHeader;
                _armCmd[1] = frameHeader;

                _arduinoPort = arduinoPort;
                if (!_arduinoPort.IsOpen) {
                    _arduinoPort.Open();
                }
            }

            public void Send() {

                _armCmd[2] = (byte)elbow.Id;
                _armCmd[3] = elbow.Data;
                _armCmd[4] = (byte)shoulderAbduction.Id;
                _armCmd[5] = shoulderAbduction.Data;
                _armCmd[6] = (byte)shoulderFlexion.Id;
                _armCmd[7] = shoulderFlexion.Data;

                if(_arduinoPort.IsOpen) {
                    _arduinoPort.Write(_armCmd, 0, _armCmdLength);
                } else {
                    Debug.Log("Cannot send arm command: arduinoPort is" + 
                              " not open.");
                }
            }
            private SerialPort _arduinoPort;
            private byte[] _armCmd;
            private int _armCmdLength;

            public MotorCmd elbow = new MotorCmd();
            public MotorCmd shoulderAbduction = new MotorCmd();
            public MotorCmd shoulderFlexion = new MotorCmd();
        }
    }
}
