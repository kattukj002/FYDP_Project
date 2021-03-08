using UnityEngine;

namespace FYDP
{
    namespace Utils
    {
        class Logging
        {
            public static void PrintQty<T>(string label, T qty, string units="")
            {
                Debug.Log(label + ": " + qty.ToString() + units);
            }
        }
    }
}