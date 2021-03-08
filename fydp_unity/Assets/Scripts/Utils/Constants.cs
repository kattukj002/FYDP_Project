using UnityEngine;
using System;

namespace FYDP {
    namespace Utils {
        class Constants {
            private static Vector3 _zeroVector = new Vector3(0,0,0);
            public static Vector3 ZeroVector {
                get {return _zeroVector;} 
                private set{}
            }

        }
    }
}