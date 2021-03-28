using UnityEngine;

namespace FYDP
{
    namespace ArmBrace
    {
        public struct CalibrationValues
        {
            public float UpperArmLength;
            public float LowerArmLength;
            public float ShoulderDistFromNeckBase;
            public Vector3 NeckBaseOffsetFromHeadset;
            public Vector3 CableMotorOffsetfromShoulder;
            public float CableWinchRadius;
            public float ImuSensorMsgFreq;
        }
    }
}
