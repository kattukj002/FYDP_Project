using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FYDP {
    namespace ArmBrace{
        public class MotorCmdFormat {
            public enum CmdTypeId {
                NoCmd = 0b_0000_0000,
                SetTorqueCW = 0b_0000_0010,
                SetTorqueCCW = 0b_0000_0100,
                HoldAngle = 0b_0000_1000
            }

            public CmdTypeId Id {get; private set;}
            public byte Data {get; private set;}

            //Augment this function with more adv. algorithm for more precision
            private byte EncodeTorque(float torque) {
                return (byte)System.Math.Round(System.Math.Abs(torque), 0, 
                                    System.MidpointRounding.AwayFromZero);
            }

            private float DecodeTorque(byte torque) {
                return (float)torque;
            }
            public void SetTorque(float torque) {
                byte headerBits = 0xC0;
                byte maxEncodableTorque = (byte)(~headerBits);
                
                if (torque > 0) {
                    Id = CmdTypeId.SetTorqueCCW;
                } else {
                    Id = CmdTypeId.SetTorqueCW;
                }

                byte encodedTorque = EncodeTorque(torque);

                if ((headerBits & encodedTorque) > 0) {
                    Debug.Log("Motor command torque exceeds the maximum " + 
                                "magnitude " + 
                                DecodeTorque(maxEncodableTorque).ToString() + 
                                "N-m, max torque applied instead.");
                        
                    Data = maxEncodableTorque;
                } else {
                    Data = encodedTorque;
                }
            }

            public void SetNoCmd() {
                Id = CmdTypeId.NoCmd;
                Data = 0;
            }
        }
    }
}
