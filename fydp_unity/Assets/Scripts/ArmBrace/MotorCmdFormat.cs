using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FYDP.Utils;

namespace FYDP {
    namespace ArmBrace{
        public class MotorCmdFormat {
            //enum values taken from the Arduino motor code
            public enum CmdTypeId {
                NoCmd = 0b_0000_0000,
                NoTorque = 1,
                TorqueCW = 2,
                TorqueCCW = 3,
                TorqueHold = 4
            }

            public CmdTypeId Id {get; private set;}
            public byte Data {get; private set;}
            
            private float _torqueRatingNm;
            private float _torqueCmdFullScale;
            private float _gearRatio;
            private byte _stictionEncodedTorque;

            public MotorCmdFormat(float torqueRatingNm, float torqueCmdFullScale, 
                                  float gearRatio, byte stictionEncodedTorque) {
                _torqueRatingNm = torqueRatingNm;
                _torqueCmdFullScale = torqueCmdFullScale;
                _gearRatio = gearRatio;
                _stictionEncodedTorque = stictionEncodedTorque;
            }

            //Augment this function with more adv. algorithm for more precision
            private byte EncodeTorque(float torque) {

                float encodedTorque = torque / _gearRatio / _TorqueRatingNm *
                                      _torqueCmdFullScale;
                
                return (byte)Numeric.Clamp(Numeric.AbsRoundToWhole(
                    encodedTorque), _torqueCmdFullScale);
            }

            private float DecodeTorque(byte encodedTorque) {
                return (float)encodedTorque * _gearRatio * _TorqueRatingNm /
                                      _torqueCmdFullScale;
            }
            public void SetNoTorque() {
                Id = CmdTypeId.NoTorque;
                Data = 0;
            }
            public void SetTorqueHold(float torque) {
                if (torque == 0) {
                    SetNoTorque();
                    return;
                }
                Id = CmdTypeId.TorqueHold;
                Data = EncodeTorque(torque);
            }

            public void SetTorqueMove(float torque) {
                if (torque == 0) {
                    SetNoTorque();
                    return;
                } 
                
                if (torque > 0) {
                    Id = CmdTypeId.TorqueCCW;
                }  
                else {
                    Id = CmdTypeId.TorqueCW;
                }
                Data = EncodeTorque(torque) + stictionEncodedTorque;
            }

            public void SetNoCmd() {
                Id = CmdTypeId.NoCmd;
                Data = 0;
            }
        }
    }
}
