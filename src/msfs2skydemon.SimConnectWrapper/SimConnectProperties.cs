﻿using Microsoft.FlightSimulator.SimConnect;

namespace msfs2skydemon.SimConnectWrapper
{
    public class SimConnectProperties
    {
        public static SimConnectProperty PlaneLongitude = new SimConnectProperty(
            SimConnectPropertyKey.PlaneLongitude, "PLANE LONGITUDE", "degree", SIMCONNECT_DATATYPE.FLOAT64);
        public static SimConnectProperty PlaneLatitude = new SimConnectProperty(
            SimConnectPropertyKey.PlaneLatitude, "PLANE LATITUDE", "degree", SIMCONNECT_DATATYPE.FLOAT64);
        public static SimConnectProperty PlaneHeadingDegreesTrue = new SimConnectProperty(
            SimConnectPropertyKey.PlaneHeadingDegreesTrue, "PLANE HEADING DEGREES TRUE", "degree", SIMCONNECT_DATATYPE.FLOAT64);
        public static SimConnectProperty PlaneAltitude = new SimConnectProperty(
            SimConnectPropertyKey.PlaneAltitude, "PLANE ALTITUDE", "feet", SIMCONNECT_DATATYPE.FLOAT64);
        public static SimConnectProperty GpsGroundSpeed = new SimConnectProperty(
            SimConnectPropertyKey.GpsGroundSpeed, "GPS GROUND SPEED", "knots", SIMCONNECT_DATATYPE.FLOAT64);
        public static SimConnectProperty AtcId = new SimConnectProperty(
            SimConnectPropertyKey.AtcId, "ATC ID", "", SIMCONNECT_DATATYPE.STRING64);
        public static SimConnectProperty AtcType = new SimConnectProperty(
            SimConnectPropertyKey.AtcType, "ATC TYPE", "", SIMCONNECT_DATATYPE.STRING64);
    }
}
