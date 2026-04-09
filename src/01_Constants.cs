/*//////////////////////////
 * Thank you for using:
 * [PAM] - Path Auto Miner
 * ————————————
 * Author:  Keks
 * Last update: 2019-12-20
 * ————————————
 * Guide: https://steamcommunity.com/sharedfiles/filedetails/?id=1553126390
 * Script: https://steamcommunity.com/sharedfiles/filedetails/?id=1507646929
 * Youtube: https://youtu.be/ne_i5U2Y8Fk
 * ————————————
 * DEOBFUSCATED VERSION - restored from minified source
 *//////////////////////////

const string VERSION = "1.3.1";
const string DATAREV = "14";

const String pamTag = "[PAM]";
const String controllerTag = "[PAM-Controller]";

const int gyroSpeedSmall = 15;       // [RPM] small ship
const int gyroSpeedLarge = 5;        // [RPM] large ship
const int generalSpeedLimit = 100;   // [m/s] 0 = no limit
const float dockingSpeed = 0.5f;     // [m/s]

const float dockDist = 0.6f;
const float followPathDock = 2f;
const float followPathJob = 1f;
const float useDockDirectionDist = 1f;
const float useJobDirectionDist = 0f;

const float wpReachedDist = 2f;      // [m]
const float drillRadius = 1.4f;      // [m]

const float sensorRange = 2f;
const float fastSpeed = 10f;

const float minAccelerationSmall = 0.2f; // [m/s²]
const float minAccelerationLarge = 0.1f; // [m/s²]

const float minEjection = 25f;       // [%]

const bool setLCDFontAndSize = true;
const bool checkConveyorSystem = false;
