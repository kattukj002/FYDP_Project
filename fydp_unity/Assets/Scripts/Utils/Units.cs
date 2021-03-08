using System;

namespace FYDP {
    namespace Utils {
        class Units {
            public static float DegreesToRadians(float degrees) {
                return (float)(degrees * Math.PI / 180);
            }

            public static float RadiansToDegrees(float radians) {
                return (float)(radians * 180 / Math.PI);
            }
        }
    }
}