I couldn't figure out how to get Microsoft Flight Simulator to sync position with Skydemon, and wanted to learn SimConnect.

For now, it's just an ugly prototype, but it works.

The project expects the SDK in "C:\MSFS SDK". If it's not there, change the pre-build events that get the following prerequisites:

- C:\MSFS SDK\SimConnect SDK\lib\SimConnect.dll
- C:\MSFS SDK\SimConnect SDK\lib\managed\Microsoft.FlightSimulator.SimConnect.dll
- C:\MSFS SDK\Samples\SimvarWatcher\SimConnect.cfg