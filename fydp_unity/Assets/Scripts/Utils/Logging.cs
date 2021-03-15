using UnityEngine;

namespace FYDP
{
    namespace Utils
    {
        class Logging
        {
            public static void PrintQtyScalar(string label, float qty, string units="")
            {
                Debug.Log(label + ": " + qty.ToString("F8") + units);
            }

            public static void PrintQtyScalar(string label, int qty, string units="")
            {
                Debug.Log(label + ": " + qty.ToString("F8") + units);
            }

            public static void PrintQtyVector3(string label, Vector3 qty, string units="")
            {
                Debug.Log(label + ": " + qty.ToString("F8") + units);
            }
        }
    }
}