# VR Force Feedback Arm
The embedded and unity code for the VR arm brace is here. We got it to demo in W2021 and can be built upon for more functionality and robustness. The core parts of this are fine but it's a bit disorganized and we needed to hack together some of the software to get it working for the demo.

`fydp_arduino/ffback_sys` has the Arduino code. You can open `fydp_unity` with Unity as a project folder. `Plotting Scripts/unity_log_to_graphable.py` will parse the Editor.log files holding all the Unity debug messsages and create a json with that data. `Plotting Scripts/plotter.py` can be modified to plot the data in that json using the functions in `Plotting Scripts/graph_unity_log.py`.

# Gotchas
- The parser is fairly primitive. Rogue statements with the structure `[A-Z\s]+:[\(\)0-9\.,]+` will get parsed as debug logs. Add these to the exclude list as needed.
- Some of the big functions in `Plotting Scripts/graph_unity_log.py` for plotting several things may be broken, but that's probabyl just because the labels for debug logs were changed. Just need to find the right label for the data, unless the data for that label was removed entirely. 
- The project was hard coded to just work with the right arm. Extending it to both arms will require some reading through the code development effort. 
- Motor directions were set according to how our motors were mounted. You might have to flip the sign of a motor torque if the motor directions are off.
- There are some scripts for testing serial communication (e.g. PingUnityFromArduino.cs, PingArduinoFromUnity.cs), but during our pre-demo day rush to integrate everything, they might have been modified, They're quite simple, but may need some cleaning up.
- BraceSensorReader read() needs to catch the timeout exception in the case that the read buffer is empty. Otherwise, you get an I/O Semaphore error on Windows.

# Details
Contact fkcheng@uwaterloo for questions about the software, the detailed report with the derivations, any crashes that occur, and to get in touch with the group members in charge of the mechanical and electrical components. 
