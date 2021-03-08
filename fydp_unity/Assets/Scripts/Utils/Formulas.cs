using System;
using UnityEngine;
using FYDP.Utils;

namespace FYDP {
    namespace Utils {
        class Formulas {
            public static float CosineLawCalcLength(
                float length1, float length2, float radians) {
                
                return (float)Math.Sqrt(length1 * length1 + length2 *  length2 -
                    2 * length1 * length2 * Math.Cos(radians));
            }

            public static float CosineLawCosAngle(float length1, float length2,
                                            float angleOppositeLength) {
                
                return (float) ((length1 * length1 + length2 * length2 - 
                        angleOppositeLength * angleOppositeLength) / 
                        (2 * length1 * length2));
            }

            public static float[] QuadraticFormula(
                float a, float b, float c) {
                
                float term1 = (float)(-b / (2 * a));
                float term2 = (float)(Math.Sqrt(b * b - 4 * a * c) / (2 * a));
                
                return new float[2]{term1 + term2, term1 - term2};
            }

            public static Vector3 YPlaneLockedTwoBarStartMidVector(
                Vector3 startEndVector, float startMidLength, 
                float midEndLength, float cosLinkageAngle, 
                Vector3 prevStartMid, float tolerance=0.01f) {
                
                float lengthExpression = 
                    startEndVector.sqrMagnitude + 
                    startMidLength * startMidLength - 
                    midEndLength * midEndLength;

                float[] startMid_z = Formulas.QuadraticFormula(
                    a: startEndVector.z * startEndVector.z / (startEndVector.x * startEndVector.x) + 1, 
                    b: -lengthExpression * startEndVector.z / 
                        (startEndVector.x * startEndVector.x * startEndVector.x), 
                    c: lengthExpression * lengthExpression * 
                        (1 / (startEndVector.x * startEndVector.x) - 
                        1 / (cosLinkageAngle * cosLinkageAngle *
                            startEndVector.sqrMagnitude)
                        ) / 4);

                float[] startMid_x = new float[2];

                float[] cacheTerms = new float[2] {
                    lengthExpression / (2 * startEndVector.x), - startEndVector.z / startEndVector.x
                };
                Vector3[] startMid = new Vector3[2];

                for(int i = 0; i < 2; i++) {
                    startMid_x[i] = cacheTerms[0] + cacheTerms[1] * startMid_z[i];    
                    startMid[i] =  new Vector3(startMid_x[i], 0, startMid_z[i]);
                }
                if (Math.Abs(Vector3.Angle(prevStartMid, startMid[0])) < 
                    Math.Abs(Vector3.Angle(prevStartMid, startMid[1]))) {
                    return startMid[0];
                } else {
                    return startMid[1];
                }
                
            } 
        }
    }
}