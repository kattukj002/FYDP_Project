using System;

namespace FYDP {
    namespace Utils {  
        class Numeric {
            public static bool FloatEquals(float a, float b, float tolerance) {
                return Math.Abs(a - b) < tolerance;
            }
        }
    }
}
