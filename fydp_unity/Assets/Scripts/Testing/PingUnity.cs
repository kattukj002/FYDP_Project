using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;

public class PingUnity : MonoBehaviour
{
    public string portName = "/dev/ttyACM0";
    SerialPort arduino;

    // Start is called before the first frame update
    void Start()
    {
        arduino = new SerialPort(portName, 9600);
        arduino.ReadTimeout = 100;
        arduino.Open();
    }

    // Update is called once per frame
    void Update()
    {
                StartCoroutine(
            AsynchronousReadFromArduino
            (   (string s) => Debug.Log(s),     // Callback
                () => Debug.LogError("Error!"), // Error callback
                10000f                          // Timeout (milliseconds)
            )
        );
 
    }

    public IEnumerator AsynchronousReadFromArduino(Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity) {
        DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        string dataString = null;

        do {
            try {
                dataString = arduino.ReadLine();
            }
            catch (TimeoutException) {
                dataString = null;
            }

            if (dataString != null)
            {
                callback(dataString);
                yield break; // Terminates the Coroutine
            } else
                yield return null; // Wait for next frame

            nowTime = DateTime.Now;
            diff = nowTime - initialTime;

        } while (diff.Milliseconds < timeout);

        if (fail != null)
            fail();
        yield return null;
    }
}
