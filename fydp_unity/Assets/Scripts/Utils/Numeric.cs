using System;

namespace FYDP {
    namespace Utils {  
        class Numeric {
            public static bool FloatEquals(float a, float b, float tolerance) {
                return Math.Abs(a - b) < tolerance;
            }
            public static float Clamp(float num, float limit) {
                return num > limit ? limit : num; 
            }
            public static int Clamp(int num, int limit) {
                return num > limit ? limit : num; 
            }
            public static float AbsRoundToWhole(float num) {
                return (float)Math.Round(Math.Abs(num), MidpointRounding.AwayFromZero);
            }
        }
    }
}
