﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FYDP {
    namespace ArmBrace{
        public class MotorCmdFormat {
            //enum values taken from the Arduino motor code
            public enum CmdTypeId {
                NoCmd = 0b_0000_0000,
                NoTorque = 1,
                SetTorqueCW = 2,
                SetTorqueCCW = 3,
                HoldTorque = 4
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
                
                if (torque == 0) {
                    Id = CmdTypeId.NoTorque;
                } else if (torque > 0) {
                    Id = CmdTypeId.SetTorqueCCW;
                }  
                else {
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
                    Data = (byte)(encodedTorque + 8);
                }
            }

            public void SetNoCmd() {
                Id = CmdTypeId.NoCmd;
                Data = 0;
            }
        }
    }
}
