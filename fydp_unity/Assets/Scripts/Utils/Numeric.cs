using System;

namespace FYDP {
    namespace Utils {  
        class Numeric {
            public static bool FloatEquals(float a, float b, float tolerance) {
                return Math.Abs(a - b) < tolerance;
            }
            public static T Clamp<T>(T num, T limit) {
                return num > limit ? limit : num; 
            }
            public static float AbsRoundToWhole(float num) {
                return Math.Round(Math.Abs(num), MidpointRounding.AwayFromZero)
            }
        }
    }
}
