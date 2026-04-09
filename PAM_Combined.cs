// ========================== 01_Constants.cs ==========================
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
const string DATAREV = "15";

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

// ========================== 02_Enums.cs ==========================
// ========================== ENUMS ==========================

public enum ShipMode       { Unknown, Miner, Grinder, Controller, Shuttle }
public enum NavState       { Idle, FlyToXY, Mining, Returning, ReturnToDock, FlyToDockArea, FlyToJobArea,
                             FlyToPath, FlyToJobPos, ApproachDock, Docking, AlignDock, AlignJob,
                             RetryDocking, Unloading, Undocking, Charging, WaitUranium, FillHydrogen,
                             WaitEjection, WaitEjectionDrop, FlyToDropPos, FlyToDropPos2, WaitForCommand }
public enum JobState       { NoJob, Paused, Active, Done, Changed, MoveHome, MoveToJob, ActiveHome, ActiveJob }
public enum BatteryState   { None, Charging, Idle, Discharging }
public enum MenuPage       { Main, Recording, JobSetup, JobRunning, StatusOverview, BehaviorSettings,
                             AdvancedSettings, InfoPage, InfoPage2, HelpPage, ShipList, ShipInventory,
                             SendCommand, ShuttlePage1, ShuttlePage2, ShuttleBehavior, ShuttleBehavior2 }
public enum StatusMsg      { Running, ConnectorNotReady, ShipModified, Interrupted, Shuttle }
public enum StartPosition  { TopLeft, Center }
public enum DamageBehavior { ReturnHome, FlyToJob, Stop, Ignore }
public enum EjectionMode   { Off, CurPosStone, CurPosStoIce, DropPosStone, DropPosStoIce, InMotionStone, InMotionStoIce }
public enum DepthMode      { Default, AutoOre, AutoStone }
public enum ItemLocation   { InCargo, InOther, All }
public enum UndockTrigger  { OnCommand, OnPlayerEntered, OnTimerDelay, OnShipFull, OnShipEmpty,
                             OnBatteriesFull, OnBatteriesLow25, OnBatteriesEmpty, OnHydrogenFull,
                             OnHydrogenLow25, OnHydrogenEmpty }
public enum LocationType   { AtJob, OnPath, AtHomeDock, AtMine, AtDock1, AtDock2 }
public enum SpiralResult   { Found, AllDone, Error }
public enum DataResult     { Success, Failed, NoData, NewVersion }
public enum CommandState   { None, Pending, Received, NoAnswer }
public enum ConfigType     { WorkSpeed, Acceleration, MaxLoad, Ignore, MinBattery, MinUranium, MinHydrogen, TimerDelay, Overlap }
public enum LCDMode        { Unknown, MainPaged, MainCompact, Menu, Inventory, Debug, MainPagedCompact }

// ========================== 03_Types.cs ==========================
// ========================== CLASSES ==========================

class WaypointInfo
{
    public bool isSet = false;
    public Vector3 position = new Vector3();
    public Vector3 downDir = new Vector3();
    public Vector3 forwardDir = new Vector3();
    public Vector3 leftDir = new Vector3();
    public Vector3 gravity = new Vector3();
    public Vector3 connectorGridPos = new Vector3();
    public float maxSpeed = 0;
    public float unused = 0;
    public float[] thrusterEfficiency = null;

    public WaypointInfo() { }

    public WaypointInfo(WaypointInfo other)
    {
        isSet = other.isSet;
        position = other.position;
        downDir = other.downDir;
        forwardDir = other.forwardDir;
        leftDir = other.leftDir;
        gravity = other.gravity;
        connectorGridPos = other.connectorGridPos;
        thrusterEfficiency = other.thrusterEfficiency;
    }

    public WaypointInfo(Vector3 position, Vector3 forwardDir, Vector3 downDir, Vector3 leftDir, Vector3 gravity)
    {
        this.position = position;
        this.downDir = downDir;
        this.forwardDir = forwardDir;
        this.leftDir = leftDir;
        this.maxSpeed = 0;
        this.gravity = gravity;
    }

    public void CalcThrusterEfficiency(List<IMyThrust> allThrusters, List<string> thrusterTypeNames)
    {
        thrusterEfficiency = new float[thrusterTypeNames.Count];
        for (int i = 0; i < thrusterEfficiency.Length; i++)
            thrusterEfficiency[i] = -1;

        for (int i = 0; i < allThrusters.Count; i++)
        {
            string typeName = GetThrusterType(allThrusters[i]);
            int idx = thrusterTypeNames.IndexOf(typeName);
            if (idx != -1)
                thrusterEfficiency[idx] = SafeDiv(allThrusters[i].MaxEffectiveThrust, allThrusters[i].MaxThrust);
        }
    }
}

class PathFollowData
{
    public WaypointInfo target = null;
    public List<Vector3> targetPositions = new List<Vector3>();
    public float directFlyDist = 0;
    public float useDockDirDist = 0;
    public float approachSpeed = 0;
    public float excludeDist = 0;
    public Vector3 excludePos = new Vector3();
}

class UndockConfig
{
    public UndockTrigger trigger = UndockTrigger.OnCommand;
    public float delay = 0;
    public float elapsedDelay = 0;
    public string dockTimerName = "";
    public string leaveTimerName = "";
    DateTime lastTimerTick;
    public bool playerWasInCockpit = false;
    private bool counting = false;

    public bool CheckDelay(bool reset)
    {
        if (reset) { elapsedDelay = 0; counting = false; return false; }
        counting = true;
        return elapsedDelay > delay;
    }

    public void Reset()
    {
        CheckDelay(true);
        playerWasInCockpit = false;
    }

    public void UpdateTimer()
    {
        if (counting)
            if ((DateTime.Now - lastTimerTick).TotalSeconds > 1)
            {
                elapsedDelay++;
                lastTimerTick = DateTime.Now;
            }
    }

    public bool HasDelay()
    {
        switch (trigger)
        {
            case UndockTrigger.OnPlayerEntered: return true;
            case UndockTrigger.OnTimerDelay: return true;
        }
        return false;
    }
}

class ItemInfo
{
    public String name = "";
    public String type = "";
    public int amount = 0;
    public ItemLocation location = ItemLocation.All;

    public ItemInfo(String name, String type, int amount, ItemLocation location)
    {
        this.name = name;
        this.type = type;
        this.amount = amount;
        this.location = location;
    }
}

class DistanceEntry
{
    public DistanceEntry(Vector3 pos, float dist)
    {
        this.pos = pos;
        this.dist = dist;
    }
    public Vector3 pos;
    public float dist;
}

class ShipInfo
{
    public DateTime lastMessageTime;
    public DateTime commandSentTime;
    public String id = "";
    public String name = "";
    public String version = "";
    public String statusMessage = "";
    public String stateText = "";
    public String menuText = "";
    public String pendingCommand = "";
    public Vector3 position = new Vector3();
    public List<ItemInfo> items = new List<ItemInfo>();
    public ShipMode shipMode = ShipMode.Unknown;
    public CommandState commandState;
    public float maxVolume;
    public float currentVolume;
    public int menuIndex;
    public int distance = 0;
    public int totalItems = 0;

    public bool IsTimedOut()
    {
        return (DateTime.Now - lastMessageTime).TotalSeconds > 10;
    }

    public bool IsCommandTimedOut()
    {
        if (commandState != CommandState.Pending) return false;
        return (DateTime.Now - commandSentTime).TotalSeconds >= 4;
    }

    public ShipInfo(String id) { this.id = id; }

    public CommandState ProcessCommand(String cmd, bool send, bool clear, bool checkReceived)
    {
        if (cmd == "" && !clear) return CommandState.None;
        if (commandState == CommandState.Pending && IsCommandTimedOut())
            commandState = CommandState.NoAnswer;

        if (send)
        {
            pendingCommand = cmd;
            commandState = CommandState.Pending;
            commandSentTime = DateTime.Now;
            return commandState;
        }
        else if (clear)
        {
            pendingCommand = "";
            commandState = CommandState.None;
        }
        else if (pendingCommand == cmd)
        {
            if (checkReceived) commandState = CommandState.Received;
            return commandState;
        }
        return CommandState.None;
    }
}


// ========================== ELEMENT CODE (user-customizable) ==========================

public String GetElementCode(String itemName)
{
    switch (itemName)
    {
        case "IRON": return "Fe";
        case "NICKEL": return "Ni";
        case "COBALT": return "Co";
        case "MAGNESIUM": return "Mg";
        case "SILICON": return "Si";
        case "SILVER": return "Ag";
        case "GOLD": return "Au";
        case "PLATINUM": return "Pt";
        case "URANIUM": return "U";
        default: return "";
    }
}

// ========================== 04_Fields.cs ==========================
// ========================== FIELDS ==========================

IMyGridTerminalSystem gridTerminalSystem;

// Ship state
Vector3 referencePosition = new Vector3();
Vector3 shipPosition = new Vector3();
Vector3 forwardDirection = new Vector3();
Vector3 leftDirection = new Vector3();
Vector3 downDirection = new Vector3();
DateTime lastScanTime = new DateTime();
bool firstRun = true;
int tickCounter = 0;
bool isSlowTick = false;
bool isTick = false;
bool needPositionUpdate = true;
bool wasSetupError = false;
bool isAligning = false;
bool initialized = false;
float shipSpeed = 0;
float shipMass = 0;
int scanPhase = 0;
int updatePhase = 0;
int scanCountdown = 0;
int broadcastCountdown = 0;
float maxInstructionPercent = 0;
float avgInstructionCount = 0;
double maxRuntime = 0;
float avgRuntime = 0;
List<int> instructionSamples = new List<int>();
List<int> runtimeSamples = new List<int>();

// Ship configuration
ShipMode shipMode = ShipMode.Unknown;
int setupErrorLevel = 0;
float toolWidth = 0;
float toolHeight = 0;
float shipDiameter = 0;

// Block references
IMyRemoteControl remoteControl;
IMySensorBlock sensor;
List<IMyTimerBlock> timerBlocks = new List<IMyTimerBlock>();
List<IMyShipConnector> connectors = new List<IMyShipConnector>();
List<IMyThrust> thrusters = new List<IMyThrust>();
List<IMyGyro> gyros = new List<IMyGyro>();
List<IMyTerminalBlock> workTools = new List<IMyTerminalBlock>();
List<IMyLandingGear> landingGears = new List<IMyLandingGear>();
List<IMyReactor> reactors = new List<IMyReactor>();
List<IMyConveyorSorter> sorters = new List<IMyConveyorSorter>();
List<IMyGasTank> hydrogenTanks = new List<IMyGasTank>();
List<IMyTerminalBlock> allGridBlocks = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> cargoBlocks = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> allInventoryBlocks = new List<IMyTerminalBlock>();
List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
List<IMyTextPanel> lcdPanels = new List<IMyTextPanel>();
List<IMyTextSurface> textSurfaces = new List<IMyTextSurface>();
List<IMyTextPanel> debugPanels = new List<IMyTextPanel>();
IMyTerminalBlock referenceWorkTool = null;

// Dock and job positions
WaypointInfo homeDock = new WaypointInfo();
WaypointInfo homeDockBackup = new WaypointInfo();
WaypointInfo jobDockBackup = new WaypointInfo();
bool useOldHome = false;
bool useOldConnector2 = false;
bool useOldPath = false;

// Path recording
bool isRecording = false;
List<String> thrusterTypeNames = new List<string>();
List<WaypointInfo> waypoints = new List<WaypointInfo>();
List<WaypointInfo> waypointsBackup = new List<WaypointInfo>();
int dockDetectCounter = 0;
double waypointSpacing = 0;

// Job state
WaypointInfo jobPosition = new WaypointInfo();
Vector3 miningForward;
Vector3 miningDown;
JobState jobState = JobState.NoJob;
StartPosition jobStartPos = StartPosition.TopLeft;
int jobWidth = 0;
int jobHeight = 0;
double jobProgress = 0;
bool damageCheckActive = false;

// Job config
UndockConfig undockConfig1 = new UndockConfig();
UndockConfig undockConfig2 = new UndockConfig();
DamageBehavior onDamageBehavior = DamageBehavior.ReturnHome;
bool whenDoneReturnHome = true;
StartPosition startPosition = StartPosition.TopLeft;
int configWidth = 3;
int configHeight = 3;
int configDepth = 30;
DepthMode depthMode = DepthMode.Default;
bool weightLimitEnabled = true;
bool invBalancingEnabled = true;
bool enableDrillsBothWays = true;
bool toggleSortersEnabled = false;
EjectionMode ejectionMode = EjectionMode.Off;
bool unloadIce = true;
float maxLoadPercent = 90;
float minBatteryPercent = 20;
float minUraniumKg = 5;
float minHydrogenPercent = 20;
float accelerationFactor = 0.70f;
float workSpeedForward = 1.50f;
float workSpeedBackward = 2.50f;
float widthOverlap = 0f;
float heightOverlap = 0f;

// Navigation state
NavState currentNavState;
JobState lastJobState;
NavState stopAtState;
PathFollowData pathFollowData = null;

// Hole tracking
int holeCol = 0;
int holeRow = 0;
int currentHoleIndex = 0;
int lastHoleIndex = 0;
int runtimeMaxDepth = 30;
int pathWaypointIndex = 0;
bool stateFirstTick = true;
Vector3 miningTargetPos;
double distToTarget = 0;
bool returnToJobAfter = false;
int waypointsRemaining = 0;
int stuckCounter = 0;
int autoDepthCounter = 0;
int pathDirection = 0;
int oreCountBeforeEject = 0;
Vector3 pathSegmentDir = new Vector3();
float lastDistToTarget = 0;
float stuckSensitivity = 0;
int waypointCalcIndex = 0;
float lastOreAmount = 0;
float autoDepthRef = 0;
float currentMineDepth = 0;
float maxMineDepth = 0;

// Mining state
bool isFirstDescent = false;
bool unloadComplete = false;
bool uraniumOk = false;
bool hydrogenOk = false;
bool batteryOk = false;
DateTime dockStartTime = new DateTime();
WaypointInfo currentPathWaypoint = null;

// Navigation target
WaypointInfo currentDockTarget = null;

// Energy
float batteryPercent = 0;
BatteryState batteryState;
float hydrogenPercent = 0;
float uraniumAmount = 0;
String lowestUraniumReactor = "";

// Cargo
float loadPercent = 0;
float currentVolume = 0;
float maxVolume = 0;
List<ItemInfo> itemList = new List<ItemInfo>();

// Weight limit
float maxWeight = 0;
bool simulateShipFull = false;

// Flight control
bool thrustEnabled = false;
bool slowOnMisalign = false;
bool thrustActive = false;
bool softApproach = false;
float maxApproachSpeed = 0;
float pathSpeed = 0;
Vector3 flightPathDir = new Vector3();
Vector3 navTargetPos = new Vector3();
Vector3 thrustMultiplier = new Vector3(1, 1, 1);
float thrustEfficiency = 1;
Vector3 thrustForce = new Vector3();

// Gyro
bool isAligned = false;
bool gyroActive = false;
bool lookAtTarget = false;
bool alignToGravity = false;
float currentAngleError = 0;
float alignThreshold = 2;
Vector3 targetLeft;
Vector3 targetForward;
Vector3 targetDown;

// Thrust maps
float[,] totalThrustMap = new float[3, 2];
Dictionary<String, float[,]> thrustByType = new Dictionary<string, float[,]>();

// Inventory balance
Dictionary<String, float> volumePerItem = new Dictionary<String, float>();
int balanceFailCount = 0;

// Profiling
Dictionary<String, float[]> profilingData = new Dictionary<string, float[]>();
float profilingStart;
bool debugMode = false;

// Menu
int animCounter = 0;
int infoRotateIndex = 0;
int infoToggle = 0;
int infoSection = 0;
String statusMessage = "";
int[] menuIndexPerPage = new int[Enum.GetValues(typeof(MenuPage)).Length];
bool menuJustNavigated = false;
int menuItemCount = 0;
int menuIndex = 0;
MenuPage currentMenu = MenuPage.Main;

// UI scroll state
int timerIdx1 = 0, timerIdx2 = 0, timerIdx3 = 0, timerIdx4 = 0;
int scrollOffset1 = 0, scrollOffset2 = 0;

// Broadcast
bool broadcastEnabled = false;
bool antennaError = false;
String shipName = "";
String broadcastChannel = "";
IMyBroadcastListener broadcastListener = null;
bool firstBroadcastInit = true;

// Controller
List<ShipInfo> connectedShips = null;
ShipInfo selectedShip = null;
int controllerPageCount1 = 0, controllerPageCount2 = 0;
int controllerPageIdx1 = 0, controllerPageIdx2 = 0;
int controllerScrollState1 = 0, controllerScrollState2 = 0, controllerScrollState3 = 0;

// Misc
String lastArgument = "";
Exception lastError = null;
bool resetRequested = false;
JobState savedJobState = JobState.NoJob;
bool dampenerState = true;
String waitingStatusText = "";
bool undockRequested = false;
bool sorterState = true;
int initialDamageCount = 0;
int[] spiralState = null;
Vector3 cachedGridPos = new Vector3();
bool gridPosCached = false;

bool IsOnSameGrid(IMyTerminalBlock block) => block.CubeGrid == Me.CubeGrid;

// ========================== 05_Constructor.cs ==========================
// ========================== CONSTRUCTOR ==========================

Program()
{
    gridTerminalSystem = GridTerminalSystem;
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    if (HasTag(Me, controllerTag, true))
        shipMode = ShipMode.Controller;

    NavigateToMenu(MenuPage.Main);

    if (shipMode != ShipMode.Controller)
    {
        statusMessage = "Welcome to [PAM]!";
        DataResult restoreResult = RestoreData();

        if (shipMode == ShipMode.Unknown)
        {
            List<IMyShipDrill> drills = new List<IMyShipDrill>();
            List<IMyShipGrinder> grinders = new List<IMyShipGrinder>();
            gridTerminalSystem.GetBlocksOfType(drills, q => q.CubeGrid == Me.CubeGrid);
            gridTerminalSystem.GetBlocksOfType(grinders, q => q.CubeGrid == Me.CubeGrid);

            if (drills.Count > 0)
            {
                shipMode = ShipMode.Miner;
                statusMessage = "Miner mode enabled!";
            }
            else if (grinders.Count > 0)
            {
                shipMode = ShipMode.Grinder;
                statusMessage = "Grinder mode enabled!";
            }
            else
            {
                shipMode = ShipMode.Shuttle;
                ShowStatusMessage(StatusMsg.Shuttle);
            }
        }

        if (restoreResult == DataResult.Success) needPositionUpdate = false;
        if (restoreResult == DataResult.Failed) statusMessage = "Data restore failed!";
        if (restoreResult == DataResult.NewVersion) statusMessage = "New version, wipe data";
    }
}

// ========================== 06_Main.cs ==========================
// ========================== MAIN ==========================

void Main(string argument, UpdateType updateSource)
{
    try
    {
        if (lastError != null) { HandleError(); PrintDisplays(); return; }

        isTick = (updateSource & UpdateType.Update10) != 0;
        if (isTick) tickCounter++;
        isSlowTick = tickCounter >= 10;
        if (isSlowTick) tickCounter = 0;

        if (isTick)
        {
            animCounter++;
            if (animCounter > 4) animCounter = 0;
            scanCountdown = Math.Max(0, 10 - (DateTime.Now - lastScanTime).Seconds);
        }

        if (argument != "") lastArgument = argument;

        if (shipMode != ShipMode.Controller)
            ShipMainLoop(argument);
        else
            ControllerMainLoop(argument);

        menuJustNavigated = false;

        try
        {
            int currentInstructions = Runtime.CurrentInstructionCount;
            float ratio = SafeDiv(currentInstructions, Runtime.MaxInstructionCount);
            if (ratio > 0.90) statusMessage = "Max. instructions >90%";
            if (ratio > maxInstructionPercent) maxInstructionPercent = ratio;

            if (debugMode)
            {
                instructionSamples.Add(currentInstructions);
                while (instructionSamples.Count > 10) instructionSamples.RemoveAt(0);
                avgInstructionCount = 0;
                for (int i = 0; i < instructionSamples.Count; i++) avgInstructionCount += instructionSamples[i];
                avgInstructionCount = SafeDiv(SafeDiv(avgInstructionCount, instructionSamples.Count), Runtime.MaxInstructionCount);

                double lastRunMs = Runtime.LastRunTimeMs;
                if (initialized && lastRunMs > maxRuntime) maxRuntime = lastRunMs;
                runtimeSamples.Add((int)(lastRunMs * 1000f));
                while (runtimeSamples.Count > 10) runtimeSamples.RemoveAt(0);
                avgRuntime = 0;
                for (int i = 0; i < runtimeSamples.Count; i++) avgRuntime += runtimeSamples[i];
                avgRuntime = SafeDiv(avgRuntime, runtimeSamples.Count) / 1000f;
            }
        }
        catch { maxInstructionPercent = 0; }
    }
    catch (Exception e) { lastError = e; }
}

// ========================== 07_ShipLoop.cs ==========================
// ========================== SHIP MAIN LOOP ==========================

void ShipMainLoop(string argument)
{
    bool shouldBroadcast = false;
    String broadcastExtra = "";

    if (setupErrorLevel <= 1 && !IsInternalMessage(argument))
        ProcessShipArgument(argument);

    if (broadcastListener != null && broadcastListener.HasPendingMessage)
    {
        MyIGCMessage msg = broadcastListener.AcceptMessage();
        String data = (string)msg.Data;
        String senderId = "";
        if (setupErrorLevel <= 1 && ParseBroadcastMessage(ref data, out senderId, out broadcastExtra) && senderId == BROADCAST_SELF_ID)
        {
            ProcessShipArgument(data);
            shouldBroadcast = true;
        }
    }

    bool isIdleOptimize = initialized && currentNavState == NavState.Idle && !isAligning && !shouldBroadcast && scanPhase == 0 && !isRecording;
    if (isSlowTick && currentNavState != NavState.Idle) shouldBroadcast = true;

    if ((isTick && !isIdleOptimize) || (isSlowTick && isIdleOptimize))
    {
        // Scan phase 1
        if (scanPhase == 0 && (scanCountdown <= 0 || firstRun))
        {
            wasSetupError = setupErrorLevel > 0;
            setupErrorLevel = 0;
            scanPhase = 1;
            StartProfile();
            ScanBlocks1();
            InitBroadcast();
            EndProfile("Scan 1");
        }
        // Scan phase 2
        else if (scanPhase == 1)
        {
            scanPhase = 2;
            StartProfile();
            ScanBlocks2();
            EndProfile("Scan 2");
        }
        // Scan phase 3
        else if (scanPhase == 2)
        {
            scanPhase = 0;
            StartProfile();
            ScanBlocks3();
            EndProfile("Scan 3");
            lastScanTime = DateTime.Now;

            if (setupErrorLevel <= 1 && needPositionUpdate)
                referencePosition = GetLocalPosition(remoteControl, remoteControl.CenterOfMass);
            needPositionUpdate = false;

            if (firstRun) { firstRun = false; StopAll(); }
            if (wasSetupError && setupErrorLevel == 0) statusMessage = "Setup complete";
        }
        else
        {
            // Inventory balancing
            if (jobState == JobState.Active && shipMode != ShipMode.Shuttle)
            {
                StartProfile();
                BalanceInventory();
                EndProfile("Inv balance");
            }

            // Cycled updates
            StartProfile();
            switch (updatePhase)
            {
                case 0: UpdateCargoLoad(); break;
                case 1: UpdateInventory(); break;
                case 2: UpdateBatteryState(); break;
                case 3: UpdateUranium(); break;
                case 4: UpdateHydrogen(); break;
                case 5: CheckDamage(); break;
                case 6: CalculateThrustVectors(remoteControl); break;
            }
            EndProfile("Update: " + updatePhase);

            updatePhase++;
            if (updatePhase > 6)
            {
                updatePhase = 0;
                initialized = true;

                if (savedJobState != JobState.NoJob)
                {
                    switch (savedJobState)
                    {
                        case JobState.ActiveHome: ContinueJob(); break;
                        case JobState.ActiveJob: ContinueJob(); break;
                        case JobState.Active: ContinueJob(); break;
                        case JobState.MoveHome: FlyToHomePosition(); break;
                        case JobState.MoveToJob: FlyToJobPosition(); break;
                    }
                    savedJobState = JobState.NoJob;
                }
            }
        }

        if (!firstRun)
        {
            if (!IsBlockValid(remoteControl, true))
            {
                remoteControl = null;
                needPositionUpdate = true;
                setupErrorLevel = 2;
            }

            if (setupErrorLevel >= 2 && currentNavState != NavState.Idle)
                StopAll();

            if (setupErrorLevel <= 1)
            {
                shipMass = remoteControl.CalculateShipMass().PhysicalMass;
                shipSpeed = (float)remoteControl.GetShipSpeed();
                shipPosition = TransformToWorld(remoteControl, referencePosition);
                forwardDirection = remoteControl.WorldMatrix.Forward;
                leftDirection = remoteControl.WorldMatrix.Left;
                downDirection = remoteControl.WorldMatrix.Down;
                UpdatePathRecording();

                if (currentNavState != NavState.Idle)
                {
                    isAligning = false;
                    SetDampeners(false);
                    CalculateMaxWeight(false);

                    String stateName = GetNavStateName(currentNavState) + " " + (int)currentNavState;
                    StartProfile();
                    UpdateNavigation();
                    UpdateProgress(false);
                    EndProfile(stateName);

                    StartProfile();
                    UpdateFlight();
                    EndProfile("Thruster");

                    StartProfile();
                    UpdateGyros();
                    EndProfile("Gyroscope");
                }
                else
                {
                    if (isAligning)
                    {
                        if (IsNearPlanet())
                        {
                            AlignToDown(downDirection, forwardDirection, leftDirection, 0.25f, true);
                            UpdateGyros();
                            statusMessage = "Aligning to planet: " + Math.Round(currentAngleError - 0.25f, 2) + "°";
                            if (isAligned) HandleAlignment(true, true);
                        }
                        else HandleAlignment(true, true);
                    }
                }
                undockRequested = false;
            }
        }
    }

    StartProfile();
    PrintDisplays();
    EndProfile("Print");

    if (shouldBroadcast || broadcastCountdown <= 0)
    {
        StartProfile();
        BroadcastShipState(broadcastExtra);
        EndProfile("Broadcast");
        broadcastCountdown = 4;
    }
    else if (isSlowTick) broadcastCountdown--;
}

// ========================== 08_Commands.cs ==========================
// ========================== ARGUMENT PROCESSING ==========================

void ProcessShipArgument(string argument)
{
    if (argument == "") return;
    var parts = argument.ToUpper().Split(' ');
    parts.DefaultIfEmpty("");
    var cmd = parts.ElementAtOrDefault(0);
    var arg1 = parts.ElementAtOrDefault(1);
    var arg2 = parts.ElementAtOrDefault(2);
    var arg3 = parts.ElementAtOrDefault(3);
    String invalidMsg = "Invalid argument: " + argument;
    bool unhandled = false;

    switch (cmd)
    {
        case "UP": this.MenuUp(false); break;
        case "DOWN": this.MenuDown(false); break;
        case "UPLOOP": this.MenuUp(true); break;
        case "DOWNLOOP": this.MenuDown(true); break;
        case "APPLY": this.BuildMenuScreen(true); break;
        case "MRES": menuIndex = 0; break;
        case "STOP": this.StopAll(); break;
        case "PATHHOME": { this.StopAll(); this.StartPathRecording(); } break;
        case "PATH": { this.StopAll(); this.StartPathRecording(); homeDock.isSet = true; } break;
        case "START": { this.StopAll(); StartNewJob(); } break;
        case "ALIGN": { HandleAlignment(!isAligning, false); } break;
        case "CONT": { this.StopAll(); this.ContinueJob(); } break;
        case "JOBPOS": { this.StopAll(); this.FlyToJobPosition(); } break;
        case "HOMEPOS": { this.StopAll(); this.FlyToHomePosition(); } break;
        case "FULL": { simulateShipFull = true; } break;
        case "RESET": { resetRequested = true; setupErrorLevel = 2; } break;
        default: unhandled = true; break;
    }

    if (shipMode != ShipMode.Shuttle)
    {
        switch (cmd)
        {
            case "SHUTTLE": { EnableShuttleMode(); } break;
            case "CFGS": { if (!ConfigSize(arg1, arg2, arg3)) statusMessage = invalidMsg; } break;
            case "CFGB": { if (!ConfigBehavior(arg1, arg2)) statusMessage = invalidMsg; } break;
            case "CFGL":
            {
                if (!ParseConfig(ref maxLoadPercent, true, ConfigType.MaxLoad, arg1, "") || !ConfigWeightLimit(arg2))
                    statusMessage = invalidMsg;
            } break;
            case "CFGE":
            {
                if (!ParseConfig(ref minUraniumKg, true, ConfigType.MinUranium, arg1, "IG") ||
                    !ParseConfig(ref minBatteryPercent, true, ConfigType.MinBattery, arg2, "IG") ||
                    !ParseConfig(ref minHydrogenPercent, true, ConfigType.MinHydrogen, arg3, "IG"))
                    statusMessage = invalidMsg;
            } break;
            case "CFGA": { if (!ParseConfig(ref accelerationFactor, false, ConfigType.Acceleration, arg1, "")) statusMessage = invalidMsg; } break;
            case "CFGW":
            {
                if (!ParseConfig(ref workSpeedForward, false, ConfigType.WorkSpeed, arg1, "") ||
                    !ParseConfig(ref workSpeedBackward, false, ConfigType.WorkSpeed, arg2, ""))
                    statusMessage = invalidMsg;
            } break;
            case "NEXT": { AdvanceHole(false); } break;
            case "PREV": { AdvanceHole(true); } break;
            default: if (unhandled) statusMessage = invalidMsg; break;
        }
    }
    else
    {
        switch (cmd)
        {
            case "UNDOCK": { undockRequested = true; } break;
            default: if (unhandled) statusMessage = invalidMsg; break;
        }
    }
}


// ========================== HELP TEXT ==========================

String GetHelpText()
{
    String text = "\n\n" + "Run-arguments: (Type without:[ ])\n" + "———————————————\n" +
        "[UP] Menu navigation up\n" + "[DOWN] Menu navigation down\n" + "[APPLY] Apply menu point\n\n" +
        "[UPLOOP] \"UP\" + looping\n" + "[DOWNLOOP] \"DOWN\" + looping\n" +
        "[PATHHOME] Record path, set home\n" + "[PATH] Record path, use old home\n" +
        "[START] Start job\n" + "[STOP] Stop every process\n" + "[CONT] Continue last job\n" +
        "[JOBPOS] Move to job position\n" + "[HOMEPOS] Move to home position\n\n" +
        "[FULL] Simulate ship is full\n" + "[ALIGN] Align the ship to planet\n" + "[RESET] Reset all data\n";

    if (shipMode != ShipMode.Shuttle)
        text += "[SHUTTLE] Enable shuttle mode\n" + "[NEXT] Next hole\n" + "[PREV] Previous hole\n\n" +
            "[CFGS width height depth]*\n" + "[CFGB done damage]*\n" + "[CFGL maxload weightLimit]*\n" +
            "[CFGE minUr minBat minHyd]*\n" + "[CFGW forward backward]*\n" + "[CFGA acceleration]*\n" +
            "———————————————\n" +
            "*[CFGS] = Config Size:\n" + " e.g.: \"CFGS 5 3 20\"\n\n" +
            "*[CFGB] = Config Behaviour:\n" + " When done: [HOME,STOP]\n" + " On Damage: [HOME,JOB,STOP,IG]\n" +
            " e.g.: \"CFGB HOME IG\"\n\n" +
            "*[CFGL] = Config max load:\n" + " maxload: [10..95]\n" + " weight limit: [On/Off]\n" +
            " e.g.: \"CFGL 70 on\"\n\n" +
            "*[CFGE] = Config energy:\n" + " minUr (Uranium): [1..25, IG]\n" + " minBat (Battery): [5..30, IG]\n" +
            " minHyd (Hydrogen): [10..90, IG]\n" + " e.g.: \"CFGE 20 10 IG\"\n\n" +
            "*[CFGW] = Config work speed:\n" + " fwd: [0.5..10]\n" + " bwd: [0.5..10]\n" +
            " e.g.: \"CFGW 1.5 2\"\n\n" +
            "*[CFGA] = Config acceleration:\n" + " acceleration: [10..100]\n" + " e.g.: \"CFGA 80\"\n";
    else
        text += "[UNDOCK] Leave current connector\n\n";

    return text;
}

// ========================== 09_ShuttleConfig.cs ==========================
// ========================== SHUTTLE / ALIGNMENT / CONFIG ==========================

void EnableShuttleMode()
{
    StopAll();
    shipMode = ShipMode.Shuttle;
    ShowStatusMessage(StatusMsg.Shuttle);
    jobPosition.isSet = false;
    homeDock.isSet = false;
    sensor = null;
    workTools.Clear();
    jobState = JobState.NoJob;
}

void HandleAlignment(bool enable, bool done)
{
    if (!enable) statusMessage = "Aligning canceled";
    if (done) statusMessage = "Aligning done";
    if (done || !enable)
    {
        isAligning = false;
        StopGyroOverride();
        SetGyroOverride(false, 0, 0, 0, 0);
        return;
    }
    if (IsNearPlanet()) isAligning = true;
}

bool ConfigSize(String widthStr, String heightStr, String depthStr)
{
    bool wasActive = jobState == JobState.Active;
    int w, h, d;
    if (int.TryParse(widthStr, out w) && int.TryParse(heightStr, out h) && int.TryParse(depthStr, out d))
    {
        this.StopAll();
        configWidth = w; configHeight = h; configDepth = d;
        ValidateJobConfig(false);
        UpdateJobConfig(false, false);
        if (wasActive) ContinueJob();
        return true;
    }
    return false;
}

bool ConfigWeightLimit(String arg)
{
    if (arg == "ON") { weightLimitEnabled = true; return true; }
    if (arg == "OFF") { weightLimitEnabled = false; return true; }
    return false;
}

bool ConfigBehavior(String doneArg, String damageArg)
{
    bool ok = true;
    if (doneArg == "HOME") whenDoneReturnHome = true;
    else if (doneArg == "STOP") whenDoneReturnHome = false;
    else ok = false;

    if (damageArg == "HOME") onDamageBehavior = DamageBehavior.ReturnHome;
    else if (damageArg == "STOP") onDamageBehavior = DamageBehavior.Stop;
    else if (damageArg == "JOB") onDamageBehavior = DamageBehavior.FlyToJob;
    else if (damageArg == "IG") onDamageBehavior = DamageBehavior.Ignore;
    else ok = false;
    return ok;
}

// ========================== 10_MenuNav.cs ==========================
// ========================== MENU NAVIGATION ==========================

void NavigateToMenu(MenuPage page)
{
    menuIndexPerPage[(int)currentMenu] = menuIndex;
    menuIndex = menuIndexPerPage[(int)page];
    if (page == MenuPage.ShipList) menuIndex = 0;
    currentMenu = page;
    if (shipMode != ShipMode.Controller)
        UpdateSensor(currentMenu == MenuPage.JobSetup, false, 0, 0);
    menuJustNavigated = true;
}

void MenuUp(bool loop)
{
    if (menuIndex > 0) menuIndex--;
    else if (loop) menuIndex = menuItemCount - 1;
}

void MenuDown(bool loop)
{
    if (menuIndex < menuItemCount - 1) menuIndex++;
    else if (loop) menuIndex = 0;
}


// ========================== NAME/STATUS HELPERS ==========================

String ShowStatusMessage(StatusMsg msg)
{
    switch (msg)
    {
        case StatusMsg.Running: statusMessage = "Job is running"; break;
        case StatusMsg.ConnectorNotReady: statusMessage = "Connector not ready!"; break;
        case StatusMsg.ShipModified: statusMessage = "Ship modified, path outdated!"; break;
        case StatusMsg.Interrupted: statusMessage = "Interrupted by player!"; break;
        case StatusMsg.Shuttle: statusMessage = "Shuttle mode enabled!"; break;
    }
    return "";
}

String GetStartPosName(StartPosition pos)
{
    switch (pos)
    {
        case StartPosition.TopLeft: return "Top-Left";
        case StartPosition.Center: return "Center";
        default: return "";
    }
}

String GetDepthModeName(DepthMode mode)
{
    switch (mode)
    {
        case DepthMode.AutoOre: return "Auto" + (shipMode == ShipMode.Miner ? " (Ore)" : "");
        case DepthMode.AutoStone: return "Auto (+Stone)";
        case DepthMode.Default: return "Default";
        default: return "";
    }
}

String GetJobStateName(JobState state)
{
    switch (state)
    {
        case JobState.NoJob: return "No job";
        case JobState.Paused: return "Job paused";
        case JobState.Active: return "Job active";
        case JobState.ActiveHome: return "Job active";
        case JobState.ActiveJob: return "Job active";
        case JobState.Done: return "Job done";
        case JobState.Changed: return "Job changed";
        case JobState.MoveHome: return "Move home";
        case JobState.MoveToJob: return "Move to job";
    }
    return "";
}

String GetDamageBehaviorName(DamageBehavior behavior)
{
    switch (behavior)
    {
        case DamageBehavior.ReturnHome: return "Return home";
        case DamageBehavior.FlyToJob: return "Fly to job pos";
        case DamageBehavior.Stop: return "Stop";
        case DamageBehavior.Ignore: return "Ignore";
    }
    return "";
}

String GetEjectionModeName(EjectionMode mode)
{
    switch (mode)
    {
        case EjectionMode.Off: return "Off";
        case EjectionMode.DropPosStone: return "Drop pos (Stone) ";
        case EjectionMode.DropPosStoIce: return "Drop pos (Sto.+Ice)";
        case EjectionMode.CurPosStone: return "Cur. pos (Stone)";
        case EjectionMode.CurPosStoIce: return "Cur. pos (Sto.+Ice)";
        case EjectionMode.InMotionStone: return "In motion (Stone)";
        case EjectionMode.InMotionStoIce: return "In motion (Sto.+Ice)";
    }
    return "";
}

String GetBatteryStateName(BatteryState state)
{
    switch (state)
    {
        case BatteryState.None: return "No batteries";
        case BatteryState.Charging: return "Charging";
        case BatteryState.Discharging: return "Discharging";
    }
    return "";
}

String GetNavStateName(NavState state)
{
    String jobLabel = shipMode == ShipMode.Shuttle ? "target" : "job";
    switch (state)
    {
        case NavState.Idle: return "Idle";
        case NavState.FlyToXY: return "Flying to XY position";
        case NavState.Mining: return shipMode == ShipMode.Grinder ? "Grinding" : "Mining";
        case NavState.Returning: return "Returning";
        case NavState.FlyToDropPos: return "Flying to drop pos";
        case NavState.ReturnToDock: return "Returning to dock";
        case NavState.FlyToDockArea: return "Flying to dock area";
        case NavState.FlyToJobArea: return "Flying to job area";
        case NavState.FlyToPath: return "Flying to path";
        case NavState.FlyToJobPos: return "Flying to job position";
        case NavState.ApproachDock: return "Approaching dock";
        case NavState.Docking: return "Docking";
        case NavState.AlignDock: return "Aligning";
        case NavState.AlignJob: return "Aligning";
        case NavState.RetryDocking: return "Retry docking";
        case NavState.Unloading: return "Unloading";
        case NavState.WaitForCommand: return waitingStatusText;
        case NavState.Undocking: return "Undocking";
        case NavState.Charging: return "Charging batteries";
        case NavState.WaitUranium: return "Waiting for uranium";
        case NavState.FillHydrogen: return "Filling up hydrogen";
        case NavState.WaitEjection: return "Waiting for ejection";
        case NavState.WaitEjectionDrop: return "Waiting for ejection";
        case NavState.FlyToDropPos2: return "Flying to drop pos";
    }
    return "";
}

String GetUndockTriggerName(UndockTrigger trigger)
{
    switch (trigger)
    {
        case UndockTrigger.OnCommand: return "On \"Undock\" command";
        case UndockTrigger.OnPlayerEntered: return "On player entered cockpit";
        case UndockTrigger.OnShipFull: return "On ship is full";
        case UndockTrigger.OnShipEmpty: return "On ship is empty";
        case UndockTrigger.OnTimerDelay: return "On time delay";
        case UndockTrigger.OnBatteriesLow25: return "On batteries empty(<25%)";
        case UndockTrigger.OnBatteriesEmpty: return "On batteries empty(=0%)";
        case UndockTrigger.OnBatteriesFull: return "On batteries full";
        case UndockTrigger.OnHydrogenLow25: return "On hydrogen empty(<25%)";
        case UndockTrigger.OnHydrogenEmpty: return "On hydrogen empty(=0%)";
        case UndockTrigger.OnHydrogenFull: return "On hydrogen full";
    }
    return "";
}

// ========================== 11_MenuBuilder.cs ==========================
// ========================== MENU SCREEN BUILDER ==========================
// (BuildMenuScreen is extremely large - handles all menu pages)

bool AddMenuItem(ref String text, int selected, int index, bool applyAction, String label)
{
    menuItemCount += 1;
    if (selected == index) label = ">" + label + (animCounter >= 2 ? " ." : "");
    else label = " " + label;
    text += label + "\n";
    return selected == index && applyAction;
}

String BuildMenuScreen(bool applyAction)
{
    int idx = 0;
    int sel = menuIndex;
    menuItemCount = 0;
    String separator = "———————————————\n";
    String thinSep = "--------------------------------------------\n";
    String text = "";

    text += GetJobStateName(jobState) + " | " + (homeDock.isSet ? "Ready to dock" : "No dock") + "\n";
    text += separator;

    double targetDist = Math.Max(Math.Round(this.distToTarget), 0);

    // ---- MAIN MENU ----
    if (currentMenu == MenuPage.Main)
    {
        bool isShuttle = shipMode == ShipMode.Shuttle;
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Record path & set home")) StartPathRecording();
        if (shipMode == ShipMode.Miner)
            if (AddMenuItem(ref text, sel, idx++, applyAction, " Setup mining job")) NavigateToMenu(MenuPage.JobSetup);
        if (shipMode == ShipMode.Grinder)
            if (AddMenuItem(ref text, sel, idx++, applyAction, " Setup grinding job")) NavigateToMenu(MenuPage.JobSetup);
        if (shipMode == ShipMode.Shuttle)
            if (AddMenuItem(ref text, sel, idx++, applyAction, " Setup shuttle job")) NavigateToMenu(MenuPage.ShuttlePage1);
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Continue job")) ContinueJob();
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Fly to home position")) FlyToHomePosition();
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Fly to job position")) FlyToJobPosition();
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Behavior settings"))
            if (isShuttle) NavigateToMenu(MenuPage.ShuttleBehavior); else NavigateToMenu(MenuPage.BehaviorSettings);
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Info")) NavigateToMenu(MenuPage.InfoPage);
        if (shipMode != ShipMode.Shuttle)
            if (AddMenuItem(ref text, sel, idx++, applyAction, " Help")) NavigateToMenu(MenuPage.HelpPage);
    }
    // ---- JOB SETUP ----
    else if (currentMenu == MenuPage.JobSetup)
    {
        double widthMeters = Math.Round(configWidth * toolWidth, 1);
        double heightMeters = Math.Round(configHeight * toolHeight, 1);
        String subText = "";

        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Start new job!")) StartNewJob();
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Change current job")) { UpdateJobConfig(false, false); NavigateToMenu(MenuPage.Main); }
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Width + (Width: " + configWidth + " = " + widthMeters + "m)")) { AdjustValue(ref configWidth, 5, 20, 1); ValidateJobConfig(true); }
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Width -")) { AdjustValue(ref configWidth, -5, 20, -1); ValidateJobConfig(true); }
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Height + (Height: " + configHeight + " = " + heightMeters + "m)")) { AdjustValue(ref configHeight, 5, 20, 1); ValidateJobConfig(true); }
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Height -")) { AdjustValue(ref configHeight, -5, 20, -1); ValidateJobConfig(true); }
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Depth + (" + (depthMode == DepthMode.Default ? "Depth" : "Min") + ": " + configDepth + "m)")) { AdjustValue(ref configDepth, 5, 50, 2); ValidateJobConfig(true); }
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Depth -")) { AdjustValue(ref configDepth, -5, 50, -2); ValidateJobConfig(true); }
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Depth mode: " + GetDepthModeName(depthMode))) { depthMode = CycleEnum(depthMode); }
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Start pos: " + GetStartPosName(startPosition))) { startPosition = CycleEnum(startPosition); }
        if (shipMode == ShipMode.Grinder && depthMode == DepthMode.AutoStone) depthMode = CycleEnum(depthMode);
        text += ScrollText(8, subText, sel, ref scrollOffset1);
    }
    // ---- SHUTTLE PAGE 1 ----
    else if (currentMenu == MenuPage.ShuttlePage1)
    {
        float[] delayValues = new float[] { 0, 3, 10, 30, 60, 300, 600, 1200, 1800 };
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Next")) NavigateToMenu(MenuPage.ShuttlePage2);
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Back")) NavigateToMenu(MenuPage.Main);
        text += " Leave connector 1:\n";
        if (AddMenuItem(ref text, sel, idx++, applyAction, " - " + GetUndockTriggerName(undockConfig1.trigger))) undockConfig1.trigger = CycleEnum(undockConfig1.trigger);
        if (!undockConfig1.HasDelay()) text += "\n";
        else if (AddMenuItem(ref text, sel, idx++, applyAction, " - Delay: " + FormatTime((int)undockConfig1.delay))) undockConfig1.delay = CycleArrayValue(undockConfig1.delay, delayValues);
        text += " Leave connector 2:\n";
        if (AddMenuItem(ref text, sel, idx++, applyAction, " - " + GetUndockTriggerName(undockConfig2.trigger))) undockConfig2.trigger = CycleEnum(undockConfig2.trigger);
        if (!undockConfig2.HasDelay()) text += "\n";
        else if (AddMenuItem(ref text, sel, idx++, applyAction, " - Delay: " + FormatTime((int)undockConfig2.delay))) undockConfig2.delay = CycleArrayValue(undockConfig2.delay, delayValues);
    }
    // ---- SHUTTLE PAGE 2 ----
    else if (currentMenu == MenuPage.ShuttlePage2)
    {
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Start job!")) StartNewJob();
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Back")) NavigateToMenu(MenuPage.ShuttlePage1);
        text += " Timer: \"Docking connector 1\":\n";
        if (AddMenuItem(ref text, sel, idx++, applyAction, " = " + (undockConfig1.dockTimerName != "" ? undockConfig1.dockTimerName : "-"))) undockConfig1.dockTimerName = NextTimerBlock(ref timerIdx1);
        text += " Timer: \"Leaving connector 1\":\n";
        if (AddMenuItem(ref text, sel, idx++, applyAction, " = " + (undockConfig1.leaveTimerName != "" ? undockConfig1.leaveTimerName : "-"))) undockConfig1.leaveTimerName = NextTimerBlock(ref timerIdx3);
        text += " Timer: \"Docking connector 2\":\n";
        if (AddMenuItem(ref text, sel, idx++, applyAction, " = " + (undockConfig2.dockTimerName != "" ? undockConfig2.dockTimerName : "-"))) undockConfig2.dockTimerName = NextTimerBlock(ref timerIdx2);
        text += " Timer: \"Leaving connector 2\":\n";
        if (AddMenuItem(ref text, sel, idx++, applyAction, " = " + (undockConfig2.leaveTimerName != "" ? undockConfig2.leaveTimerName : "-"))) undockConfig2.leaveTimerName = NextTimerBlock(ref timerIdx4);
    }
    // ---- JOB RUNNING ----
    else if (currentMenu == MenuPage.JobRunning)
    {
        String loadText = maxLoadPercent + " %";
        if (isSlowTick) infoRotateIndex++;
        if (infoRotateIndex > 1) { infoRotateIndex = 0; infoToggle++; if (infoToggle > 1) infoToggle = 0; }
        bool[] skipSection = new bool[] { reactors.Count == 0, batteryState == BatteryState.None, hydrogenTanks.Count == 0 };
        int tries = 0;
        while (true)
        {
            tries++;
            infoSection++;
            if (infoSection > skipSection.Length - 1) infoSection = 0;
            if (tries >= skipSection.Length) break;
            if (!skipSection[infoSection]) break;
        }

        bool isShuttle = shipMode == ShipMode.Shuttle;
        if (!isShuttle && weightLimitEnabled && maxWeight != -1 && infoToggle == 0)
            loadText = maxWeight < 1000000 ? Math.Round(maxWeight) + " Kg" : Math.Round(maxWeight / 1000) + " t";

        if (AddMenuItem(ref text, sel, idx++, applyAction, " Stop!")) { StopAll(); NavigateToMenu(MenuPage.Main); }
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Behavior settings"))
            if (!isShuttle) NavigateToMenu(MenuPage.BehaviorSettings); else NavigateToMenu(MenuPage.ShuttleBehavior);
        if (!isShuttle) { if (AddMenuItem(ref text, sel, idx++, applyAction, " Next hole")) AdvanceHole(false); }
        else if (AddMenuItem(ref text, sel, idx++, applyAction, " Undock")) undockRequested = true;

        text += thinSep;
        if (!isShuttle) text += "Progress: " + Math.Round(jobProgress, 1) + " %\n";
        text += "State: " + GetNavStateName(currentNavState) + " " + targetDist + "m \n";
        text += "Load: " + loadPercent + " % Max: " + loadText + " \n";

        if (infoSection == 0) text += "Uranium: " + (reactors.Count == 0 ? "No reactors" : Math.Round(uraniumAmount, 1) + "Kg " + (minUraniumKg == -1 ? "" : " Min: " + minUraniumKg + " Kg")) + "\n";
        if (infoSection == 1) text += "Battery: " + (batteryState == BatteryState.None ? GetBatteryStateName(batteryState) : batteryPercent + "% " + (minBatteryPercent == -1 || isShuttle ? "" : " Min: " + minBatteryPercent + " %")) + "\n";
        if (infoSection == 2) text += "Hydrogen: " + (hydrogenTanks.Count == 0 ? "No tanks" : Math.Round(hydrogenPercent, 1) + "% " + (minHydrogenPercent == -1 || isShuttle ? "" : " Min: " + minHydrogenPercent + " %")) + "\n";
    }
    // ---- BEHAVIOR SETTINGS ----
    else if (currentMenu == MenuPage.BehaviorSettings)
    {
        String subText = "";
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Back")) { if (jobState == JobState.Active) NavigateToMenu(MenuPage.JobRunning); else NavigateToMenu(MenuPage.Main); }
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Max load: " + maxLoadPercent + "%")) AdjustConfig(ref maxLoadPercent, maxLoadPercent <= 80 ? -10 : -5, ConfigType.MaxLoad, false);
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Weight limit: " + (weightLimitEnabled ? "On" : "Off"))) weightLimitEnabled = !weightLimitEnabled;
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Ejection: " + GetEjectionModeName(ejectionMode))) ejectionMode = CycleEnum(ejectionMode);
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Toggle sorters: " + (toggleSortersEnabled ? "On" : "Off"))) { toggleSortersEnabled = !toggleSortersEnabled; if (toggleSortersEnabled) SetSortersEnabled(sorterState); }
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Unload ice: " + (unloadIce ? "On" : "Off"))) unloadIce = !unloadIce;
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Uranium: " + (minUraniumKg == -1 ? "Ignore" : "Min " + minUraniumKg + "Kg"))) AdjustConfig(ref minUraniumKg, (minUraniumKg > 5 ? -5 : -1), ConfigType.MinUranium, true);
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Battery: " + (minBatteryPercent == -1 ? "Ignore" : "Min " + minBatteryPercent + "%"))) AdjustConfig(ref minBatteryPercent, -5, ConfigType.MinBattery, true);
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Hydrogen: " + (minHydrogenPercent == -1 ? "Ignore" : "Min " + minHydrogenPercent + "%"))) AdjustConfig(ref minHydrogenPercent, -10, ConfigType.MinHydrogen, true);
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " When done: " + (whenDoneReturnHome ? "Return home" : "Stop"))) whenDoneReturnHome = !whenDoneReturnHome;
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " On damage: " + GetDamageBehaviorName(onDamageBehavior))) onDamageBehavior = CycleEnum(onDamageBehavior);
        if (AddMenuItem(ref subText, sel, idx++, applyAction, " Advanced...")) NavigateToMenu(MenuPage.AdvancedSettings);
        text += ScrollText(8, subText, sel, ref scrollOffset2);
    }
    // ---- ADVANCED SETTINGS ----
    else if (currentMenu == MenuPage.AdvancedSettings)
    {
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Back")) { if (jobState == JobState.Active) NavigateToMenu(MenuPage.JobRunning); else NavigateToMenu(MenuPage.Main); }
        if (AddMenuItem(ref text, sel, idx++, applyAction, (shipMode == ShipMode.Grinder ? " Grinder" : " Drill") + " inv. balancing: " + (invBalancingEnabled ? "On" : "Off"))) invBalancingEnabled = !invBalancingEnabled;
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Enable" + (shipMode == ShipMode.Grinder ? " grinders" : " drills") + ": " + (enableDrillsBothWays ? "Fwd + Bwd" : "Fwd"))) enableDrillsBothWays = !enableDrillsBothWays;
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Work speed fwd.: " + workSpeedForward + "m/s")) AdjustConfig(ref workSpeedForward, 0.5f, ConfigType.WorkSpeed, false);
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Work speed bwd.: " + workSpeedBackward + "m/s")) AdjustConfig(ref workSpeedBackward, 0.5f, ConfigType.WorkSpeed, false);
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Acceleration: " + Math.Round(accelerationFactor * 100f) + "%" + (accelerationFactor > 0.80f ? " (risky)" : ""))) AdjustConfig(ref accelerationFactor, 0.1f, ConfigType.Acceleration, false);
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Width overlap: " + widthOverlap * 100f + "%")) ChangeOverlap(true, 0.05f);
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Height overlap: " + heightOverlap * 100f + "%")) ChangeOverlap(false, 0.05f);
    }
    // ---- SHUTTLE BEHAVIOR ----
    else if (currentMenu == MenuPage.ShuttleBehavior)
    {
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Back")) { if (jobState == JobState.Active) NavigateToMenu(MenuPage.JobRunning); else NavigateToMenu(MenuPage.Main); }
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Max load: " + maxLoadPercent + "%")) AdjustConfig(ref maxLoadPercent, maxLoadPercent <= 80 ? -10 : -5, ConfigType.MaxLoad, false);
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Unload ice: " + (unloadIce ? "On" : "Off"))) unloadIce = !unloadIce;
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Uranium: " + (minUraniumKg == -1 ? "Ignore" : "Min " + minUraniumKg + "Kg"))) AdjustConfig(ref minUraniumKg, (minUraniumKg > 5 ? -5 : -1), ConfigType.MinUranium, true);
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Battery: " + (minBatteryPercent == -1 ? "Ignore" : "Charge up"))) minBatteryPercent = (minBatteryPercent == -1 ? 1 : -1);
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Hydrogen: " + (minHydrogenPercent == -1 ? "Ignore" : "Fill up"))) minHydrogenPercent = (minHydrogenPercent == -1 ? 1 : -1);
        if (AddMenuItem(ref text, sel, idx++, applyAction, " On damage: " + GetDamageBehaviorName(onDamageBehavior))) onDamageBehavior = CycleEnum(onDamageBehavior);
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Acceleration: " + Math.Round(accelerationFactor * 100f) + "%" + (accelerationFactor > 0.80f ? " (risky)" : ""))) AdjustConfig(ref accelerationFactor, 0.1f, ConfigType.Acceleration, false);
    }
    // ---- RECORDING ----
    else if (currentMenu == MenuPage.Recording)
    {
        double lastWpDist = 0;
        if (waypoints.Count > 0) lastWpDist = Vector3.Distance(waypoints.Last().position, shipPosition);
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Stop path recording")) StopPathRecording();
        if (shipMode != ShipMode.Shuttle)
        {
            if (AddMenuItem(ref text, sel, idx++, applyAction, " Home: " + (useOldHome ? "Use old home" : (homeDock.isSet ? "Was set! " : "none ")))) useOldHome = !useOldHome;
        }
        else
        {
            if (AddMenuItem(ref text, sel, idx++, applyAction, " Connector 1: " + (useOldHome ? "Use old connector" : (homeDock.isSet ? "Was set! " : "none ")))) useOldHome = !useOldHome;
            if (AddMenuItem(ref text, sel, idx++, applyAction, " Connector 2: " + (useOldConnector2 ? "Use old connector" : (jobPosition.isSet ? "Was set! " : "none ")))) useOldConnector2 = !useOldConnector2;
        }
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Path: " + (useOldPath ? "Use old path" : (waypoints.Count > 1 ? "Count: " + waypoints.Count : "none ")))) useOldPath = !useOldPath;
        text += thinSep;
        text += "Wp spacing: " + Math.Round(waypointSpacing) + "m\n";
    }
    // ---- FLYING STATUS ----
    else if (currentMenu == MenuPage.StatusOverview)
    {
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Stop")) { StopAll(); NavigateToMenu(MenuPage.Main); }
        text += thinSep;
        text += "State: " + GetNavStateName(currentNavState) + " \n";
        text += "Speed: " + Math.Round(shipSpeed, 1) + "m/s\n";
        text += "Target dist: " + targetDist + "m\n";
        text += "Wp count: " + waypoints.Count + "\n";
        text += "Wp left: " + waypointsRemaining + "\n";
    }
    // ---- INFO PAGE ----
    else if (currentMenu == MenuPage.InfoPage)
    {
        List<IMyTerminalBlock> damaged = GetDamagedBlocks();
        if (isSlowTick) infoRotateIndex++;
        if (infoRotateIndex >= damaged.Count) infoRotateIndex = 0;
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Next")) NavigateToMenu(MenuPage.InfoPage2);
        text += thinSep;
        text += "Version: " + VERSION + "\n";
        text += "Ship load: " + Math.Round(loadPercent, 1) + "% " + Math.Round(currentVolume, 1) + " / " + Math.Round(maxVolume, 1) + "\n";
        text += "Uranium: " + (reactors.Count == 0 ? "No reactors" : Math.Round(uraniumAmount, 1) + "Kg " + lowestUraniumReactor) + "\n";
        text += "Battery: " + (batteryState == BatteryState.None ? "" : batteryPercent + "% ") + GetBatteryStateName(batteryState) + "\n";
        text += "Hydrogen: " + (hydrogenTanks.Count == 0 ? "No tanks" : Math.Round(hydrogenPercent, 1) + "% ") + "\n";
        text += "Damage: " + (damaged.Count == 0 ? "None" : "" + (infoRotateIndex + 1) + "/" + damaged.Count + " " + damaged[infoRotateIndex].CustomName) + "\n";
    }
    // ---- INFO PAGE 2 ----
    else if (currentMenu == MenuPage.InfoPage2)
    {
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Back")) NavigateToMenu(MenuPage.Main);
        text += thinSep;
        text += "Next scan: " + scanCountdown + "s\n";
        text += "Ship size: " + Math.Round(toolWidth, 1) + "m " + Math.Round(toolHeight, 1) + "m " + Math.Round(shipDiameter, 1) + "m \n";
        text += "Broadcast: " + (broadcastEnabled ? "Online - " + shipName : "Offline") + "\n";
        text += "Max Instructions: " + Math.Round(maxInstructionPercent * 100f, 1) + "% \n";
    }
    // ---- HELP PAGE ----
    else if (currentMenu == MenuPage.HelpPage)
    {
        if (AddMenuItem(ref text, sel, idx++, applyAction, " Back")) NavigateToMenu(MenuPage.Main);
        text += thinSep;
        text += "1. Dock to your docking station\n";
        text += "2. Select Record path & set home\n";
        text += "3. Fly the path to the ores\n";
        text += "4. Select stop path recording\n";
        text += "5. Align ship in mining direction\n";
        text += "6. Select Setup job and start\n";
    }

    if (setupErrorLevel == 2) text = "Fatal setup error\nNext scan: " + scanCountdown + "s\n";
    if (resetRequested) text = "Recompile script now";

    int lineCount = text.Split('\n').Length;
    for (int i = lineCount; i <= 10; i++) text += "\n";
    text += separator;
    text += "Last: " + statusMessage + "\n";
    return text;
}

// ========================== 12_ConfigHelpers.cs ==========================
// ========================== CONFIG HELPERS ==========================

void ChangeOverlap(bool isWidth, float step)
{
    StopAll();
    UpdateJobConfig(true, false);
    if (isWidth) AdjustConfig(ref widthOverlap, step, ConfigType.Overlap, false);
    else AdjustConfig(ref heightOverlap, step, ConfigType.Overlap, false);
    CalculateToolDimensions();
    UpdateSensor(true, true, 0, 0);
}

float CycleArrayValue(float current, float[] values)
{
    float result = values[0];
    for (int i = values.Length - 1; i >= 0; i--)
        if (current < values[i]) result = values[i];
    return result;
}

String NextTimerBlock(ref int index)
{
    String result = "";
    if (index >= timerBlocks.Count) index = -1;
    if (index >= 0) result = timerBlocks[index].CustomName;
    index++;
    return result;
}

void TriggerTimerBlock(string name)
{
    if (jobState != JobState.Active) return;
    if (name == "") return;
    IMyTerminalBlock block = gridTerminalSystem.GetBlockWithName(name);
    if (block == null || !(block is IMyTimerBlock))
    {
        statusMessage = "Timerblock " + name + " not found!";
        return;
    }
    ((IMyTimerBlock)block).Trigger();
}

void AdjustValue(ref int value, int bigStep, int threshold, int smallStep)
{
    if (bigStep == 0) return;
    if (value < threshold && smallStep > 0 || value <= threshold && smallStep < 0)
    {
        value += smallStep;
        return;
    }
    int absStep = Math.Abs(bigStep);
    int accumulated = 0;
    int multiplier = 1;
    while (true)
    {
        accumulated += multiplier * absStep * 10;
        if (bigStep < 0 && value - threshold <= accumulated) break;
        if (bigStep > 0 && value - threshold < accumulated) break;
        multiplier++;
    }
    value += multiplier * bigStep;
}

void ValidateJobConfig(bool updateSensor)
{
    configWidth = Math.Max(configWidth, 1);
    configHeight = Math.Max(configHeight, 1);
    configDepth = Math.Max(configDepth, 0);
    UpdateSensor(currentMenu == MenuPage.JobSetup, false, 0, 0);
}

bool ParseConfig(ref float value, bool round, ConfigType type, String input, String ignoreKeyword)
{
    if (input == "") return false;
    float parsed = -1;
    bool ignore = false;
    if (input.ToUpper() == ignoreKeyword) ignore = true;
    else if (!float.TryParse(input, out parsed)) return false;
    else parsed = Math.Max(0, parsed);
    if (round) parsed = (float)Math.Round(parsed);
    AdjustConfig(ref value, parsed, type, ignore, false);
    return true;
}

void AdjustConfig(ref float value, float step, ConfigType type, bool toggleIgnore)
{
    AdjustConfig(ref value, value + step, type, toggleIgnore, true);
}

void AdjustConfig(ref float value, float newValue, ConfigType type, bool toggleIgnore, bool wrap)
{
    float minVal = 0, maxVal = 0;
    if (type == ConfigType.WorkSpeed) { minVal = 0.5f; maxVal = 10f; }
    if (type == ConfigType.Acceleration) { minVal = 0.1f; maxVal = 1f; }
    if (type == ConfigType.Ignore) { minVal = 50f; maxVal = 100f; }
    if (type == ConfigType.MinBattery) { minVal = 5f; maxVal = 30f; }
    if (type == ConfigType.MinUranium) { minVal = 1f; maxVal = 25f; }
    if (type == ConfigType.MaxLoad) { minVal = 10f; maxVal = 95f; }
    if (type == ConfigType.MinHydrogen) { minVal = 10f; maxVal = 90f; }
    if (type == ConfigType.TimerDelay) { minVal = 10f; maxVal = 1800; }
    if (type == ConfigType.Overlap) { minVal = 0.0f; maxVal = 0.75f; }

    if (newValue == -1 && toggleIgnore) { value = -1; return; }
    if (value == -1) toggleIgnore = false;

    bool outOfRange = newValue < minVal || newValue > maxVal;
    if (outOfRange && wrap)
    {
        if (newValue < value) value = maxVal;
        else if (newValue > value) value = minVal;
    }
    else value = newValue;

    if (outOfRange && toggleIgnore) value = -1;
    else value = Math.Max(minVal, Math.Min(value, maxVal));
    value = (float)Math.Round(value, 2);
}

void AdvanceHole(bool previous)
{
    if (previous) currentHoleIndex = Math.Max(0, currentHoleIndex - 1);
    else currentHoleIndex++;
    UpdateProgress(true);
}

T CycleEnum<T>(T current)
{
    int idx = Array.IndexOf(Enum.GetValues(current.GetType()), current);
    idx++;
    if (idx >= EnumCount(current)) idx = 0;
    return (T)Enum.GetValues(current.GetType()).GetValue(idx);
}

int EnumCount<T>(T value)
{
    return Enum.GetValues(value.GetType()).Length;
}

// ========================== 13_PathRecording.cs ==========================
// ========================== PATH RECORDING ==========================

void StartPathRecording()
{
    waypointsBackup.Clear();
    for (int i = 0; i < waypoints.Count; i++) waypointsBackup.Add(waypoints[i]);
    waypoints.Clear();
    isRecording = true;
    homeDockBackup = new WaypointInfo(homeDock);
    jobDockBackup = new WaypointInfo(jobPosition);
    homeDock.isSet = false;
    if (shipMode == ShipMode.Shuttle) jobPosition.isSet = false;

    for (int i = 0; i < thrustByType.Count; i++)
        if (!thrusterTypeNames.Contains(thrustByType.Keys.ElementAt(i)))
            thrusterTypeNames.Add(thrustByType.Keys.ElementAt(i));

    useOldHome = false;
    useOldConnector2 = false;
    useOldPath = false;
    NavigateToMenu(MenuPage.Recording);
}

void StopPathRecording()
{
    if (useOldHome) homeDock = homeDockBackup;
    if (useOldConnector2) jobPosition = jobDockBackup;
    if (useOldPath)
    {
        waypoints.Clear();
        for (int i = 0; i < waypointsBackup.Count; i++) waypoints.Add(waypointsBackup[i]);
    }
    isRecording = false;
    StopAll();
    NavigateToMenu(MenuPage.Main);
}

void UpdatePathRecording()
{
    if (!isRecording) return;
    if (currentNavState != NavState.Idle) { StopPathRecording(); return; }

    if (!homeDockBackup.isSet) useOldHome = false;
    if (!jobDockBackup.isSet) useOldConnector2 = false;
    if (waypointsBackup.Count <= 1) useOldPath = false;

    IMyShipConnector conn = FindConnector(MyShipConnectorStatus.Connectable);
    if (conn == null) conn = FindConnector(MyShipConnectorStatus.Connected);

    if (conn != null)
    {
        if (Math.Round(shipSpeed, 2) <= 0.20) dockDetectCounter++;
        else dockDetectCounter = 0;

        if (dockDetectCounter >= 5)
        {
            if (shipMode == ShipMode.Shuttle && (homeDock.isSet || useOldHome) && Vector3.Distance(homeDock.position, conn.GetPosition()) > 5)
            {
                jobPosition.forwardDir = remoteControl.WorldMatrix.Forward;
                jobPosition.leftDir = remoteControl.WorldMatrix.Left;
                jobPosition.downDir = remoteControl.WorldMatrix.Down;
                jobPosition.gravity = remoteControl.GetNaturalGravity();
                jobPosition.position = conn.GetPosition();
                jobPosition.isSet = true;
                jobPosition.connectorGridPos = conn.Position;
            }
            else
            {
                homeDock.forwardDir = remoteControl.WorldMatrix.Forward;
                homeDock.leftDir = remoteControl.WorldMatrix.Left;
                homeDock.downDir = remoteControl.WorldMatrix.Down;
                homeDock.gravity = remoteControl.GetNaturalGravity();
                homeDock.position = conn.GetPosition();
                homeDock.isSet = true;
                homeDock.connectorGridPos = conn.Position;
            }
        }
    }

    double lastDist = -1;
    if (waypoints.Count > 0) lastDist = Vector3.Distance(shipPosition, waypoints.Last().position);

    double speedFactor = Math.Max(1.5, Math.Pow(shipSpeed / 100.0, 2));
    double minSpacing = Math.Max(shipSpeed * speedFactor, 2);
    waypointSpacing = minSpacing;

    if ((lastDist == -1) || lastDist >= minSpacing)
    {
        WaypointInfo wp = new WaypointInfo(shipPosition, forwardDirection, downDirection, leftDirection, remoteControl.GetNaturalGravity());
        wp.CalcThrusterEfficiency(thrusters, thrusterTypeNames);
        waypoints.Add(wp);
    }
}


// ========================== DOCK HELPERS ==========================

WaypointInfo GetCurrentDock()
{
    if (shipMode != ShipMode.Shuttle) return homeDock;
    return currentDockTarget;
}

void SetDockTarget(WaypointInfo dock, JobState state)
{
    currentDockTarget = dock;
    if (jobState == JobState.Active) lastJobState = state;
}

bool GetDockApproachPosition(WaypointInfo dock, float dist, bool useConnector, out Vector3 pos)
{
    if (useConnector)
    {
        Vector3I gridPos = new Vector3I((int)dock.connectorGridPos.X, (int)dock.connectorGridPos.Y, (int)dock.connectorGridPos.Z);
        IMySlimBlock block = Me.CubeGrid.GetCubeBlock(gridPos);
        if (block == null || !(block.FatBlock is IMyShipConnector))
        {
            pos = new Vector3();
            return false;
        }
        Vector3 connOffset = LocalTransformDirection(remoteControl, block.FatBlock.GetPosition() - shipPosition);
        Vector3 connForward = LocalTransformDirection(remoteControl, block.FatBlock.WorldMatrix.Forward);
        pos = dock.position - TransformLocalToWorld(dock.forwardDir, dock.downDir * -1, connOffset) - TransformLocalToWorld(dock.forwardDir, dock.downDir * -1, connForward) * dist;
        return true;
    }
    else
    {
        pos = dock.position;
        return true;
    }
}


// ========================== JOB POSITION CALCULATION ==========================

Vector3 GetJobGridPosition(int col, int row, bool recalc)
{
    if (!recalc && gridPosCached) return cachedGridPos;
    float colOffset = ((jobWidth - 1f) / 2f) - col;
    float rowOffset = ((jobHeight - 1f) / 2f) - row;
    cachedGridPos = jobPosition.position + jobPosition.leftDir * colOffset * toolWidth + miningDown * -1 * rowOffset * toolHeight;
    gridPosCached = true;
    return cachedGridPos;
}

Vector3 GetMiningDepthPosition(Vector3 surfacePos, float depth)
{
    return surfacePos + (miningForward * depth);
}

// ========================== 14_JobControl.cs ==========================
// ========================== LOCATION DETERMINATION ==========================

LocationType DetermineLocation()
{
    float closest = -1;
    LocationType result = LocationType.AtJob;

    if (shipMode != ShipMode.Shuttle)
    {
        if (jobState != JobState.NoJob)
        {
            Vector3 localPos = TransformToLocal(miningForward, miningDown * -1, shipPosition - jobPosition.position);
            if (Math.Abs(localPos.X) <= (float)(jobWidth * toolWidth) / 2f && Math.Abs(localPos.Y) <= (float)(jobHeight * toolHeight) / 2f)
            {
                if (localPos.Z <= -1 && localPos.Z >= -runtimeMaxDepth * 2) return LocationType.AtMine;
                if (localPos.Z > -1 && localPos.Z < shipDiameter * 2) return LocationType.AtJob;
            }
            if (CheckCloser(jobPosition.position, ref closest)) result = LocationType.AtJob;
        }
        if (homeDock.isSet)
        {
            if (CheckCloser(homeDock.position, ref closest)) result = LocationType.AtHomeDock;
            for (int i = 0; i < waypoints.Count; i++)
                if (CheckCloser(waypoints[i].position, ref closest)) result = LocationType.OnPath;
            if (Vector3.Distance(shipPosition, homeDock.position) < dockDist * shipDiameter) result = LocationType.AtHomeDock;
            if (FindConnector(MyShipConnectorStatus.Connectable) != null || FindConnector(MyShipConnectorStatus.Connected) != null)
                result = LocationType.AtHomeDock;
        }
    }
    else
    {
        Vector3 pos = new Vector3();
        IMyShipConnector connected = FindConnector(MyShipConnectorStatus.Connected);
        if (homeDock.isSet)
        {
            if (CheckCloser(homeDock.position, ref closest)) result = LocationType.AtHomeDock;
            if (GetDockApproachPosition(homeDock, dockDist, true, out pos))
                if (CheckCloser(pos, ref closest)) result = LocationType.AtHomeDock;
            if (connected != null && Vector3.Distance(connected.GetPosition(), homeDock.position) < 5)
                return LocationType.AtDock1;
        }
        for (int i = 0; i < waypoints.Count; i++)
            if (Vector3.Distance(waypoints[i].position, homeDock.position) > dockDist * shipDiameter && Vector3.Distance(waypoints[i].position, jobPosition.position) > dockDist * shipDiameter)
                if (CheckCloser(waypoints[i].position, ref closest)) result = LocationType.OnPath;
        if (jobPosition.isSet)
        {
            if (CheckCloser(jobPosition.position, ref closest)) result = LocationType.AtJob;
            if (GetDockApproachPosition(jobPosition, dockDist, true, out pos))
                if (CheckCloser(pos, ref closest)) result = LocationType.AtJob;
            if (connected != null && Vector3.Distance(connected.GetPosition(), jobPosition.position) < 5)
                return LocationType.AtDock2;
        }
    }
    return result;
}

bool CheckCloser(Vector3 pos, ref float closest)
{
    float dist = Vector3.Distance(pos, shipPosition);
    if (dist < closest || closest == -1) { closest = dist; return true; }
    return false;
}


// ========================== JOB MANAGEMENT ==========================

void StartNewJob()
{
    if (setupErrorLevel > 0) { statusMessage = "Setup error! Can't start"; return; }
    if (shipMode == ShipMode.Shuttle) { ContinueJob(); return; }

    jobPosition.position = shipPosition;
    jobPosition.gravity = remoteControl.GetNaturalGravity();
    jobPosition.forwardDir = forwardDirection;
    jobPosition.downDir = downDirection;
    jobPosition.leftDir = leftDirection;
    miningForward = referenceWorkTool.WorldMatrix.Forward;
    miningDown = jobPosition.downDir;
    if (miningForward == remoteControl.WorldMatrix.Down) miningDown = remoteControl.WorldMatrix.Backward;
    UpdateJobConfig(true, true);
    SetNavState(NavState.FlyToXY);
    StartJobCommon();
}

void UpdateJobConfig(bool force, bool resetProgress)
{
    if (jobState == JobState.NoJob && !force) return;
    bool needsReset = force || jobState == JobState.Done || jobWidth != configWidth || jobHeight != configHeight || jobStartPos != startPosition;
    if (needsReset)
    {
        if (jobState != JobState.NoJob)
        {
            jobState = JobState.Changed;
            GetJobGridPosition(holeCol, holeRow, resetProgress);
            statusMessage = "Job changed, lost progress";
        }
        jobStartPos = startPosition;
        jobWidth = configWidth;
        jobHeight = configHeight;
        holeRow = 0; holeCol = 0;
        maxMineDepth = 0; lastHoleIndex = 0;
        currentMineDepth = 0; currentHoleIndex = 0;
        UpdateProgress(true);
    }
}

void PrepareForFlight()
{
    SetFlightTarget(shipPosition, 0);
    SetBlocksEnabled(thrusters, true);
}

void StartJobCommon()
{
    ShowStatusMessage(StatusMsg.Running);
    PrepareForFlight();
    SetLandingGears(landingGears, false);
    SetSortersEnabled(sorterState);
    jobState = JobState.Active;
    CalculateMaxWeight(true);
    lastJobState = jobState;
    NavigateToMenu(MenuPage.JobRunning);
    ScanAllBlocks();
    damageCheckActive = true;
    initialDamageCount = 0;
    for (int i = allGridBlocks.Count - 1; i >= 0; i--)
        if (IsBlockDamaged(allGridBlocks[i], false)) initialDamageCount++;
    if (initialDamageCount > 0) statusMessage = "Started with damage";
}

void StopAll()
{
    if (jobState == JobState.Active) { jobState = JobState.Paused; statusMessage = "Job paused"; }
    SetNavState(NavState.Idle);
    lastJobState = jobState;
    SetGyroOverride(false, 0, 0, 0, 0);
    StopFlight();
    ApplyThrustOverride(new Vector3(), false);
    StopGyroOverride();
    SetBatteryChargeMode(ChargeMode.Auto);
    SetHydrogenStockpile(false);
    SetDampeners(true);
    SetStopAtState(NavState.Idle);
    UpdateSensor(false, false, 0, 0);
    SetBlocksEnabled(workTools, false);
    SetBlocksEnabled(connectors, true);
    SetSortersEnabled(true);
    returnToJobAfter = false;
    damageCheckActive = false;
    simulateShipFull = false;
    undockRequested = false;
    if (currentMenu != MenuPage.Main && currentMenu != MenuPage.BehaviorSettings && currentMenu != MenuPage.AdvancedSettings && currentMenu != MenuPage.ShuttleBehavior)
        NavigateToMenu(MenuPage.Main);
}

void ContinueJob()
{
    LocationType loc = DetermineLocation();

    if (shipMode == ShipMode.Shuttle)
    {
        if (!jobPosition.isSet || !homeDock.isSet) return;
        StartJobCommon();
        bool goToHome = Vector3.Distance(shipPosition, homeDock.position) < Vector3.Distance(shipPosition, jobPosition.position);
        if (savedJobState == JobState.ActiveHome) goToHome = true;
        if (savedJobState == JobState.ActiveJob) goToHome = false;

        if (goToHome)
        {
            SetDockTarget(homeDock, JobState.ActiveHome);
            switch (loc)
            {
                case LocationType.AtDock1: SetNavState(NavState.WaitForCommand); break;
                case LocationType.OnPath: SetNavState(NavState.FlyToJobArea); break;
                case LocationType.AtHomeDock: SetNavState(NavState.ApproachDock); break;
                default: SetNavState(NavState.Undocking); break;
            }
        }
        else
        {
            SetDockTarget(jobPosition, JobState.ActiveJob);
            switch (loc)
            {
                case LocationType.AtDock2: SetNavState(NavState.WaitForCommand); break;
                case LocationType.AtJob: SetNavState(NavState.ApproachDock); break;
                case LocationType.OnPath: SetNavState(NavState.FlyToJobArea); break;
                default: SetNavState(NavState.Undocking); break;
            }
        }
    }
    else
    {
        if (jobState != JobState.Paused && jobState != JobState.Changed) return;
        bool wasChanged = jobState == JobState.Changed;
        StartJobCommon();
        bool hasDock = IsShipFull(false) && homeDock.isSet;

        switch (loc)
        {
            case LocationType.AtJob: SetNavState(hasDock ? NavState.FlyToPath : NavState.FlyToXY); break;
            case LocationType.OnPath: SetNavState(hasDock ? NavState.FlyToDockArea : NavState.FlyToJobArea); break;
            case LocationType.AtHomeDock: SetNavState(hasDock ? NavState.ApproachDock : NavState.Unloading); break;
            case LocationType.AtMine:
            {
                if (lastHoleIndex != currentHoleIndex || wasChanged) SetNavState(NavState.Returning);
                else SetNavState(NavState.Mining);
            }
            break;
            default: break;
        }
    }
}

void FlyToJobPosition()
{
    if (jobState == JobState.NoJob && !homeDock.isSet) return;
    if (shipMode == ShipMode.Shuttle && (!jobPosition.isSet || !homeDock.isSet)) return;
    statusMessage = "Move to job";

    LocationType loc = DetermineLocation();
    if (shipMode == ShipMode.Shuttle)
    {
        SetDockTarget(jobPosition, JobState.ActiveJob);
        switch (loc)
        {
            case LocationType.AtJob: SetNavState(NavState.ApproachDock); break;
            case LocationType.OnPath: SetNavState(NavState.FlyToJobArea); break;
            case LocationType.AtDock2: return;
            default: SetNavState(NavState.Undocking); break;
        }
        SetStopAtState(NavState.WaitForCommand);
    }
    else
    {
        switch (loc)
        {
            case LocationType.AtJob: SetNavState(NavState.FlyToJobArea); break;
            case LocationType.OnPath: SetNavState(NavState.FlyToJobArea); break;
            case LocationType.AtHomeDock: SetNavState(NavState.Unloading); break;
            case LocationType.AtMine: SetNavState(NavState.Returning); break;
            default: break;
        }
        if (jobState == JobState.NoJob) SetStopAtState(NavState.FlyToJobArea);
        else SetStopAtState(NavState.AlignJob);
        returnToJobAfter = true;
    }
    PrepareForFlight();
    NavigateToMenu(MenuPage.StatusOverview);
    SetLandingGears(landingGears, false);
    lastJobState = JobState.MoveToJob;
}

void FlyToHomePosition()
{
    if (!homeDock.isSet) return;
    statusMessage = "Move home";

    LocationType loc = DetermineLocation();
    if (shipMode == ShipMode.Shuttle)
    {
        SetDockTarget(homeDock, JobState.ActiveHome);
        switch (loc)
        {
            case LocationType.OnPath: SetNavState(NavState.FlyToDockArea); break;
            case LocationType.AtHomeDock: SetNavState(NavState.ApproachDock); break;
            case LocationType.AtDock1: return;
            default: SetNavState(NavState.Undocking); break;
        }
        SetStopAtState(NavState.WaitForCommand);
    }
    else
    {
        if (FindConnector(MyShipConnectorStatus.Connected) != null) return;
        if (FindConnector(MyShipConnectorStatus.Connectable) != null)
        {
            SetNavState(NavState.Docking);
            SetStopAtState(NavState.Unloading);
            return;
        }
        switch (loc)
        {
            case LocationType.AtJob: SetNavState(NavState.FlyToPath); break;
            case LocationType.OnPath: SetNavState(NavState.FlyToDockArea); break;
            case LocationType.AtHomeDock: SetNavState(NavState.FlyToDockArea); break;
            case LocationType.AtMine: SetNavState(NavState.ReturnToDock); break;
            default: break;
        }
        SetStopAtState(NavState.Unloading);
    }
    PrepareForFlight();
    NavigateToMenu(MenuPage.StatusOverview);
    SetLandingGears(landingGears, false);
    lastJobState = JobState.MoveHome;
}

void SetNavState(NavState newState)
{
    if (newState == NavState.Idle) stopAtState = NavState.Idle;
    if (stopAtState != NavState.Idle && currentNavState == stopAtState && newState != stopAtState) { StopAll(); return; }
    stateFirstTick = true;
    currentNavState = newState;
}

void SetStopAtState(NavState state)
{
    this.stopAtState = state;
}

// ========================== 15_Navigation.cs ==========================
// ========================== HOLE SPIRAL PATTERN ==========================

SpiralResult GetNextHolePosition(int holeNumber, bool reset)
{
    if (reset) { spiralState = null; holeCol = 0; holeRow = 0; }

    if (startPosition == StartPosition.TopLeft)
    {
        int next = holeNumber + 1;
        holeRow = (int)Math.Floor(SafeDiv(holeNumber, jobWidth));
        if (holeRow % 2 == 0) holeCol = holeNumber - (holeRow * jobWidth);
        else holeCol = jobWidth - 1 - (holeNumber - (holeRow * jobWidth));
        if (holeRow >= jobHeight) return SpiralResult.AllDone;
        else return SpiralResult.Found;
    }
    else if (startPosition == StartPosition.Center)
    {
        if (spiralState == null) spiralState = new int[] { 0, -1, 0, 0 };
        int halfW = (int)Math.Ceiling(jobWidth / 2f);
        int halfH = (int)Math.Ceiling(jobHeight / 2f);
        int halfW2 = (int)Math.Floor(jobWidth / 2f);
        int halfH2 = (int)Math.Floor(jobHeight / 2f);
        int iterations = 0;
        while (spiralState[2] < Math.Pow(Math.Max(jobWidth, jobHeight), 2))
        {
            if (iterations > 200) return SpiralResult.Error;
            iterations++;
            spiralState[2]++;
            if (-halfW < holeCol && holeCol <= halfW2 && -halfH < holeRow && holeRow <= halfH2)
            {
                if (spiralState[3] == holeNumber)
                {
                    this.holeCol = holeCol - 1 + halfW;
                    this.holeRow = holeRow - 1 + halfH;
                    return SpiralResult.Found;
                }
                spiralState[3]++;
            }
            if (holeCol == holeRow || (holeCol < 0 && holeCol == -holeRow) || (holeCol > 0 && holeCol == 1 - holeRow))
            {
                int temp = spiralState[0];
                spiralState[0] = -spiralState[1];
                spiralState[1] = temp;
            }
            holeCol += spiralState[0];
            holeRow += spiralState[1];
        }
    }
    return SpiralResult.AllDone;
}


// ========================== NAVIGATION STATE MACHINE ==========================
// (UpdateNavigation is the core state machine - extremely large)
// See the original logic flow for all NavState transitions.
// The full method has been preserved with readable variable names.

void UpdateNavigation()
{
    // Due to extreme length (500+ lines), this method preserves the original
    // state machine logic. Each NavState case handles transitions.
    // The full implementation follows the same flow as the obfuscated version
    // with meaningful variable names applied throughout.

    // [FlyToXY] -> determine next hole, fly to surface position
    if (currentNavState == NavState.FlyToXY)
    {
        if (stateFirstTick)
        {
            stuckCounter = 0;
            if (lastHoleIndex != currentHoleIndex) maxMineDepth = 0;
            lastHoleIndex = currentHoleIndex;
        }
        if (stuckCounter == 0)
        {
            SpiralResult result = GetNextHolePosition(currentHoleIndex, stateFirstTick);
            if (result == SpiralResult.AllDone)
            {
                jobState = JobState.Done;
                statusMessage = "Job done";
                if (whenDoneReturnHome && homeDock.isSet) { SetNavState(NavState.FlyToPath); SetStopAtState(NavState.Unloading); lastJobState = JobState.MoveHome; }
                else { SetNavState(NavState.FlyToJobPos); SetStopAtState(NavState.AlignJob); lastJobState = JobState.MoveToJob; }
                return;
            }
            if (result == SpiralResult.Found)
            {
                stuckCounter = 1;
                SetBlocksEnabled(workTools, true);
                miningTargetPos = GetJobGridPosition(holeCol, holeRow, true);
                SetFlightTarget(miningTargetPos, 10);
                AlignToDown(jobPosition.downDir, jobPosition.forwardDir, jobPosition.leftDir, false);
            }
        }
        else
        {
            if (distToTarget < wpReachedDist) { SetNavState(NavState.Mining); return; }
        }
    }

    // [Mining] -> drill downward
    if (currentNavState == NavState.Mining)
    {
        if (stateFirstTick)
        {
            SetBlocksEnabled(workTools, true);
            SetSortersEnabled(false);
            miningTargetPos = GetJobGridPosition(holeCol, holeRow, false);
            SetFlightTarget(GetMiningDepthPosition(miningTargetPos, 0), 0);
            AlignToDown(jobPosition.downDir, jobPosition.forwardDir, jobPosition.leftDir, false);
            stuckCounter = 1;
            lastDistToTarget = 0;
            stuckSensitivity = 0;
            autoDepthRef = 0;
            lastOreAmount = -1;
            runtimeMaxDepth = configDepth;
            isFirstDescent = true;
        }

        if (!CheckEnergy()) { SetNavState(NavState.ReturnToDock); return; }
        if (IsShipFull(true))
        {
            oreCountBeforeEject = CountItems("", "ORE", ItemLocation.All);
            if ((ejectionMode == EjectionMode.DropPosStone || ejectionMode == EjectionMode.DropPosStoIce ||
                 ejectionMode == EjectionMode.InMotionStone || ejectionMode == EjectionMode.InMotionStoIce) && shipMode != ShipMode.Grinder)
                SetNavState(NavState.FlyToDropPos);
            else if ((ejectionMode == EjectionMode.CurPosStone || ejectionMode == EjectionMode.CurPosStoIce) && shipMode != ShipMode.Grinder)
                SetNavState(NavState.WaitEjection);
            else
                SetNavState(NavState.ReturnToDock);
            return;
        }

        currentMineDepth = Vector3.Distance(shipPosition, miningTargetPos);
        if (currentMineDepth > maxMineDepth) { maxMineDepth = currentMineDepth; isFirstDescent = false; }

        if (shipMode == ShipMode.Grinder && GetDetectedEntityType() == MyDetectedEntityType.SmallGrid)
            stuckSensitivity += 2;
        else stuckSensitivity -= 2;
        stuckSensitivity = Math.Max(100, Math.Min(400, stuckSensitivity));

        if (stuckCounter > 0 && stuckCounter < stuckSensitivity)
        {
            if (currentMineDepth > lastDistToTarget) { if (stuckSensitivity > 150) lastDistToTarget = currentMineDepth; else lastDistToTarget = (float)Math.Ceiling(currentMineDepth); stuckCounter = 1; }
            else stuckCounter++;
        }
        else
        {
            if (stuckCounter > 0)
            {
                statusMessage = "Ship stuck! Retrying";
                lastDistToTarget = currentMineDepth;
                stuckCounter = 0;
                UpdateSensor(false, true, 0, shipDiameter * sensorRange);
            }
            SetFlightTarget(GetMiningDepthPosition(miningTargetPos, Math.Max(0, lastDistToTarget - shipDiameter)), GetWorkSpeed(false));
            if (distToTarget <= wpReachedDist / 2) { stuckCounter = 1; lastDistToTarget = 0; }
            return;
        }

        UpdateSensor(false, true, shipDiameter * sensorRange, 0);
        Vector3 alignedPos = miningTargetPos + miningForward * currentMineDepth;
        bool isOffCenter = false;
        if (Vector3.Distance(alignedPos, shipPosition) > 0.3f)
        {
            Vector3 corrected = miningTargetPos + miningForward * (currentMineDepth + 0.1f);
            SetFlightTarget(corrected, 4);
            isOffCenter = true;
        }
        else
        {
            float speed = GetWorkSpeed(true);
            Vector3 deepTarget = GetMiningDepthPosition(miningTargetPos, Math.Max(configDepth + 1, currentMineDepth + 1));
            SetFlightTargetFull(true, false, false, deepTarget, deepTarget - miningTargetPos, speed, speed);
        }

        bool depthReached = false;
        if (depthMode == DepthMode.AutoStone || depthMode == DepthMode.AutoOre)
        {
            if (!isOffCenter)
            {
                float oreCount = 0;
                foreach (IMyTerminalBlock tool in workTools)
                    oreCount += CountBlockItems(tool, "", "", depthMode == DepthMode.AutoOre ? new string[] { "STONE" } : null);
                if (oreCount > lastOreAmount || currentMineDepth < configDepth || isFirstDescent) { autoDepthCounter = 0; autoDepthRef = currentMineDepth; runtimeMaxDepth = (int)(Math.Max(runtimeMaxDepth, autoDepthRef) + shipDiameter / 2); }
                else { depthReached = currentMineDepth - autoDepthRef > 2 && autoDepthCounter >= 20; autoDepthCounter++; }
                lastOreAmount = oreCount;
            }
        }
        else depthReached = currentMineDepth >= runtimeMaxDepth;

        if (lastHoleIndex != currentHoleIndex) { SetNavState(NavState.Returning); currentMineDepth = 0; return; }
        if (depthReached) { currentHoleIndex++; SetNavState(NavState.Returning); currentMineDepth = 0; return; }
    }

    // [Returning / ReturnToDock / FlyToDropPos] -> fly back to surface
    if (currentNavState == NavState.Returning || currentNavState == NavState.ReturnToDock || currentNavState == NavState.FlyToDropPos)
    {
        if (stateFirstTick)
        {
            miningTargetPos = GetJobGridPosition(holeCol, holeRow, false);
            AlignToDown(jobPosition.downDir, jobPosition.forwardDir, jobPosition.leftDir, false);
            SetBlocksEnabled(workTools, enableDrillsBothWays);
            SetSortersEnabled(false);
            lastDistToTarget = Vector3.Distance(shipPosition, miningTargetPos);
            UpdateSensor(false, true, 0, shipDiameter * sensorRange);
        }
        SetFlightTarget(miningTargetPos, GetWorkSpeed(false));

        if (Vector3.Distance(shipPosition, miningTargetPos) >= lastDistToTarget + 5) { SetBlocksEnabled(workTools, false); SetSortersEnabled(true); statusMessage = "Can´t return!"; }

        if (distToTarget < wpReachedDist)
        {
            if (currentNavState == NavState.Returning && returnToJobAfter) SetNavState(NavState.FlyToJobPos);
            if (currentNavState == NavState.Returning) SetNavState(NavState.FlyToXY);
            if (currentNavState == NavState.FlyToDropPos) SetNavState(NavState.FlyToDropPos2);
            if (currentNavState == NavState.ReturnToDock)
            {
                if (homeDock.isSet) SetNavState(NavState.FlyToPath);
                else { StopAll(); FlyToJobPosition(); statusMessage = "Can´t return, no dock found"; }
            }
            return;
        }
    }

    // [FlyToPath -> FlyToDockArea -> FlyToJobArea] follow recorded path
    if (currentNavState == NavState.FlyToDockArea || currentNavState == NavState.FlyToJobArea)
    {
        if (currentNavState == NavState.FlyToJobArea && jobState == JobState.Active && shipMode != ShipMode.Shuttle)
        {
            if (!CheckEnergy() || IsShipFull(true)) { SetNavState(NavState.FlyToDockArea); return; }
        }

        bool reachedEnd = false;
        bool useDockDir = false;
        bool atWaypoint = false;
        float approachSpeed = 0;
        bool offPath = false;
        WaypointInfo wp = null;

        if (stateFirstTick)
        {
            if (currentNavState == NavState.FlyToDockArea || shipMode == ShipMode.Shuttle)
            {
                WaypointInfo dock = GetCurrentDock();
                pathFollowData = new PathFollowData();
                pathFollowData.target = dock;
                pathFollowData.directFlyDist = followPathDock * shipDiameter;
                pathFollowData.useDockDirDist = useDockDirectionDist * shipDiameter;
                pathFollowData.approachSpeed = 10;
                pathFollowData.targetPositions.Add(dock.position);
                Vector3 approachPos = new Vector3();
                if (GetDockApproachPosition(dock, dockDist * shipDiameter, true, out approachPos))
                    pathFollowData.targetPositions.Add(approachPos);
                else pathFollowData.directFlyDist *= 1.5f;
                if (shipMode == ShipMode.Shuttle)
                {
                    if (dock == homeDock) pathFollowData.excludePos = jobPosition.position;
                    if (dock == jobPosition) pathFollowData.excludePos = homeDock.position;
                    pathFollowData.excludeDist = dockDist * shipDiameter * 1.1f;
                }
            }
            else if (currentNavState == NavState.FlyToJobArea)
            {
                pathFollowData = new PathFollowData();
                pathFollowData.target = jobPosition;
                pathFollowData.directFlyDist = followPathJob * shipDiameter;
                pathFollowData.useDockDirDist = useJobDirectionDist * shipDiameter;
                pathFollowData.approachSpeed = 10;
                pathFollowData.excludePos = homeDock.position;
                pathFollowData.excludeDist = dockDist * shipDiameter * 1.1f;
                pathFollowData.targetPositions.Add(jobPosition.position);
                if (jobState == JobState.NoJob)
                {
                    if (!homeDock.isSet || waypoints.Count == 0) { StopAll(); return; }
                    float distFirst = Vector3.Distance(waypoints.First().position, homeDock.position);
                    float distLast = Vector3.Distance(waypoints.Last().position, homeDock.position);
                    if (distFirst < distLast) pathFollowData.target = waypoints.Last();
                    else pathFollowData.target = waypoints.First();
                }
            }

            pathSegmentDir = new Vector3();
            offPath = !IsOnPath(shipPosition);
            SetBlocksEnabled(workTools, false);
            SetSortersEnabled(true);
            pathWaypointIndex = -1;
            double closestDist = -1;
            for (int i = waypoints.Count - 1; i >= 0; i--)
            {
                if (Vector3.Distance(waypoints[i].position, pathFollowData.excludePos) <= pathFollowData.excludeDist) continue;
                double dist = Vector3.Distance(waypoints[i].position, shipPosition);
                if (closestDist == -1 || dist < closestDist) { pathWaypointIndex = i; closestDist = dist; }
            }
            pathDirection = GetPathDirection(pathFollowData.target.position, pathWaypointIndex);
            currentPathWaypoint = null;
        }

        CalculatePathSpeeds(waypoints, pathDirection, pathFollowData.targetPositions, pathFollowData.directFlyDist, stateFirstTick, ref stuckCounter);

        for (int i = 0; i < pathFollowData.targetPositions.Count; i++)
        {
            float dist = Vector3.Distance(shipPosition, pathFollowData.targetPositions[i]);
            if (dist <= pathFollowData.directFlyDist) reachedEnd = true;
            if (dist <= pathFollowData.useDockDirDist) useDockDir = true;
        }
        if (useDockDir) approachSpeed = pathFollowData.approachSpeed;

        float wpSpeed = currentPathWaypoint != null ? currentPathWaypoint.maxSpeed : shipSpeed;
        float wpReachDist = (float)Math.Max(shipSpeed * 0.1f * shipDiameter, wpReachedDist);

        if ((distToTarget < wpReachDist) || stateFirstTick)
        {
            if (!stateFirstTick) pathWaypointIndex += pathDirection;
            if (pathDirection == 0 || pathWaypointIndex > waypoints.Count - 1 || pathWaypointIndex < 0)
                reachedEnd = true;
            else
            {
                waypointsRemaining = pathDirection > 0 ? waypoints.Count - 1 - pathWaypointIndex : pathWaypointIndex;
                wp = waypoints[pathWaypointIndex];
                currentPathWaypoint = wp;
                if (pathWaypointIndex >= 1 && pathWaypointIndex < waypoints.Count - 1)
                    pathSegmentDir = wp.position - waypoints[pathWaypointIndex - pathDirection].position;
                else currentPathWaypoint = null;
                navTargetPos = wp.position;
                atWaypoint = true;
            }
        }

        if (useDockDir)
            AlignToDown(pathFollowData.target.downDir, pathFollowData.target.forwardDir, pathFollowData.target.leftDir, false);
        else if (offPath)
            AlignToGravity(pathFollowData.target.downDir, 10, true);
        else if (atWaypoint && wp != null)
        {
            if (pathDirection > 0) AlignToDown(wp.downDir, wp.forwardDir, wp.leftDir, 90, false);
            else AlignToDown(wp.downDir, -wp.forwardDir, -wp.leftDir, 90, false);
        }

        SetFlightTargetFull(true, false, true, navTargetPos, pathSegmentDir, currentPathWaypoint == null ? 0 : currentPathWaypoint.maxSpeed, approachSpeed);

        if (reachedEnd)
        {
            waypointsRemaining = 0;
            if (currentNavState == NavState.FlyToDockArea || shipMode == ShipMode.Shuttle) { SetNavState(NavState.ApproachDock); return; }
            if (currentNavState == NavState.FlyToJobArea && returnToJobAfter) { SetNavState(NavState.FlyToJobPos); return; }
            if (currentNavState == NavState.FlyToJobArea) { SetNavState(NavState.FlyToXY); return; }
        }
    }

    // [ApproachDock / RetryDocking] -> approach connector
    if (currentNavState == NavState.ApproachDock || currentNavState == NavState.RetryDocking)
    {
        WaypointInfo dock = GetCurrentDock();
        if (stateFirstTick)
        {
            if (!GetDockApproachPosition(dock, dockDist * shipDiameter, true, out navTargetPos))
            { ShowStatusMessage(StatusMsg.ConnectorNotReady); StopAll(); return; }
            SetFlightTarget(navTargetPos, 0);
            AlignToGravity(dock.downDir, 90, true);
        }
        if (distToTarget < followPathDock * shipDiameter && distToTarget != -1)
        {
            SetFlightTarget(navTargetPos, 10);
            AlignToDown(dock.downDir, dock.forwardDir, dock.leftDir, false);
        }
        if (FindConnector(MyShipConnectorStatus.Connectable) != null || FindConnector(MyShipConnectorStatus.Connected) != null)
        { SetNavState(NavState.Docking); return; }
        if (distToTarget < wpReachedDist / 2 && distToTarget != -1)
        { SetNavState(NavState.AlignDock); return; }
    }

    // [AlignDock / AlignJob] -> precise alignment
    if (currentNavState == NavState.AlignDock || currentNavState == NavState.AlignJob)
    {
        if (stateFirstTick)
        {
            if (currentNavState == NavState.AlignDock)
            {
                WaypointInfo dock = GetCurrentDock();
                if (!GetDockApproachPosition(dock, dockDist * shipDiameter, true, out navTargetPos))
                { ShowStatusMessage(StatusMsg.ConnectorNotReady); StopAll(); return; }
                SetFlightTargetFull(true, true, false, navTargetPos, 0);
                AlignToDown(dock.downDir, dock.forwardDir, dock.leftDir, 10, false);
            }
            if (currentNavState == NavState.AlignJob)
            {
                AlignToDown(jobPosition.downDir, jobPosition.forwardDir, jobPosition.leftDir, 0.5f, false);
                navTargetPos = jobPosition.position;
                SetFlightTargetFull(true, true, false, navTargetPos, 0);
            }
        }
        if (isAligned)
        {
            SetGyroOverride(false, 0, 0, 0, 0);
            if (currentNavState == NavState.AlignDock) SetNavState(NavState.Docking);
            if (currentNavState == NavState.AlignJob) StopAll();
            return;
        }
    }

    // [Docking] -> final approach and connect
    if (currentNavState == NavState.Docking)
    {
        if (FindConnector(MyShipConnectorStatus.Connected) != null)
        {
            if (shipMode == ShipMode.Shuttle) SetNavState(NavState.WaitForCommand);
            else SetNavState(NavState.Unloading);
            return;
        }

        WaypointInfo dock = GetCurrentDock();
        if (stateFirstTick)
        {
            stuckSensitivity = 0;
            dockStartTime = DateTime.Now;
            stuckCounter = 0;
            AlignToDown(dock.downDir, dock.forwardDir, dock.leftDir, false);
        }

        Vector3I gridPos = new Vector3I((int)dock.connectorGridPos.X, (int)dock.connectorGridPos.Y, (int)dock.connectorGridPos.Z);
        IMySlimBlock connBlock = Me.CubeGrid.GetCubeBlock(gridPos);
        float dockSpeed2 = dockingSpeed;
        float maxDockSpeed = dockingSpeed * 5;
        float approachDist = Math.Max(1.5f, Math.Min(5f, shipDiameter * 0.15f));

        if (!GetDockApproachPosition(dock, 0, true, out navTargetPos) || !GetDockApproachPosition(dock, approachDist, true, out pathSegmentDir) ||
            connBlock == null || !connBlock.FatBlock.IsFunctional)
        { ShowStatusMessage(StatusMsg.ConnectorNotReady); StopAll(); return; }

        if (stuckSensitivity == 1 || (Vector3.Distance(shipPosition, navTargetPos) <= approachDist * 1.1f && !stateFirstTick))
            stuckSensitivity = 1;
        else
        {
            Vector3 toApproach = LocalTransformDirection(remoteControl, pathSegmentDir - shipPosition);
            Vector3 gravDir = LocalTransformDirection(remoteControl, remoteControl.GetNaturalGravity());
            float safeSpeed = CalculateMaxSpeed(toApproach, gravDir, null);
            dockSpeed2 = Math.Min(maxDockSpeed, safeSpeed);
        }

        SetFlightTargetFull(true, false, false, navTargetPos, navTargetPos - shipPosition, dockingSpeed, dockSpeed2);
        if (stateFirstTick) lastDistToTarget = (float)distToTarget;

        IMyShipConnector connectable = FindConnector(MyShipConnectorStatus.Connectable);
        if (connectable != null)
        {
            SetFlightTargetFull(false, false, false, navTargetPos, 0);
            if (stuckCounter > 0) stuckCounter = 0;
            stuckCounter--;
            if (stuckCounter < -5)
            {
                connectable.Connect();
                if (connectable.Status == MyShipConnectorStatus.Connected)
                {
                    if (shipMode == ShipMode.Shuttle) SetNavState(NavState.WaitForCommand);
                    else SetNavState(NavState.Unloading);
                    StopFlight();
                    SetLandingGears(landingGears, true);
                    return;
                }
            }
        }
        else
        {
            float distRounded = (float)Math.Round(distToTarget, 1);
            if (distRounded < lastDistToTarget) { stuckCounter = -1; lastDistToTarget = distRounded; }
            else stuckCounter++;
            if (stuckCounter > 20) { SetNavState(NavState.RetryDocking); return; }
        }
    }

    // [Unloading / WaitForCommand / WaitUranium / FillHydrogen / Charging] -> docked operations
    if (currentNavState == NavState.Unloading || currentNavState == NavState.WaitForCommand ||
        currentNavState == NavState.WaitUranium || currentNavState == NavState.FillHydrogen || currentNavState == NavState.Charging)
    {
        bool atHome = false, atJob = false;
        if (shipMode == ShipMode.Shuttle)
        {
            if (GetCurrentDock() == homeDock) atHome = true;
            else if (GetCurrentDock() == jobPosition) atJob = true;
        }

        if (stateFirstTick)
        {
            simulateShipFull = false;
            if (FindConnector(MyShipConnectorStatus.Connected) == null) { SetNavState(NavState.Undocking); return; }
            StopFlight();
            if (atHome) TriggerTimerBlock(undockConfig1.dockTimerName);
            if (atJob) TriggerTimerBlock(undockConfig2.dockTimerName);
            unloadComplete = false;
            batteryOk = false;
            hydrogenOk = false;
            uraniumOk = false;
        }

        if (FindConnector(MyShipConnectorStatus.Connected) == null) { StopAll(); ShowStatusMessage(StatusMsg.Interrupted); return; }

        // Check energy states
        if (jobState != JobState.Active || minBatteryPercent == -1 || batteryState == BatteryState.None) batteryOk = true;
        else if (batteryPercent >= 100f) batteryOk = true;
        else if (batteryPercent <= 99f) batteryOk = false;

        if (jobState != JobState.Active || minHydrogenPercent == -1 || hydrogenTanks.Count == 0) hydrogenOk = true;
        else if (hydrogenPercent >= 100f) hydrogenOk = true;
        else if (hydrogenPercent <= 99) hydrogenOk = false;

        if (jobState != JobState.Active || minUraniumKg == -1 || reactors.Count == 0) uraniumOk = true;
        else uraniumOk = uraniumAmount >= minUraniumKg;

        UndockConfig undockCfg = null;
        if (atHome) undockCfg = undockConfig1;
        if (atJob) undockCfg = undockConfig2;
        if (undockCfg != null && (undockCfg.trigger == UndockTrigger.OnBatteriesLow25 || undockCfg.trigger == UndockTrigger.OnBatteriesEmpty)) batteryOk = true;
        if (undockCfg != null && (undockCfg.trigger == UndockTrigger.OnBatteriesFull)) if (!unloadComplete) batteryOk = false;
        if (undockCfg != null && (undockCfg.trigger == UndockTrigger.OnHydrogenLow25 || undockCfg.trigger == UndockTrigger.OnHydrogenEmpty)) hydrogenOk = true;
        if (undockCfg != null && (undockCfg.trigger == UndockTrigger.OnHydrogenFull)) if (!unloadComplete) hydrogenOk = false;

        if (isSlowTick)
        {
            ChargeMode chargeMode = batteryOk ? ChargeMode.Auto : ChargeMode.Recharge;
            if (undockCfg != null && (undockCfg.trigger == UndockTrigger.OnBatteriesEmpty || undockCfg.trigger == UndockTrigger.OnBatteriesLow25))
                chargeMode = ChargeMode.Discharge;
            SetBatteryChargeMode(chargeMode);
            SetHydrogenStockpile(!hydrogenOk);
        }

        if (!unloadComplete)
        {
            if (shipMode == ShipMode.Shuttle)
                unloadComplete = jobState != JobState.Active || CheckShuttleUndock(stateFirstTick, true) || undockRequested;
            else
                unloadComplete = jobState != JobState.Active || IsCargoEmpty();
        }
        else
        {
            if (!batteryOk) SetNavState(NavState.Charging);
            if (!hydrogenOk) SetNavState(NavState.FillHydrogen);
            if (!uraniumOk) SetNavState(NavState.WaitUranium);
            stateFirstTick = false;
        }

        if (unloadComplete && batteryOk && hydrogenOk && uraniumOk)
        {
            SetBatteryChargeMode(ChargeMode.Auto);
            SetHydrogenStockpile(false);
            if (jobState == JobState.Active)
            {
                if (shipMode == ShipMode.Shuttle)
                {
                    if (GetCurrentDock() == homeDock) TriggerTimerBlock(undockConfig1.leaveTimerName);
                    else if (GetCurrentDock() == jobPosition) TriggerTimerBlock(undockConfig2.leaveTimerName);
                    if (GetCurrentDock() == homeDock) SetDockTarget(jobPosition, JobState.ActiveJob);
                    else SetDockTarget(homeDock, JobState.ActiveHome);
                }
            }
            SetNavState(NavState.Undocking);
            return;
        }
    }

    // [Undocking] -> disconnect and fly away from dock
    if (currentNavState == NavState.Undocking)
    {
        if (stateFirstTick)
        {
            IMyShipConnector connected = FindConnector(MyShipConnectorStatus.Connected);
            if (connected == null) { SetNavState(NavState.FlyToJobArea); return; }
            IMyShipConnector other = connected.OtherConnector;
            SetBlockEnabled(connected, false);
            SetLandingGears(landingGears, false);

            WaypointInfo dock = null;
            if (Vector3.Distance(connected.GetPosition(), homeDock.position) < 5f && homeDock.isSet) dock = homeDock;
            if (Vector3.Distance(connected.GetPosition(), jobPosition.position) < 5f && jobPosition.isSet) dock = jobPosition;

            if (dock != null)
            {
                if (!GetDockApproachPosition(dock, dockDist * shipDiameter, true, out navTargetPos))
                { ShowStatusMessage(StatusMsg.ConnectorNotReady); StopAll(); return; }
                SetFlightTarget(navTargetPos, 5);
                AlignToDown(dock.downDir, dock.forwardDir, dock.leftDir, false);
            }
            else SetFlightTarget(shipPosition + other.WorldMatrix.Forward * dockDist * shipDiameter, 5);

            if (jobState == JobState.Active) ShowStatusMessage(StatusMsg.Running);
        }
        if (distToTarget < wpReachedDist)
        {
            SetBlocksEnabled(connectors, true);
            SetNavState(NavState.FlyToJobArea);
            return;
        }
    }

    // [FlyToJobPos] -> fly directly to job area
    if (currentNavState == NavState.FlyToJobPos)
    {
        if (stateFirstTick)
        {
            SetSortersEnabled(true);
            SetBlocksEnabled(workTools, false);
            navTargetPos = jobPosition.position;
            SetFlightTarget(navTargetPos, 20);
            AlignToDown(jobPosition.downDir, jobPosition.forwardDir, jobPosition.leftDir, false);
        }
        if (distToTarget < wpReachedDist / 2) { SetNavState(NavState.AlignJob); return; }
    }

    // [FlyToPath] -> fly to nearest path waypoint
    if (currentNavState == NavState.FlyToPath)
    {
        if (stateFirstTick)
        {
            SetSortersEnabled(true);
            SetBlocksEnabled(workTools, false);
            int closest = -1;
            double closestDist = -1;
            for (int i = waypoints.Count - 1; i >= 0; i--)
            {
                double dist = Vector3.Distance(waypoints[i].position, shipPosition);
                if (closestDist == -1 || dist < closestDist) { closest = i; closestDist = dist; }
            }
            if (closest == -1) { SetNavState(NavState.FlyToDockArea); return; }
            navTargetPos = waypoints[closest].position;
            SetFlightTarget(navTargetPos, 10);
            AlignToDown(jobPosition.downDir, jobPosition.forwardDir, jobPosition.leftDir, false);
        }
        if (distToTarget < wpReachedDist) { SetNavState(NavState.FlyToDockArea); return; }
    }

    // [WaitEjection / WaitEjectionDrop / FlyToDropPos2] -> stone ejection
    if (currentNavState == NavState.FlyToDropPos2)
    {
        bool done = false;
        if (stateFirstTick)
        {
            SetSortersEnabled(true);
            if ((ejectionMode == EjectionMode.DropPosStone || ejectionMode == EjectionMode.DropPosStoIce) && IsNearPlanet() &&
                AngleBetween(miningForward, remoteControl.GetNaturalGravity()) < 25 && jobWidth >= 2 && jobHeight >= 2)
            {
                Vector3 dropPos = shipPosition;
                if (holeCol > 0 && holeRow < jobHeight - 1) dropPos = GetJobGridPosition(holeCol - 1, holeRow + 1, true);
                else if (holeCol < jobWidth - 1 && holeRow < jobHeight - 1) dropPos = GetJobGridPosition(holeCol + 1, holeRow + 1, true);
                else if (holeCol < jobWidth - 1 && holeRow > 0) dropPos = GetJobGridPosition(holeCol + 1, holeRow - 1, true);
                else if (holeCol > 0 && holeRow > 0) dropPos = GetJobGridPosition(holeCol - 1, holeRow - 1, true);
                else done = true;
                if (!done) SetFlightTarget(dropPos, 10);
            }
            else done = true;
        }
        if (distToTarget < wpReachedDist / 2) done = true;
        if (done) { SetNavState(NavState.WaitEjectionDrop); return; }
    }

    if (currentNavState == NavState.WaitEjection || currentNavState == NavState.WaitEjectionDrop)
    {
        if (stateFirstTick)
        {
            SetFlightTargetFull(true, true, false, shipPosition, 0);
            SetBlocksEnabled(workTools, false);
            SetSortersEnabled(true);
            stuckCounter = -1;
            stuckSensitivity = ejectionMode == EjectionMode.InMotionStone || ejectionMode == EjectionMode.InMotionStoIce ? 0 : -1;
        }

        bool energyLow = !CheckEnergy();
        int stoneCount = CountItems("STONE", "ORE", ItemLocation.All);
        if (ejectionMode == EjectionMode.CurPosStoIce || ejectionMode == EjectionMode.InMotionStoIce || ejectionMode == EjectionMode.DropPosStoIce)
            stoneCount += CountItems("ICE", "ORE", ItemLocation.All);
        bool hasStone = stoneCount > 0;

        bool ejectionStuck = false;
        if (stuckSensitivity >= 0)
        {
            float angle = (float)Math.Sin(DegToRad(stuckSensitivity)) * toolWidth / 3f;
            float angle2 = (float)Math.Cos(DegToRad(stuckSensitivity)) * toolHeight / 3f;
            Vector3 circlePos = GetJobGridPosition(holeCol, holeRow, true) + TransformLocalToWorld(miningForward, miningDown * -1, new Vector3(angle, angle2, 0));
            SetFlightTarget(circlePos, 0.3f);
            if (distToTarget < Math.Min(toolWidth, toolHeight) / 10f) stuckSensitivity += 5f;
            if (stuckSensitivity >= 360) stuckSensitivity = 0;
        }

        if (stuckCounter == -1 || stoneCount < stuckCounter) { stuckCounter = stoneCount; lastDistToTarget = 0; }
        else { lastDistToTarget++; if (lastDistToTarget > 50) ejectionStuck = true; }

        if (!hasStone || energyLow || ejectionStuck)
        {
            if (!energyLow)
            {
                int totalOre = CountItems("", "ORE", ItemLocation.All);
                if (IsShipFull(true)) energyLow = true;
                else if (100 - (SafeDiv(totalOre, oreCountBeforeEject) * 100) < minEjection) energyLow = true;
                else ShowStatusMessage(StatusMsg.Running);
            }
            if (ejectionStuck && energyLow) statusMessage = "Ejection failed";
            if (currentNavState == NavState.WaitEjectionDrop)
            {
                if (energyLow) { if (homeDock.isSet) SetNavState(NavState.FlyToPath); else { StopAll(); FlyToJobPosition(); statusMessage = "Can´t return, no dock found"; } }
                else SetNavState(NavState.FlyToXY);
            }
            else if (energyLow) SetNavState(NavState.ReturnToDock);
            else SetNavState(NavState.Mining);
            return;
        }
    }

    stateFirstTick = false;
}


// ========================== PATH SPEED CALCULATION ==========================

void CalculatePathSpeeds(List<WaypointInfo> wps, int dir, List<Vector3> targets, float targetDist, bool reset, ref int calcIdx)
{
    if (reset) { for (int i = 0; i < wps.Count; i++) wps[i].maxSpeed = 0; calcIdx = -1; return; }
    if (dir == 0) return;
    int step = dir * -1;
    if (calcIdx == -1) calcIdx = step > 0 ? 1 : wps.Count - 2;

    int iterations = 0;
    while (calcIdx >= 1 && calcIdx < wps.Count - 1)
    {
        if (iterations > 50) return;
        iterations++;
        try
        {
            if ((step < 0 && calcIdx >= 1) || (step > 0 && calcIdx <= wps.Count - 2))
            {
                WaypointInfo current = wps[calcIdx];
                bool nearTarget = false;
                for (int j = 0; j < targets.Count; j++)
                    if (Vector3.Distance(current.position, targets[j]) <= targetDist) { nearTarget = true; break; }

                if (!nearTarget)
                {
                    WaypointInfo prev = wps[calcIdx - step];
                    WaypointInfo next = wps[calcIdx + step];
                    Vector3 toNext = current.position - next.position;
                    Vector3 toPrev = prev.position - current.position;
                    Vector3 projected = current.position + Vector3.Normalize(toNext) * toPrev.Length();
                    Vector3 deviation = prev.position - projected;
                    Vector3 localDev = TransformToLocal(dir > 0 ? current.forwardDir : current.forwardDir * -1, current.downDir * -1, deviation);
                    Vector3 localPrev = TransformToLocal(dir > 0 ? current.forwardDir : current.forwardDir * -1, current.downDir * -1, toPrev);
                    Vector3 localGrav = TransformToLocal(dir > 0 ? current.forwardDir : current.forwardDir * -1, current.downDir * -1, current.gravity);

                    current.maxSpeed = (float)Math.Sqrt(Math.Pow(prev.maxSpeed, 2) + Math.Pow(CalculateMaxSpeed(-localPrev, localGrav, current), 2));

                    for (int j = 0; j < targets.Count; j++)
                        if (Vector3.Distance(prev.position, targets[j]) <= targetDist)
                        {
                            Vector3 localTarget = TransformToLocal(dir > 0 ? current.forwardDir : current.forwardDir * -1, current.downDir * -1, targets[j] - current.position);
                            float targetSpeed = CalculateMaxSpeed(-localTarget, localGrav, current);
                            current.maxSpeed = Math.Min(current.maxSpeed, targetSpeed) / 2f;
                        }

                    if (localDev.Length() == 0) localDev = new Vector3(0, 0, 1);
                    Vector3 localGravDir = TransformToLocal(current.forwardDir, current.downDir * -1, current.gravity);
                    float thrustForDev = GetThrustForDirection(-localDev, localGravDir, current);
                    float accel = SafeDiv(thrustForDev, shipMass);
                    float timeToStop = (float)Math.Sqrt(SafeDiv(localDev.Length() * 1.0f, 0.5f * accel));
                    current.maxSpeed = Math.Min(current.maxSpeed, (toPrev.Length() / timeToStop) * accelerationFactor);
                }
            }
        }
        catch { return; }
        calcIdx += step;
    }
    calcIdx = -1;
}

// ========================== 16_MiningProgress.cs ==========================
// ========================== PROGRESS / DETECTION / SPEED ==========================

void UpdateProgress(bool reset)
{
    if (reset) { jobProgress = 0; return; }
    if (shipMode == ShipMode.Shuttle) return;
    float done = currentHoleIndex * Math.Max(1, configDepth);
    if (lastHoleIndex == currentHoleIndex) done += Math.Min(configDepth, currentMineDepth);
    float total = jobWidth * jobHeight * Math.Max(1, configDepth);
    jobProgress = Math.Max(jobProgress, (float)Math.Min(done / total * 100.0, 100));
}

MyDetectedEntityType GetDetectedEntityType()
{
    try { if (IsBlockValid(sensor, true) && !sensor.LastDetectedEntity.IsEmpty()) return sensor.LastDetectedEntity.Type; }
    catch { }
    return MyDetectedEntityType.None;
}

float GetWorkSpeed(bool forward)
{
    if (shipMode == ShipMode.Grinder && GetDetectedEntityType() == MyDetectedEntityType.None && !IsBlockDamaged(sensor, true))
        return fastSpeed;
    else return forward ? workSpeedForward : workSpeedBackward;
}

// ========================== 17_Resources.cs ==========================
// ========================== ENERGY / CARGO / DAMAGE ==========================

void UpdateBatteryState()
{
    float maxPower = 0, currentPower = 0, input = 0, output = 0;
    for (int i = 0; i < batteries.Count; i++)
    {
        IMyBatteryBlock bat = batteries[i];
        if (!IsBlockValid(bat, true)) continue;
        maxPower += bat.MaxStoredPower;
        currentPower += bat.CurrentStoredPower;
        input += bat.CurrentInput;
        output += bat.CurrentOutput;
    }
    batteryPercent = (float)Math.Round(SafeDiv(currentPower, maxPower) * 100, 1);
    if (input >= output) batteryState = BatteryState.Charging;
    else batteryState = BatteryState.Discharging;
    if (input == 0 && output == 0 || batteryPercent == 100.0) batteryState = BatteryState.Idle;
    if (batteries.Count == 0) batteryState = BatteryState.None;
}

void UpdateHydrogen()
{
    float filled = 0;
    for (int i = 0; i < hydrogenTanks.Count; i++)
    {
        IMyGasTank tank = hydrogenTanks[i];
        if (!IsBlockValid(tank, true)) continue;
        filled += (float)tank.FilledRatio;
    }
    hydrogenPercent = SafeDiv(filled, hydrogenTanks.Count) * 100f;
}

void UpdateUranium()
{
    uraniumAmount = 0;
    try
    {
        for (int i = 0; i < reactors.Count; i++)
        {
            IMyReactor reactor = reactors[i];
            if (!IsBlockValid(reactor, true)) continue;
            List<MyInventoryItem> items = new List<MyInventoryItem>();
            reactor.GetInventory(0).GetItems(items);
            float amount = 0;
            for (int j = 0; j < items.Count; j++)
            {
                MyInventoryItem item = items[j];
                if (item.Type.SubtypeId.ToUpper() == "URANIUM" && item.Type.TypeId.ToUpper().Contains("_INGOT"))
                    amount += (float)item.Amount;
            }
            if (amount < uraniumAmount || i == 0) { uraniumAmount = amount; lowestUraniumReactor = reactor.CustomName; }
        }
    }
    catch (Exception e) { lastError = e; }
}

void UpdateCargoLoad()
{
    maxVolume = 0;
    currentVolume = 0;
    try
    {
        for (int i = 0; i < cargoBlocks.Count; i++)
        {
            IMyTerminalBlock block = cargoBlocks[i];
            if (!IsBlockValid(block, true)) continue;
            currentVolume += (float)block.GetInventory(0).CurrentVolume;
            maxVolume += (float)block.GetInventory(0).MaxVolume;
        }
        loadPercent = (float)Math.Min(Math.Round(SafeDiv(currentVolume, maxVolume) * 100, 1), 100.0);
    }
    catch (Exception e) { lastError = e; }
}

void UpdateInventory()
{
    try
    {
        itemList.Clear();
        for (int i = 0; i < cargoBlocks.Count; i++)
        {
            IMyTerminalBlock block = cargoBlocks[i];
            if (!IsBlockValid(block, true)) continue;
            AddBlockInventory(block, ItemLocation.InCargo);
        }
        if (ejectionMode != EjectionMode.Off)
        {
            for (int i = 0; i < allInventoryBlocks.Count; i++)
            {
                IMyTerminalBlock block = allInventoryBlocks[i];
                if (!IsBlockValid(block, true)) continue;
                AddBlockInventory(block, ItemLocation.InOther);
            }
        }
        SortItemList(itemList);
    }
    catch (Exception e) { lastError = e; }
}

void CheckDamage()
{
    if (damageCheckActive)
    {
        if (GetDamagedBlocks().Count > initialDamageCount)
        {
            damageCheckActive = false;
            if (onDamageBehavior != DamageBehavior.Ignore)
            {
                StopAll();
                if (onDamageBehavior == DamageBehavior.FlyToJob) FlyToJobPosition();
                if (onDamageBehavior == DamageBehavior.ReturnHome)
                    if (homeDock.isSet) FlyToHomePosition(); else FlyToJobPosition();
            }
            statusMessage = "Damage detected";
        }
    }
}

bool CheckEnergy()
{
    if (!initialized) return true;
    if (jobState == JobState.Active)
    {
        if (minBatteryPercent > 0 && batteryState != BatteryState.None)
            if (batteryPercent <= minBatteryPercent) { statusMessage = "Low energy! Move home"; return false; }
        if (minUraniumKg > 0 && reactors.Count > 0)
            if (uraniumAmount <= minUraniumKg) { statusMessage = "Low fuel: " + lowestUraniumReactor; return false; }
        if (minHydrogenPercent > 0 && hydrogenTanks.Count > 0)
            if (hydrogenPercent <= minHydrogenPercent) { statusMessage = "Low hydrogen"; return false; }
    }
    return true;
}

List<IMyTerminalBlock> GetDamagedBlocks()
{
    List<IMyTerminalBlock> damaged = new List<IMyTerminalBlock>();
    for (int i = 0; i < allGridBlocks.Count; i++)
    {
        IMyTerminalBlock block = allGridBlocks[i];
        if (IsBlockDamaged(block, false)) damaged.Add(block);
    }
    return damaged;
}

bool IsBlockDamaged(IMyTerminalBlock block, bool strict)
{
    return (!IsBlockValid(block, strict) || !block.IsFunctional);
}

bool IsBlockValid(IMyTerminalBlock block, bool strict)
{
    if (block == null) return false;
    try
    {
        IMyCubeBlock gridBlock = Me.CubeGrid.GetCubeBlock(block.Position).FatBlock;
        if (strict) return gridBlock == block;
        else return gridBlock.GetType() == block.GetType();
    }
    catch { return false; }
}


// ========================== INVENTORY HELPERS ==========================

float CountBlockItems(IMyTerminalBlock block, String typeName, String subtypeName, String[] exclude)
{
    float count = 0;
    for (int i = 0; i < block.InventoryCount; i++)
    {
        IMyInventory inv = block.GetInventory(i);
        List<MyInventoryItem> items = new List<MyInventoryItem>();
        inv.GetItems(items);
        foreach (MyInventoryItem item in items)
        {
            if (exclude != null && (exclude.Contains(item.Type.TypeId.ToUpper()) || exclude.Contains(item.Type.SubtypeId.ToUpper())))
                continue;
            if ((typeName == "" || item.Type.TypeId.ToUpper() == typeName) && (subtypeName == "" || item.Type.SubtypeId.ToUpper() == subtypeName))
                count += (float)item.Amount;
        }
    }
    return count;
}

ItemInfo FindItem(String name, String type, ItemLocation loc, bool create)
{
    name = name.ToUpper();
    type = type.ToUpper();
    for (int i = 0; i < itemList.Count; i++)
    {
        ItemInfo item = itemList[i];
        if (item.name.ToUpper() == name && item.type.ToUpper() == type && (item.location == loc || loc == ItemLocation.All))
            return item;
    }
    ItemInfo result = null;
    if (create) { result = new ItemInfo(name, type, 0, loc); itemList.Add(result); }
    return result;
}

int CountItems(String name, String type, ItemLocation loc)
{
    return CountItems(name, type, loc, null);
}

int CountItems(String name, String type, ItemLocation loc, String[] exclude)
{
    int count = 0;
    name = name.ToUpper();
    type = type.ToUpper();
    for (int i = 0; i < itemList.Count; i++)
    {
        ItemInfo item = itemList[i];
        if (exclude != null && exclude.Contains(item.name.ToUpper())) continue;
        if ((name == "" || item.name.ToUpper() == name) && (type == "" || item.type.ToUpper() == type) && (item.location == loc || loc == ItemLocation.All))
            count += item.amount;
    }
    return count;
}

void AddBlockInventory(IMyTerminalBlock block, ItemLocation loc)
{
    for (int i = 0; i < block.InventoryCount; i++)
    {
        List<MyInventoryItem> items = new List<MyInventoryItem>();
        block.GetInventory(i).GetItems(items);
        for (int j = 0; j < items.Count; j++)
            FindItem(items[j].Type.SubtypeId, items[j].Type.TypeId.Replace("MyObjectBuilder_", ""), loc, true).amount += (int)items[j].Amount;
    }
}

void SortItemList(List<ItemInfo> list)
{
    for (int i = list.Count - 1; i > 0; i--)
        for (int j = 0; j < i; j++)
        {
            ItemInfo a = list[j];
            ItemInfo b = list[j + 1];
            if (a.amount < b.amount) list.Move(j, j + 1);
        }
}

bool IsShipFull(bool showMsg)
{
    if (weightLimitEnabled && shipMode != ShipMode.Shuttle)
        if (maxWeight != -1 && shipMass >= maxWeight) { statusMessage = "Ship too heavy"; return true; }
    if (loadPercent >= maxLoadPercent || simulateShipFull)
    {
        simulateShipFull = false;
        statusMessage = "Ship is full";
        return true;
    }
    return false;
}

bool IsCargoEmpty()
{
    String[] exclude = null;
    if (!unloadIce) exclude = new string[] { "ICE" };
    if (shipMode == ShipMode.Miner) return CountItems("", "ORE", ItemLocation.InCargo, exclude) == 0;
    if (shipMode == ShipMode.Grinder) return CountItems("", "COMPONENT", ItemLocation.InCargo, exclude) == 0;
    else return CountItems("", "", ItemLocation.InCargo, exclude) == 0;
}

bool IsPlayerInCockpit()
{
    List<IMyCockpit> cockpits = new List<IMyCockpit>();
    gridTerminalSystem.GetBlocksOfType(cockpits, q => q.CubeGrid == Me.CubeGrid);
    for (int i = 0; i < cockpits.Count; i++)
        if (cockpits[i].IsUnderControl) return true;
    return false;
}

bool CheckShuttleUndock(bool reset, bool showStatus)
{
    IMyShipConnector conn = FindConnector(MyShipConnectorStatus.Connected);
    if (conn == null) return false;
    if (Vector3.Distance(homeDock.position, conn.GetPosition()) < 5)
        return CheckUndockTrigger(undockConfig1, reset, showStatus);
    if (Vector3.Distance(jobPosition.position, conn.GetPosition()) < 5)
        return CheckUndockTrigger(undockConfig2, reset, showStatus);
    return false;
}

bool CheckUndockTrigger(UndockConfig cfg, bool reset, bool showStatus)
{
    if (reset) cfg.Reset();
    cfg.UpdateTimer();
    bool triggered = false;
    String status = "";
    switch (cfg.trigger)
    {
        case UndockTrigger.OnCommand: { status = "Waiting for command"; triggered = false; break; }
        case UndockTrigger.OnShipFull: { status = "Waiting for cargo"; triggered = IsShipFull(true); break; }
        case UndockTrigger.OnShipEmpty: { status = "Unloading"; triggered = IsCargoEmpty(); break; }
        case UndockTrigger.OnTimerDelay: { triggered = true; break; }
        case UndockTrigger.OnBatteriesFull: { status = "Charging batteries"; triggered = batteryPercent >= 100f; break; }
        case UndockTrigger.OnBatteriesLow25: { status = "Discharging batteries"; triggered = batteryPercent <= 25f; break; }
        case UndockTrigger.OnBatteriesEmpty: { status = "Discharging batteries"; triggered = batteryPercent <= 0f; break; }
        case UndockTrigger.OnHydrogenFull: { status = "Filling up hydrogen"; triggered = hydrogenPercent >= 100f; break; }
        case UndockTrigger.OnHydrogenLow25: { status = "Unloading hydrogen"; triggered = hydrogenPercent <= 25f; break; }
        case UndockTrigger.OnHydrogenEmpty: { status = "Unloading hydrogen"; triggered = hydrogenPercent <= 0f; break; }
        case UndockTrigger.OnPlayerEntered:
        {
            bool inCockpit = IsPlayerInCockpit();
            if (!inCockpit) cfg.playerWasInCockpit = true;
            triggered = cfg.playerWasInCockpit && inCockpit;
            status = "Waiting for passengers";
            break;
        }
    }
    if (!triggered) cfg.CheckDelay(true);
    if (triggered && cfg.HasDelay())
    {
        triggered = cfg.CheckDelay(false);
        status = "Undocking in: " + FormatTime((int)Math.Max(0, cfg.delay - cfg.elapsedDelay));
    }
    if (showStatus) waitingStatusText = status;
    return triggered;
}

// ========================== 18_InventoryBalance.cs ==========================
// ========================== INVENTORY BALANCING ==========================

void BalanceInventory()
{
    if (!invBalancingEnabled) return;
    if (workTools.Count <= 1) return;

    float totalMax = 0, totalCurrent = 0;
    for (int i = 0; i < workTools.Count; i++)
    {
        IMyTerminalBlock tool = workTools[i];
        if (IsBlockDamaged(tool, true)) continue;
        totalMax += (float)tool.GetInventory(0).MaxVolume;
        totalCurrent += (float)tool.GetInventory(0).CurrentVolume;
    }
    float avgFill = (float)Math.Round(SafeDiv(totalCurrent, totalMax), 5);

    for (int pass = 0; pass < Math.Max(1, Math.Floor(workTools.Count / 10f)); pass++)
    {
        float highestFill = 0, lowestFill = 0;
        float highestMax = 0, lowestMax = 0;
        IMyTerminalBlock fullest = null, emptiest = null;

        for (int i = 0; i < workTools.Count; i++)
        {
            IMyTerminalBlock tool = workTools[i];
            if (IsBlockDamaged(tool, true)) continue;
            float maxVol = (float)tool.GetInventory(0).MaxVolume;
            float fillRatio = SafeDiv((float)tool.GetInventory(0).CurrentVolume, maxVol);

            if (fullest == null || fillRatio > highestFill) { fullest = tool; highestFill = fillRatio; highestMax = maxVol; }
            if (emptiest == null || fillRatio < lowestFill) { emptiest = tool; lowestFill = fillRatio; lowestMax = maxVol; }
        }

        if (fullest == null || emptiest == null || fullest == emptiest) return;

        if (checkConveyorSystem && !fullest.GetInventory(0).IsConnectedTo(emptiest.GetInventory(0)))
        {
            if (balanceFailCount > 20) statusMessage = "Inventory balancing failed";
            else balanceFailCount++;
            return;
        }
        balanceFailCount = 0;

        List<MyInventoryItem> items = new List<MyInventoryItem>();
        fullest.GetInventory(0).GetItems(items);
        float volPerUnit = 0;
        if (items.Count == 0) continue;
        MyInventoryItem firstItem = items[0];
        String itemKey = firstItem.Type.TypeId + firstItem.Type.SubtypeId;

        if (!volumePerItem.TryGetValue(itemKey, out volPerUnit))
        {
            if (MeasureItemVolume(fullest.GetInventory(0), 0, emptiest.GetInventory(0), out volPerUnit))
                volumePerItem.Add(itemKey, volPerUnit);
            else return;
        }

        float toTransferFromFull = ((highestFill - avgFill) * highestMax / volPerUnit);
        float toTransferToEmpty = ((avgFill - lowestFill) * lowestMax / volPerUnit);
        int transferAmount = (int)Math.Min(toTransferToEmpty, toTransferFromFull);
        if (transferAmount <= 0) return;

        if ((float)firstItem.Amount < transferAmount)
            fullest.GetInventory(0).TransferItemTo(emptiest.GetInventory(0), 0, null, null, firstItem.Amount);
        else
            fullest.GetInventory(0).TransferItemTo(emptiest.GetInventory(0), 0, null, null, transferAmount);
    }
}

bool MeasureItemVolume(IMyInventory source, int idx, IMyInventory dest, out float volume)
{
    volume = 0;
    float beforeVol = (float)source.CurrentVolume;
    List<MyInventoryItem> beforeItems = new List<MyInventoryItem>();
    source.GetItems(beforeItems);
    float beforeCount = 0;
    for (int i = 0; i < beforeItems.Count; i++) beforeCount += (float)beforeItems[i].Amount;

    source.TransferItemTo(dest, idx, null, null, 1);
    float volDiff = beforeVol - (float)source.CurrentVolume;

    List<MyInventoryItem> afterItems = new List<MyInventoryItem>();
    source.GetItems(afterItems);
    float afterCount = 0;
    for (int i = 0; i < afterItems.Count; i++) afterCount += (float)afterItems[i].Amount;

    if (volDiff == 0f || !InRange(0.9999, beforeCount - afterCount, 1.0001))
        return false;
    volume = volDiff;
    return true;
}

// ========================== 19_Flight.cs ==========================
// ========================== FLIGHT CONTROL ==========================

void UpdateFlight()
{
    Vector3 toTarget = navTargetPos - shipPosition;
    if (toTarget.Length() == 0) toTarget = new Vector3(0, 0, -1);
    Vector3 localTarget = LocalTransformDirection(remoteControl, toTarget);
    Vector3 targetDir = Vector3.Normalize(localTarget);
    Vector3 localGravity = LocalTransformDirection(remoteControl, remoteControl.GetNaturalGravity());

    float speedFactorPath = pathSpeed > 0 ? Math.Max(0, 1 - AngleBetween(toTarget, flightPathDir) / 5) : 0;
    float maxSpeed = (float)Math.Min((maxApproachSpeed > 0 ? maxApproachSpeed : 1000f), Math.Max(CalculateMaxSpeed(-localTarget, localGravity, null), pathSpeed * speedFactorPath));

    if (!thrustEnabled) maxSpeed = 0;
    if (slowOnMisalign) maxSpeed = Math.Max(0, 1 - currentAngleError / alignThreshold) * maxSpeed;
    if (generalSpeedLimit > 0) maxSpeed = Math.Min(generalSpeedLimit, maxSpeed);
    if (softApproach) maxSpeed *= (float)Math.Min(1, SafeDiv(toTarget.Length(), wpReachedDist) / 2);

    Vector3 localVelocity = LocalTransformDirection(remoteControl, remoteControl.GetShipVelocities().LinearVelocity);
    float alignmentFactor = (float)(Math.Max(0, 15 - AngleBetween(-targetDir, -localVelocity)) / 15) * 0.85f + 0.15f;
    thrustEfficiency += Math.Sign(alignmentFactor - thrustEfficiency) / 10f;

    Vector3 desiredVelocity = targetDir * maxSpeed * thrustEfficiency - (localVelocity);
    Vector3 availableThrust = GetThrustMap(desiredVelocity, null);

    if (thrustActive && distToTarget > 0.1f)
    {
        desiredVelocity.X *= SmoothThrust(desiredVelocity.X, ref thrustMultiplier.X, 1f, availableThrust.X, 20);
        desiredVelocity.Y *= SmoothThrust(desiredVelocity.Y, ref thrustMultiplier.Y, 1f, availableThrust.Y, 20);
        desiredVelocity.Z *= SmoothThrust(desiredVelocity.Z, ref thrustMultiplier.Z, 1f, availableThrust.Z, 20);
    }
    else thrustMultiplier = new Vector3(1, 1, 1);

    thrustForce = shipMass * desiredVelocity - localGravity * shipMass;
    ApplyThrustOverride(thrustForce, thrustActive);
    distToTarget = Vector3.Distance(shipPosition, navTargetPos);
}

float SmoothThrust(float value, ref float state, float step, float maxThrust, float maxMultiplier)
{
    value = Math.Sign(Math.Round(value, 2));
    if (value == Math.Sign(state)) state += Math.Sign(state) * step;
    else state = value;
    if (value == 0) state = 1;
    float result = Math.Abs(state);
    if (result < maxMultiplier || maxThrust == 0) return result;
    state = Math.Min(maxMultiplier, Math.Max(-maxMultiplier, state));
    result = Math.Abs(maxThrust);
    return result;
}

void UpdateGyros()
{
    float pitch = 90, yaw = 90, roll = 90;
    float speed = (float)(Me.CubeGrid.GridSizeEnum == MyCubeSize.Small ? gyroSpeedSmall : gyroSpeedLarge) / 100f;

    Vector3 forward, localDown, localForward, localLeft;

    if (lookAtTarget)
    {
        forward = Vector3.Normalize(navTargetPos - shipPosition);
        localForward = LocalTransformDirection(remoteControl, forward);
        localDown = LocalTransformDirection(remoteControl, targetDown);
        pitch = AngleBetween(localForward, new Vector3(0, -1, 0)) - 90;
        yaw = AngleWithSign(localDown, new Vector3(-1, 0, 0), localDown.Y);
        roll = AngleWithSign(localForward, new Vector3(-1, 0, 0), localForward.Z);
    }
    else
    {
        forward = targetForward;
        localDown = LocalTransformDirection(remoteControl, targetDown);
        localForward = LocalTransformDirection(remoteControl, targetForward);
        localLeft = LocalTransformDirection(remoteControl, targetLeft);
        pitch = AngleWithSign(localDown, new Vector3(0, 0, 1), localDown.Y);
        yaw = AngleWithSign(localDown, new Vector3(-1, 0, 0), localDown.Y);
        roll = AngleWithSign(localLeft, new Vector3(0, 0, 1), localLeft.X);
    }

    if (alignToGravity && IsNearPlanet())
    {
        Vector3 gravDir = remoteControl.GetNaturalGravity();
        localDown = LocalTransformDirection(remoteControl, gravDir);
        pitch = AngleWithSign(localDown, new Vector3(0, 0, 1), localDown.Y);
        yaw = AngleWithSign(localDown, new Vector3(-1, 0, 0), localDown.Y);
    }

    if (!InRange(-45, yaw, 45)) { pitch = 0; roll = 0; }
    if (!InRange(-45, roll, 45)) pitch = 0;

    SetGyroOverride(gyroActive, 1, (-pitch) * speed, (-roll) * speed, (-yaw) * speed);
    currentAngleError = Math.Max(Math.Abs(pitch), Math.Max(Math.Abs(yaw), Math.Abs(roll)));
    isAligned = currentAngleError <= alignThreshold;
}

// ========================== 20_ThrustGyro.cs ==========================
// ========================== THRUST SYSTEM ==========================

void CalculateThrustVectors(IMyTerminalBlock reference)
{
    if (reference == null) return;
    totalThrustMap = new float[3, 2];
    thrustByType = new Dictionary<string, float[,]>();

    for (int i = 0; i < thrusters.Count; i++)
    {
        IMyThrust thruster = thrusters[i];
        if (!thruster.IsFunctional) continue;
        Vector3 dir = LocalTransformDirection(reference, thruster.WorldMatrix.Backward);
        float effective = thruster.MaxEffectiveThrust;

        if (Math.Round(dir.X, 2) != 0.0)
            if (dir.X >= 0) totalThrustMap[0, 0] += effective; else totalThrustMap[0, 1] -= effective;
        if (Math.Round(dir.Y, 2) != 0.0)
            if (dir.Y >= 0) totalThrustMap[1, 0] += effective; else totalThrustMap[1, 1] -= effective;
        if (Math.Round(dir.Z, 2) != 0.0)
            if (dir.Z >= 0) totalThrustMap[2, 0] += effective; else totalThrustMap[2, 1] -= effective;

        String typeName = GetThrusterType(thruster);
        float[,] typeMap = null;
        if (thrustByType.ContainsKey(typeName)) typeMap = thrustByType[typeName];
        else { typeMap = new float[3, 2]; thrustByType.Add(typeName, typeMap); }

        float maxThrust = thruster.MaxThrust;
        if (Math.Round(dir.X, 2) != 0.0)
            if (dir.X >= 0) typeMap[0, 0] += maxThrust; else typeMap[0, 1] -= maxThrust;
        if (Math.Round(dir.Y, 2) != 0.0)
            if (dir.Y >= 0) typeMap[1, 0] += maxThrust; else typeMap[1, 1] -= maxThrust;
        if (Math.Round(dir.Z, 2) != 0.0)
            if (dir.Z >= 0) typeMap[2, 0] += maxThrust; else typeMap[2, 1] -= maxThrust;
    }
}

static String GetThrusterType(IMyThrust thruster)
{
    return thruster.BlockDefinition.SubtypeId;
}

Vector3 LookupThrustMap(Vector3 direction, float[,] map)
{
    return new Vector3(
        direction.X >= 0 ? map[0, 0] : map[0, 1],
        direction.Y >= 0 ? map[1, 0] : map[1, 1],
        direction.Z >= 0 ? map[2, 0] : map[2, 1]);
}

bool GetThrusterEfficiency(WaypointInfo wp, String typeName, out float efficiency)
{
    efficiency = 0;
    int idx = thrusterTypeNames.IndexOf(typeName);
    if (idx == -1 || wp.thrusterEfficiency == null || idx >= wp.thrusterEfficiency.Length)
        return false;
    efficiency = wp.thrusterEfficiency[idx];
    if (efficiency == -1) return false;
    return true;
}

Vector3 GetThrustMap(Vector3 direction, WaypointInfo wp)
{
    if (wp != null)
    {
        Vector3 result = new Vector3();
        for (int i = 0; i < thrustByType.Keys.Count; i++)
        {
            String typeName = thrustByType.Keys.ElementAt(i);
            float eff = 0;
            if (!GetThrusterEfficiency(wp, typeName, out eff))
                return LookupThrustMap(direction, totalThrustMap);
            result += LookupThrustMap(direction, thrustByType.Values.ElementAt(i)) * eff;
        }
        return result;
    }
    return LookupThrustMap(direction, totalThrustMap);
}

float GetThrustForDirection(Vector3 direction, WaypointInfo wp)
{
    return GetThrustForDirection(direction, new Vector3(), wp);
}

float GetThrustForDirection(Vector3 direction, Vector3 gravity, WaypointInfo wp)
{
    Vector3 thrust = GetThrustMap(direction, wp);
    Vector3 effective = thrust + gravity * shipMass;
    float ratio = (effective / direction).AbsMin();
    return (float)(direction * ratio).Length();
}

float CalculateMaxSpeed(Vector3 localDir, Vector3 localGravity, WaypointInfo wp)
{
    if (localDir.Length() == 0) return 0;
    float alignFactor = 1;
    if (localGravity.Length() > 0) alignFactor = Math.Min(1, AngleBetween(-localGravity, localDir) / 90) * 0.4f + 0.6f;
    float thrustForce = GetThrustForDirection(localDir, localGravity, wp);
    if (thrustForce == 0) return 0.1f;
    float accel = SafeDiv(thrustForce, shipMass);
    float timeToStop = (float)Math.Sqrt(SafeDiv(localDir.Length(), accel * 0.5f));
    return accel * timeToStop * alignFactor * accelerationFactor;
}

float GravityThrustRatio(Vector3 forward, Vector3 up, Vector3 gravity, WaypointInfo wp)
{
    if (gravity.Length() == 0f) return 0;
    Vector3 localGrav = TransformToLocal(forward, up, Vector3.Normalize(gravity));
    float force = GetThrustForDirection(-localGrav, wp);
    return force / gravity.Length();
}

void CalculateMaxWeight(bool init)
{
    float weight = 0;
    float factor = 0.9f;
    if (init)
    {
        maxWeight = -1;
        waypointCalcIndex = 0;
        currentPathWaypoint = null;

        if (jobState != JobState.NoJob && jobPosition.gravity.Length() != 0)
        {
            weight = factor * GravityThrustRatio(jobPosition.forwardDir, jobPosition.downDir * -1, jobPosition.gravity, null);
            if (weight < maxWeight || maxWeight == -1) maxWeight = weight;
        }
        if (homeDock.isSet && homeDock.gravity.Length() != 0)
        {
            weight = factor * GravityThrustRatio(homeDock.forwardDir, homeDock.downDir * -1, homeDock.gravity, null);
            if (weight < maxWeight || maxWeight == -1) maxWeight = weight;
        }
        return;
    }

    // Process waypoints incrementally
    if (waypointCalcIndex == -1) return;
    if (waypointCalcIndex >= 0)
    {
        int processed = 0;
        while (waypointCalcIndex < waypoints.Count)
        {
            if (processed > 100) return;
            processed++;
            WaypointInfo wp = waypoints[waypointCalcIndex];
            if (wp.gravity.Length() != 0f)
            {
                weight = factor * Math.Min(
                    GravityThrustRatio(wp.forwardDir, wp.downDir * -1, wp.gravity, wp),
                    GravityThrustRatio(wp.forwardDir * -1, wp.downDir * -1, wp.gravity, wp));
                if (weight < maxWeight || maxWeight == -1) maxWeight = weight;
            }
            else currentPathWaypoint = wp;
            waypointCalcIndex++;
        }
        waypointCalcIndex = -1;
    }

    bool hasAtmoThrusters = true;
    float minThrust = 0;
    if (currentPathWaypoint != null)
    {
        for (int i = 0; i < thrustByType.Count; i++)
        {
            String typeName = thrustByType.Keys.ElementAt(i);
            float[,] typeMap = thrustByType.Values.ElementAt(i);
            float eff = 0;
            if (!GetThrusterEfficiency(currentPathWaypoint, typeName, out eff))
            {
                hasAtmoThrusters = false;
                break;
            }
            for (int a = 0; a < typeMap.GetLength(0); a++)
                for (int b = 0; b < typeMap.GetLength(1); b++)
                {
                    float absThrust = Math.Abs(typeMap[a, b] * eff);
                    if (absThrust == 0) continue;
                    hasAtmoThrusters = true;
                    if (minThrust == 0 || absThrust < minThrust) minThrust = absThrust;
                }
        }
    }

    if (!hasAtmoThrusters)
    {
        for (int a = 0; a < totalThrustMap.GetLength(0); a++)
            for (int b = 0; b < totalThrustMap.GetLength(1); b++)
            {
                float absThrust = Math.Abs(totalThrustMap[a, b]);
                if (absThrust == 0) continue;
                if (minThrust == 0 || absThrust < minThrust) minThrust = absThrust;
            }
    }

    if (minThrust > 0)
    {
        float minAccel = Me.CubeGrid.GridSizeEnum == MyCubeSize.Small ? minAccelerationSmall : minAccelerationLarge;
        weight = SafeDiv(minThrust, minAccel);
        if (weight > 0)
            if (weight < maxWeight || maxWeight == -1) maxWeight = weight;
    }
}

void ApplyThrustOverride(Vector3 force, bool enable)
{
    if (!enable)
    {
        for (int i = 0; i < thrusters.Count; i++)
            thrusters[i].SetValueFloat("Override", 0.0f);
        return;
    }

    Vector3 available = GetThrustMap(force, null);
    float xRatio = Math.Min(1, Math.Abs(SafeDiv(force.X, available.X)));
    float yRatio = Math.Min(1, Math.Abs(SafeDiv(force.Y, available.Y)));
    float zRatio = Math.Min(1, Math.Abs(SafeDiv(force.Z, available.Z)));

    for (int i = 0; i < thrusters.Count; i++)
    {
        IMyThrust thruster = thrusters[i];
        Vector3 dir = RoundVector(LocalTransformDirection(remoteControl, thruster.WorldMatrix.Backward), 1);
        if (dir.X != 0 && Math.Sign(dir.X) == Math.Sign(force.X))
            thruster.SetValueFloat("Override", thruster.MaxThrust * xRatio);
        else if (dir.Y != 0 && Math.Sign(dir.Y) == Math.Sign(force.Y))
            thruster.SetValueFloat("Override", thruster.MaxThrust * yRatio);
        else if (dir.Z != 0 && Math.Sign(dir.Z) == Math.Sign(force.Z))
            thruster.SetValueFloat("Override", thruster.MaxThrust * zRatio);
        else
            thruster.SetValueFloat("Override", 0.0f);
    }
}

void SetGyroOverride(bool enable, float power, float pitch, float roll, float yaw)
{
    for (int i = 0; i < gyros.Count; i++)
    {
        IMyGyro gyro = gyros[i];
        gyro.GyroOverride = enable;
        if (!enable) gyro.GyroPower = 100;
        else gyro.GyroPower = power;
        if (!enable) continue;

        Vector3 fwd = remoteControl.WorldMatrix.Forward;
        Vector3 right = remoteControl.WorldMatrix.Right;
        Vector3 up = remoteControl.WorldMatrix.Up;
        Vector3 gFwd = gyro.WorldMatrix.Forward;
        Vector3 gUp = gyro.WorldMatrix.Up;
        Vector3 gRight = gyro.WorldMatrix.Left * -1;

        if (gFwd == fwd) gyro.SetValueFloat("Roll", yaw);
        else if (gFwd == (fwd * -1)) gyro.SetValueFloat("Roll", yaw * -1);
        else if (gUp == (fwd * -1)) gyro.SetValueFloat("Yaw", yaw);
        else if (gUp == fwd) gyro.SetValueFloat("Yaw", yaw * -1);
        else if (gRight == fwd) gyro.SetValueFloat("Pitch", yaw);
        else if (gRight == (fwd * -1)) gyro.SetValueFloat("Pitch", yaw * -1);

        if (gRight == (right * -1)) gyro.SetValueFloat("Pitch", pitch);
        else if (gRight == right) gyro.SetValueFloat("Pitch", pitch * -1);
        else if (gUp == right) gyro.SetValueFloat("Yaw", pitch);
        else if (gUp == (right * -1)) gyro.SetValueFloat("Yaw", pitch * -1);
        else if (gFwd == (right * -1)) gyro.SetValueFloat("Roll", pitch);
        else if (gFwd == right) gyro.SetValueFloat("Roll", pitch * -1);

        if (gUp == (up * -1)) gyro.SetValueFloat("Yaw", roll);
        else if (gUp == up) gyro.SetValueFloat("Yaw", roll * -1);
        else if (gRight == up) gyro.SetValueFloat("Pitch", roll);
        else if (gRight == (up * -1)) gyro.SetValueFloat("Pitch", roll * -1);
        else if (gFwd == up) gyro.SetValueFloat("Roll", roll);
        else if (gFwd == (up * -1)) gyro.SetValueFloat("Roll", roll * -1);
    }
}

// ========================== 21_NavTargets.cs ==========================
// ========================== NAV TARGET SETTERS ==========================

void StopGyroOverride() { this.gyroActive = false; }

void AlignToDown(Vector3 down, Vector3 fwd, Vector3 left, float threshold, bool useGravity)
{
    AlignToGravity(down, threshold, useGravity);
    alignThreshold = threshold;
    lookAtTarget = false;
    this.targetForward = fwd;
    this.targetLeft = left;
}

void AlignToDown(Vector3 down, Vector3 fwd, Vector3 left, bool useGravity)
{
    AlignToDown(down, fwd, left, 2f, useGravity);
}

void AlignToGravity(Vector3 down, float threshold, bool useGravity)
{
    alignThreshold = threshold;
    this.gyroActive = true;
    this.alignToGravity = useGravity;
    lookAtTarget = true;
    isAligned = false;
    this.targetDown = down;
}

void StopFlight()
{
    SetFlightTargetFull(false, false, false, navTargetPos, 0);
    thrustActive = false;
}

void SetFlightTarget(Vector3 target, float speed)
{
    SetFlightTargetFull(true, false, false, target, speed);
}

void SetFlightTargetFull(bool enable, bool soft, bool useSegment, Vector3 target, float speed)
{
    SetFlightTargetFull(enable, soft, useSegment, target, target - shipPosition, 0.0f, speed);
}

void SetFlightTargetFull(bool enable, bool soft, bool useSegment, Vector3 target, Vector3 segDir, float segSpeed, float speed)
{
    thrustActive = true;
    this.thrustEnabled = enable;
    navTargetPos = target;
    this.maxApproachSpeed = speed;
    this.pathSpeed = segSpeed;
    this.softApproach = soft;
    this.slowOnMisalign = useSegment;
    this.flightPathDir = segDir;
    distToTarget = Vector3.Distance(target, shipPosition);
}

// ========================== 22_BlockScan.cs ==========================
// ========================== BLOCK MANAGEMENT ==========================

void ScanAllBlocks() { gridTerminalSystem.GetBlocksOfType(allGridBlocks, IsOnSameGrid); }

void ScanBlocks1()
{
    if (resetRequested) { setupErrorLevel = 2; return; }

    List<IMyRemoteControl> remotes = new List<IMyRemoteControl>();
    List<IMySensorBlock> sensors = new List<IMySensorBlock>();
    List<IMyTerminalBlock> taggedBlocks = new List<IMyTerminalBlock>();

    gridTerminalSystem.GetBlocksOfType(remotes, IsOnSameGrid);
    gridTerminalSystem.GetBlocksOfType(lcdPanels, IsOnSameGrid);
    gridTerminalSystem.GetBlocksOfType(sensors, IsOnSameGrid);
    gridTerminalSystem.SearchBlocksOfName(pamTag.Substring(0, pamTag.Length - 1) + ":", taggedBlocks,
        q => q.CubeGrid == Me.CubeGrid && q is IMyTextSurfaceProvider);

    lcdPanels = FilterByTag(lcdPanels, pamTag, true);
    SetupDebugPanels();
    SetupTaggedSurfaces(taggedBlocks);
    SetLCDFormat(lcdPanels, setLCDFontAndSize, 1.4f, false);
    SetLCDFormat(textSurfaces, setLCDFontAndSize, 1.4f, true);

    List<IMySensorBlock> taggedSensors = FilterByTag(sensors, pamTag, true);
    if (taggedSensors.Count > 0) sensor = taggedSensors[0]; else sensor = null;

    if (shipMode == ShipMode.Miner)
    {
        gridTerminalSystem.GetBlocksOfType(workTools, q => q.CubeGrid == Me.CubeGrid && q is IMyShipDrill);
        if (workTools.Count == 0) { setupErrorLevel = 1; statusMessage = "Drills are missing"; }
    }
    else if (shipMode == ShipMode.Grinder)
    {
        gridTerminalSystem.GetBlocksOfType(workTools, q => q.CubeGrid == Me.CubeGrid && q is IMyShipGrinder);
        if (workTools.Count == 0) { setupErrorLevel = 1; statusMessage = "Grinders are missing"; }
        if (sensor == null) { setupErrorLevel = 1; statusMessage = "Sensor is missing"; }
    }
    else if (shipMode == ShipMode.Shuttle)
    {
        gridTerminalSystem.GetBlocksOfType(timerBlocks, q => q.CubeGrid == Me.CubeGrid);
    }

    List<IMyRemoteControl> taggedRemotes = FilterByTag(remotes, pamTag, true);
    if (taggedRemotes.Count > 0) remotes = taggedRemotes;
    if (remotes.Count > 0) remoteControl = remotes[0];
    else { remoteControl = null; setupErrorLevel = 2; statusMessage = "Remote is missing"; return; }

    shipDiameter = (float)remoteControl.CubeGrid.WorldVolume.Radius * 2;
    referenceWorkTool = null;

    if (shipMode != ShipMode.Shuttle)
    {
        CalculateToolDimensions();
        if (workTools.Count > 0 && referenceWorkTool != null)
        {
            if (sensor != null && (referenceWorkTool.WorldMatrix.Forward != sensor.WorldMatrix.Forward ||
                !(remoteControl.WorldMatrix.Forward == sensor.WorldMatrix.Up || remoteControl.WorldMatrix.Down == sensor.WorldMatrix.Down)))
            { setupErrorLevel = 1; statusMessage = "Wrong sensor direction"; }
            if (referenceWorkTool.WorldMatrix.Forward != remoteControl.WorldMatrix.Forward &&
                referenceWorkTool.WorldMatrix.Forward != remoteControl.WorldMatrix.Down)
            { setupErrorLevel = 2; statusMessage = "Wrong remote direction"; }
        }
    }
}

void ScanBlocks2()
{
    gridTerminalSystem.GetBlocksOfType(landingGears, IsOnSameGrid);
    gridTerminalSystem.GetBlocksOfType(connectors, IsOnSameGrid);
    gridTerminalSystem.GetBlocksOfType(thrusters, IsOnSameGrid);
    gridTerminalSystem.GetBlocksOfType(gyros, IsOnSameGrid);
    gridTerminalSystem.GetBlocksOfType(batteries, IsOnSameGrid);
    gridTerminalSystem.GetBlocksOfType(reactors, IsOnSameGrid);
    gridTerminalSystem.GetBlocksOfType(hydrogenTanks,
        q => q.CubeGrid == Me.CubeGrid && q.BlockDefinition.ToString().ToUpper().Contains("HYDROGEN"));
    gridTerminalSystem.GetBlocksOfType(sorters, IsOnSameGrid);

    if (Me.CubeGrid.GridSizeEnum == MyCubeSize.Small)
        connectors = FilterByTag(connectors, "ConnectorMedium", false);
    else
        connectors = FilterByTag(connectors, "Connector", false);

    List<IMyShipConnector> taggedConns = FilterByTag(connectors, pamTag, true);
    if (taggedConns.Count > 0) connectors = taggedConns;

    if (setupErrorLevel <= 1)
    {
        if (connectors.Count == 0) { setupErrorLevel = 1; statusMessage = "Connector is missing"; }
        if (gyros.Count == 0) { setupErrorLevel = 1; statusMessage = "Gyros are missing"; }
        if (thrusters.Count == 0) { setupErrorLevel = 1; statusMessage = "Thrusters are missing"; }
    }

    List<IMyConveyorSorter> taggedSorters = FilterByTag(sorters, pamTag, true);
    if (taggedSorters.Count > 0) sorters = taggedSorters;
    List<IMyLandingGear> taggedGears = FilterByTag(landingGears, pamTag, true);
    if (taggedGears.Count > 0) landingGears = taggedGears;
    for (int i = 0; i < landingGears.Count; i++) landingGears[i].AutoLock = false;
    List<IMyBatteryBlock> taggedBatteries = FilterByTag(batteries, pamTag, true);
    if (taggedBatteries.Count > 0) batteries = taggedBatteries;
    List<IMyGasTank> taggedTanks = FilterByTag(hydrogenTanks, pamTag, true);
    if (taggedTanks.Count > 0) hydrogenTanks = taggedTanks;
}

void ScanBlocks3()
{
    gridTerminalSystem.GetBlocksOfType(allInventoryBlocks, q => q.CubeGrid == Me.CubeGrid && q.InventoryCount > 0);
    cargoBlocks.Clear();
    for (int i = allInventoryBlocks.Count - 1; i >= 0; i--)
    {
        if (IsCargoBlock(allInventoryBlocks[i]))
        {
            cargoBlocks.Add(allInventoryBlocks[i]);
            allInventoryBlocks.RemoveAt(i);
        }
    }
}

bool IsCargoBlock(IMyTerminalBlock block)
{
    if (block is IMyCargoContainer) return true;
    if (block is IMyShipDrill) return true;
    if (block is IMyShipGrinder) return true;
    if (block is IMyShipConnector)
    {
        if (((IMyShipConnector)block).ThrowOut) return false;
        if (Me.CubeGrid.GridSizeEnum != MyCubeSize.Large && HasTag(block, "ConnectorSmall", false))
            return false;
        else return true;
    }
    return false;
}

void CalculateToolDimensions()
{
    if (remoteControl == null) return;
    referenceWorkTool = null;

    float minX = 0, maxX = 0, minY = 0, maxY = 0, minZ = 0, maxZ = 0;
    List<IMyTerminalBlock> tagged = FilterByTag(workTools, pamTag, true);
    bool noTagged = tagged.Count == 0;
    if (tagged.Count > 0) referenceWorkTool = tagged[0];
    else if (workTools.Count > 0) referenceWorkTool = workTools[0];

    int count = 0;
    for (int i = 0; i < workTools.Count; i++)
    {
        if (workTools[i].WorldMatrix.Forward != referenceWorkTool.WorldMatrix.Forward)
        {
            if (noTagged) { setupErrorLevel = 2; statusMessage = "Mining direction is unclear!"; return; }
            continue;
        }
        count++;
        Vector3 localPos = GetLocalPosition(remoteControl, workTools[i].GetPosition());
        if (i == 0) { minX = localPos.X; maxX = localPos.X; minY = localPos.Y; maxY = localPos.Y; minZ = localPos.Z; maxZ = localPos.Z; }
        maxX = Math.Max(localPos.X, maxX); minX = Math.Min(localPos.X, minX);
        maxY = Math.Max(localPos.Y, maxY); minY = Math.Min(localPos.Y, minY);
        maxZ = Math.Max(localPos.Z, maxZ); minZ = Math.Min(localPos.Z, minZ);
    }
    toolWidth = (maxX - minX) * (1 - widthOverlap) + drillRadius * 2;
    toolHeight = (maxY - minY) * (1 - heightOverlap) + drillRadius * 2;
    if (referenceWorkTool != null && referenceWorkTool.WorldMatrix.Forward == remoteControl.WorldMatrix.Down)
        toolHeight = (maxZ - minZ) * (1 - heightOverlap) + drillRadius * 2;
}

List<T> FilterByTag<T>(List<T> list, String tag, bool inName)
{
    List<T> result = new List<T>();
    for (int i = 0; i < list.Count; i++)
        if (HasTag(list[i], tag, inName)) result.Add(list[i]);
    return result;
}

bool HasTag<T>(T block, String tag, bool inName)
{
    IMyTerminalBlock b = (IMyTerminalBlock)block;
    if (inName && b.CustomName.ToUpper().Contains(tag.ToUpper())) return true;
    if (!inName && b.BlockDefinition.ToString().ToUpper().Contains(tag.ToUpper())) return true;
    return false;
}


// ========================== BLOCK CONTROL HELPERS ==========================

void SetBlocksEnabled<T>(List<T> list, bool enable)
{
    for (int i = 0; i < list.Count; i++)
        SetBlockEnabled((IMyTerminalBlock)list[i], enable);
}

void SetBlockEnabled(IMyTerminalBlock block, bool enable)
{
    if (block == null) return;
    String action = enable ? "OnOff_On" : "OnOff_Off";
    var a = block.GetActionWithName(action);
    a.Apply(block);
}

void SetSortersEnabled(bool enable)
{
    sorterState = enable;
    if (!toggleSortersEnabled) return;
    SetBlocksEnabled(sorters, enable);
}

void SetBatteryChargeMode(ChargeMode mode)
{
    for (int i = 0; i < batteries.Count; i++) batteries[i].ChargeMode = mode;
}

void SetHydrogenStockpile(bool stockpile)
{
    for (int i = 0; i < hydrogenTanks.Count; i++) hydrogenTanks[i].Stockpile = stockpile;
}

void SetLandingGears(List<IMyLandingGear> gears, bool lockState)
{
    for (int i = 0; i < gears.Count; i++)
    {
        if (lockState) gears[i].Lock();
        if (!lockState) gears[i].Unlock();
    }
}

void SetDampeners(bool enable)
{
    if (dampenerState == enable) return;
    List<IMyShipController> controllers = new List<IMyShipController>();
    gridTerminalSystem.GetBlocksOfType(controllers, IsOnSameGrid);
    if (controllers.Count == 0) return;
    for (int i = 0; i < controllers.Count; i++)
        controllers[i].DampenersOverride = enable;
    dampenerState = enable;
}

IMyShipConnector FindConnector(MyShipConnectorStatus status)
{
    for (int i = 0; i < connectors.Count; i++)
    {
        if (!IsBlockValid(connectors[i], true)) continue;
        if (connectors[i].Status == status) return connectors[i];
    }
    return null;
}

void UpdateSensor(bool showOnHUD, bool detect, float frontRange, float backRange)
{
    if (sensor == null || workTools.Count == 0) return;

    Vector3 center = new Vector3();
    int count = 0;
    for (int i = 0; i < workTools.Count; i++)
    {
        if (workTools[i].WorldMatrix.Forward != referenceWorkTool.WorldMatrix.Forward) continue;
        count++;
        center += workTools[i].GetPosition();
    }
    center = center / count;

    Vector3 offset = GetLocalPosition(sensor, center);
    sensor.Enabled = true;
    sensor.ShowOnHUD = showOnHUD;
    sensor.LeftExtend = (detect ? 1 : configWidth) / 2f * toolWidth - offset.X;
    sensor.RightExtend = (detect ? 1 : configWidth) / 2f * toolWidth + offset.X;
    sensor.TopExtend = (detect ? 1 : configHeight) / 2f * toolHeight + offset.Y;
    sensor.BottomExtend = (detect ? 1 : configHeight) / 2f * toolHeight - offset.Y;
    sensor.FrontExtend = (showOnHUD ? configDepth : frontRange) - offset.Z;
    sensor.BackExtend = showOnHUD ? 0 : backRange + shipDiameter * 0.75f + offset.Z;
    sensor.DetectFloatingObjects = true;
    sensor.DetectAsteroids = false;
    sensor.DetectLargeShips = true;
    sensor.DetectSmallShips = true;
    sensor.DetectStations = true;
    sensor.DetectOwner = true;
    sensor.DetectSubgrids = false;
    sensor.DetectPlayers = false;
    sensor.DetectEnemy = true;
    sensor.DetectFriendly = true;
    sensor.DetectNeutral = true;
}

void SetLCDFormat<T>(List<T> list, bool setFont, float fontSize, bool isSurface)
{
    for (int i = 0; i < list.Count; i++)
    {
        IMyTextSurface surface = null;
        if (list[i] is IMyTextSurface) surface = (IMyTextSurface)list[i];
        if (surface != null)
        {
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            if (!setFont) continue;
            surface.Font = "Debug";
            if (isSurface) continue;
            surface.FontSize = fontSize;
        }
    }
}

bool IsNearPlanet()
{
    Vector3D pos;
    return this.remoteControl.TryGetPlanetPosition(out pos);
}

int GetPathDirection(Vector3 targetPos, int startIndex)
{
    if (startIndex == -1) return 0;
    double closest = -1;
    int closestIdx = -1;
    for (int i = waypoints.Count - 1; i >= 0; i--)
    {
        double dist = Vector3.Distance(waypoints[i].position, targetPos);
        if (closest == -1 || dist < closest) { closestIdx = i; closest = dist; }
    }
    return Math.Sign(closestIdx - startIndex);
}

bool IsOnPath(Vector3 pos)
{
    List<Vector3> points = new List<Vector3>();
    for (int i = 0; i < waypoints.Count; i++) points.Add(waypoints[i].position);

    if (homeDock.isSet && waypoints.Count >= 1)
    {
        Vector3 approachPos = new Vector3();
        GetDockApproachPosition(homeDock, dockDist * shipDiameter, false, out approachPos);
        if (Vector3.Distance(homeDock.position, waypoints.First().position) < Vector3.Distance(homeDock.position, waypoints.Last().position))
        { points.Insert(0, approachPos); points.Insert(0, homeDock.position); }
        else { points.Add(approachPos); points.Add(homeDock.position); }
    }

    if (shipMode == ShipMode.Shuttle)
    {
        if (jobPosition.isSet && waypoints.Count >= 1)
        {
            Vector3 approachPos2 = new Vector3();
            GetDockApproachPosition(jobPosition, dockDist * shipDiameter, false, out approachPos2);
            if (Vector3.Distance(jobPosition.position, waypoints.First().position) < Vector3.Distance(jobPosition.position, waypoints.Last().position))
            { points.Insert(0, approachPos2); points.Insert(0, jobPosition.position); }
            else { points.Add(approachPos2); points.Add(jobPosition.position); }
        }
    }
    else
    {
        if (jobState != JobState.NoJob)
            if (waypoints.Count > 0 && Vector3.Distance(jobPosition.position, waypoints.First().position) < Vector3.Distance(jobPosition.position, waypoints.Last().position))
                points.Insert(0, jobPosition.position);
            else points.Add(jobPosition.position);
    }

    int closestIdx = -1;
    double closestDist = -1;
    for (int i = 0; i < points.Count; i++)
    {
        double dist = Vector3.Distance(points[i], pos);
        if (dist < closestDist || closestDist == -1) { closestDist = dist; closestIdx = i; }
    }

    if (points.Count == 0) return false;
    double distToClosest = Vector3.Distance(points[closestIdx], pos);
    double prevSpacing = Vector3.Distance(points[Math.Max(0, closestIdx - 1)], points[closestIdx]) * 1.5f;
    double nextSpacing = Vector3.Distance(points[Math.Min(points.Count - 1, closestIdx + 1)], points[closestIdx]) * 1.5f;
    return distToClosest < prevSpacing || distToClosest < nextSpacing;
}

// ========================== 23_MathAndSerial.cs ==========================
// ========================== MATH UTILITIES ==========================

static float SafeDiv(float a, float b) { if (b == 0) return 0; return a / b; }

Vector3 RoundVector(Vector3 v, int decimals)
{
    return new Vector3(Math.Round(v.X, decimals), Math.Round(v.Y, decimals), Math.Round(v.Z, decimals));
}

float AngleBetween(Vector3 a, Vector3 b)
{
    if (a == b) return 0;
    float dot = (a * b).Sum;
    float lenA = a.Length();
    float lenB = b.Length();
    if (lenA == 0 || lenB == 0) return 0;
    float result = (float)((180.0f / Math.PI) * Math.Acos(dot / (lenA * lenB)));
    if (float.IsNaN(result)) return 0;
    return result;
}

float AngleWithSign(Vector3 v, Vector3 reference, float signValue)
{
    float angle = AngleBetween(v, reference);
    if (signValue > 0f) angle *= -1;
    if (angle > -90f) return angle - 90f;
    else return 180f - (-angle - 90f);
}

double DegToRad(float degrees) { return (Math.PI / 180) * degrees; }

bool InRange(double min, double value, double max) { return (value >= min && value <= max); }

Vector3 TransformToWorld(IMyTerminalBlock block, Vector3 localPos)
{
    return Vector3D.Transform(localPos, block.WorldMatrix);
}

Vector3 GetLocalPosition(IMyTerminalBlock block, Vector3 worldPos)
{
    return LocalTransformDirection(block, worldPos - block.GetPosition());
}

Vector3 LocalTransformDirection(IMyTerminalBlock block, Vector3 worldDir)
{
    return Vector3D.TransformNormal(worldDir, MatrixD.Transpose(block.WorldMatrix));
}

Vector3 TransformToLocal(Vector3 forward, Vector3 up, Vector3 worldDir)
{
    MatrixD matrix = MatrixD.CreateFromDir(forward, up);
    return Vector3D.TransformNormal(worldDir, MatrixD.Transpose(matrix));
}

Vector3 TransformLocalToWorld(Vector3 forward, Vector3 up, Vector3 localPos)
{
    MatrixD matrix = MatrixD.CreateFromDir(forward, up);
    return Vector3D.Transform(localPos, matrix);
}


// ========================== SERIALIZATION ==========================

String VectorToString(Vector3 v) { return "" + v.X + "|" + v.Y + "|" + v.Z; }

Vector3 StringToVector(String s)
{
    String[] parts = s.Split('|');
    return new Vector3(float.Parse(SafeElement(parts, 0)), float.Parse(SafeElement(parts, 1)), float.Parse(SafeElement(parts, 2)));
}

String WaypointToString(WaypointInfo wp)
{
    String sep = ":";
    String result = VectorToString(wp.position) + sep + VectorToString(wp.forwardDir) + sep +
        VectorToString(wp.downDir) + sep + VectorToString(wp.leftDir) + sep + VectorToString(wp.gravity);
    for (int i = 0; i < wp.thrusterEfficiency.Length; i++)
        result += sep + Math.Round(wp.thrusterEfficiency[i], 3);
    return result;
}

WaypointInfo StringToWaypoint(String s)
{
    String[] parts = s.Split(':');
    WaypointInfo wp = new WaypointInfo(
        StringToVector(SafeElement(parts, 0)), StringToVector(SafeElement(parts, 1)),
        StringToVector(SafeElement(parts, 2)), StringToVector(SafeElement(parts, 3)),
        StringToVector(SafeElement(parts, 4)));
    int idx = 5;
    List<float> effList = new List<float>();
    while (idx < parts.Length)
    {
        String val = SafeElement(parts, idx);
        float f = 0;
        if (!float.TryParse(val, out f)) break;
        effList.Add(f);
        idx++;
    }
    wp.thrusterEfficiency = effList.ToArray();
    return wp;
}

void AppendStorage<T>(T value, bool newLine) { if (newLine) Storage += "\n"; Storage += value; }
void AppendStorage<T>(T value) { AppendStorage(value, true); }

String SafeElement(String[] arr, int idx)
{
    String val = arr.ElementAtOrDefault(idx);
    if (String.IsNullOrEmpty(val)) return "";
    return val;
}

// ========================== 24_Persistence.cs ==========================
// ========================== SAVE / LOAD ==========================

void Save()
{
    if (resetRequested || shipMode == ShipMode.Controller) { Storage = ""; return; }
    Storage = DATAREV + ";";
    AppendStorage(VectorToString(referencePosition), false);
    AppendStorage(VectorToString(homeDock.forwardDir));
    AppendStorage(VectorToString(homeDock.leftDir));
    AppendStorage(VectorToString(homeDock.downDir));
    AppendStorage(VectorToString(homeDock.gravity));
    AppendStorage(VectorToString(homeDock.position));
    AppendStorage(VectorToString(homeDock.connectorGridPos));
    AppendStorage(homeDock.isSet);
    AppendStorage(VectorToString(jobPosition.position));
    AppendStorage(VectorToString(jobPosition.gravity));
    AppendStorage(VectorToString(jobPosition.forwardDir));
    AppendStorage(VectorToString(jobPosition.downDir));
    AppendStorage(VectorToString(jobPosition.leftDir));
    AppendStorage(VectorToString(jobPosition.connectorGridPos));
    AppendStorage(jobPosition.isSet);
    AppendStorage(VectorToString(miningForward));
    AppendStorage(VectorToString(miningDown));
    AppendStorage(";");
    AppendStorage((int)shipMode, false);
    AppendStorage((int)jobState); AppendStorage((int)lastJobState);
    AppendStorage(maxLoadPercent); AppendStorage(minBatteryPercent); AppendStorage(minUraniumKg);
    AppendStorage(minHydrogenPercent); AppendStorage(accelerationFactor);
    AppendStorage(weightLimitEnabled); AppendStorage(unloadIce);

    if (shipMode == ShipMode.Shuttle)
    {
        AppendStorage((int)undockConfig1.trigger); AppendStorage(undockConfig1.delay);
        AppendStorage(undockConfig1.elapsedDelay); AppendStorage(undockConfig1.dockTimerName); AppendStorage(undockConfig1.leaveTimerName);
        AppendStorage((int)undockConfig2.trigger); AppendStorage(undockConfig2.delay);
        AppendStorage(undockConfig2.elapsedDelay); AppendStorage(undockConfig2.dockTimerName); AppendStorage(undockConfig2.leaveTimerName);
    }
    else
    {
        AppendStorage((int)startPosition); AppendStorage((int)onDamageBehavior); AppendStorage((int)ejectionMode);
        AppendStorage((int)depthMode); AppendStorage((int)jobStartPos); AppendStorage(whenDoneReturnHome);
        AppendStorage(toggleSortersEnabled); AppendStorage(invBalancingEnabled); AppendStorage(enableDrillsBothWays);
        AppendStorage(configWidth); AppendStorage(configHeight); AppendStorage(configDepth);
        AppendStorage(workSpeedForward); AppendStorage(workSpeedBackward);
        AppendStorage(widthOverlap); AppendStorage(heightOverlap);
        AppendStorage(jobWidth); AppendStorage(jobHeight);
        AppendStorage(holeCol); AppendStorage(holeRow);
        AppendStorage(lastHoleIndex); AppendStorage(currentHoleIndex);
        AppendStorage(runtimeMaxDepth); AppendStorage(maxMineDepth);
    }
    AppendStorage(";");
    for (int i = 0; i < thrusterTypeNames.Count; i++)
        AppendStorage((i > 0 ? "|" : "") + thrusterTypeNames[i], false);
    AppendStorage(";");
    for (int i = 0; i < waypoints.Count; i++)
        AppendStorage(WaypointToString(waypoints[i]), i > 0);
}

DataResult RestoreData()
{
    if (Storage == "") return DataResult.NoData;
    String[] sections = Storage.Split(';');
    if (SafeElement(sections, 0) != DATAREV) return DataResult.NewVersion;

    int idx = 0;
    try
    {
        String[] lines = SafeElement(sections, 1).Split('\n');
        referencePosition = StringToVector(SafeElement(lines, idx++));
        homeDock.forwardDir = StringToVector(SafeElement(lines, idx++));
        homeDock.leftDir = StringToVector(SafeElement(lines, idx++));
        homeDock.downDir = StringToVector(SafeElement(lines, idx++));
        homeDock.gravity = StringToVector(SafeElement(lines, idx++));
        homeDock.position = StringToVector(SafeElement(lines, idx++));
        homeDock.connectorGridPos = StringToVector(SafeElement(lines, idx++));
        homeDock.isSet = bool.Parse(SafeElement(lines, idx++));
        jobPosition.position = StringToVector(SafeElement(lines, idx++));
        jobPosition.gravity = StringToVector(SafeElement(lines, idx++));
        jobPosition.forwardDir = StringToVector(SafeElement(lines, idx++));
        jobPosition.downDir = StringToVector(SafeElement(lines, idx++));
        jobPosition.leftDir = StringToVector(SafeElement(lines, idx++));
        jobPosition.connectorGridPos = StringToVector(SafeElement(lines, idx++));
        jobPosition.isSet = bool.Parse(SafeElement(lines, idx++));
        miningForward = StringToVector(SafeElement(lines, idx++));
        miningDown = StringToVector(SafeElement(lines, idx++));

        lines = SafeElement(sections, 2).Split('\n'); idx = 0;
        shipMode = (ShipMode)int.Parse(SafeElement(lines, idx++));
        jobState = (JobState)int.Parse(SafeElement(lines, idx++));
        lastJobState = (JobState)int.Parse(SafeElement(lines, idx++));
        maxLoadPercent = int.Parse(SafeElement(lines, idx++));
        minBatteryPercent = int.Parse(SafeElement(lines, idx++));
        minUraniumKg = int.Parse(SafeElement(lines, idx++));
        minHydrogenPercent = int.Parse(SafeElement(lines, idx++));
        accelerationFactor = float.Parse(SafeElement(lines, idx++));
        weightLimitEnabled = bool.Parse(SafeElement(lines, idx++));
        unloadIce = bool.Parse(SafeElement(lines, idx++));

        if (shipMode == ShipMode.Shuttle)
        {
            undockConfig1.trigger = (UndockTrigger)int.Parse(SafeElement(lines, idx++));
            undockConfig1.delay = float.Parse(SafeElement(lines, idx++));
            undockConfig1.elapsedDelay = float.Parse(SafeElement(lines, idx++));
            undockConfig1.dockTimerName = SafeElement(lines, idx++);
            undockConfig1.leaveTimerName = SafeElement(lines, idx++);
            undockConfig2.trigger = (UndockTrigger)int.Parse(SafeElement(lines, idx++));
            undockConfig2.delay = float.Parse(SafeElement(lines, idx++));
            undockConfig2.elapsedDelay = float.Parse(SafeElement(lines, idx++));
            undockConfig2.dockTimerName = SafeElement(lines, idx++);
            undockConfig2.leaveTimerName = SafeElement(lines, idx++);
        }
        else
        {
            startPosition = (StartPosition)int.Parse(SafeElement(lines, idx++));
            onDamageBehavior = (DamageBehavior)int.Parse(SafeElement(lines, idx++));
            ejectionMode = (EjectionMode)int.Parse(SafeElement(lines, idx++));
            depthMode = (DepthMode)int.Parse(SafeElement(lines, idx++));
            jobStartPos = (StartPosition)int.Parse(SafeElement(lines, idx++));
            whenDoneReturnHome = bool.Parse(SafeElement(lines, idx++));
            toggleSortersEnabled = bool.Parse(SafeElement(lines, idx++));
            invBalancingEnabled = bool.Parse(SafeElement(lines, idx++));
            enableDrillsBothWays = bool.Parse(SafeElement(lines, idx++));
            configWidth = int.Parse(SafeElement(lines, idx++));
            configHeight = int.Parse(SafeElement(lines, idx++));
            configDepth = int.Parse(SafeElement(lines, idx++));
            workSpeedForward = float.Parse(SafeElement(lines, idx++));
            workSpeedBackward = float.Parse(SafeElement(lines, idx++));
            widthOverlap = float.Parse(SafeElement(lines, idx++));
            heightOverlap = float.Parse(SafeElement(lines, idx++));
            jobWidth = int.Parse(SafeElement(lines, idx++));
            jobHeight = int.Parse(SafeElement(lines, idx++));
            holeCol = int.Parse(SafeElement(lines, idx++));
            holeRow = int.Parse(SafeElement(lines, idx++));
            lastHoleIndex = int.Parse(SafeElement(lines, idx++));
            currentHoleIndex = int.Parse(SafeElement(lines, idx++));
            runtimeMaxDepth = int.Parse(SafeElement(lines, idx++));
            maxMineDepth = float.Parse(SafeElement(lines, idx++));
        }

        lines = SafeElement(sections, 3).Replace("\n", "").Split('|');
        thrusterTypeNames = lines.ToList();

        lines = SafeElement(sections, 4).Split('\n');
        waypoints.Clear();
        if (lines.Count() >= 1 && lines[0] != "")
            for (int i = 0; i < lines.Length; i++)
                waypoints.Add(StringToWaypoint(SafeElement(lines, i)));
    }
    catch { return DataResult.Failed; }

    savedJobState = lastJobState;
    StopAll();
    return DataResult.Success;
}

// ========================== 25_Display.cs ==========================
// ========================== DISPLAY ==========================

void SetupDebugPanels()
{
    debugPanels.Clear();
    for (int i = lcdPanels.Count - 1; i >= 0; i--)
    {
        String data = lcdPanels[i].CustomData.ToUpper();
        bool isSpecial = false;
        if (data == "INSTRUCTIONS") { isSpecial = true; debugMode = true; }
        if (data == "DEBUG") isSpecial = true;
        if (isSpecial) { debugPanels.Add(lcdPanels[i]); lcdPanels.RemoveAt(i); }
    }
    SetLCDFormat(debugPanels, false, 1, false);
}

void SetupTaggedSurfaces(List<IMyTerminalBlock> blocks)
{
    textSurfaces.Clear();
    for (int i = 0; i < blocks.Count; i++)
    {
        IMyTerminalBlock block = blocks[i];
        try
        {
            String prefix = pamTag.Substring(0, pamTag.Length - 1) + ":";
            int idx = block.CustomName.IndexOf(prefix);
            int surfaceIdx = -1;
            if (idx < 0 || !int.TryParse(block.CustomName.Substring(idx + prefix.Length, 1), out surfaceIdx)) continue;
            if (surfaceIdx == -1) continue;
            surfaceIdx--;
            IMyTextSurfaceProvider provider = (IMyTextSurfaceProvider)block;
            if (surfaceIdx < provider.SurfaceCount && surfaceIdx >= 0)
                textSurfaces.Add(provider.GetSurface(surfaceIdx));
        }
        catch { }
    }
}

String menuCacheText = "";
void PrintDisplays()
{
    String mainText = "";
    String echoText = "";
    menuCacheText = BuildMenuScreen(false);
    mainText += menuCacheText;
    echoText += menuCacheText;
    echoText += GetHelpText();

    for (int i = 0; i < lcdPanels.Count; i++) lcdPanels[i].WriteText(mainText);
    for (int i = 0; i < textSurfaces.Count; i++) textSurfaces[i].WriteText(mainText);
    Echo(echoText);

    for (int i = 0; i < debugPanels.Count; i++)
    {
        IMyTextPanel panel = debugPanels[i];
        String data = panel.CustomData.ToUpper();
        if (data == "DEBUG") panel.WriteText("" + "\n" + "");
        if (data == "INSTRUCTIONS") panel.WriteText(GetInstructionInfo());
    }
}

string GetInstructionInfo()
{
    String text = "";
    try
    {
        float maxInst = Runtime.MaxInstructionCount;
        text += "Inst: " + Runtime.CurrentInstructionCount + " Time: " + Math.Round(Runtime.LastRunTimeMs, 3) + "\n";
        text += "Inst. avg/max: " + (int)(avgInstructionCount * maxInst) + " / " + (int)(maxInstructionPercent * maxInst) + "\n";
        text += "Inst. avg/max: " + Math.Round(avgInstructionCount * 100f, 1) + "% / " + Math.Round(maxInstructionPercent * 100f, 1) + "% \n";
        text += "Time avg/max: " + Math.Round(avgRuntime, 2) + "ms / " + Math.Round(maxRuntime, 2) + "ms \n";
    }
    catch { }
    for (int i = 0; i < profilingData.Count; i++)
        text += "" + profilingData.Keys.ElementAt(i) + " = " + Math.Round(profilingData.Values.ElementAt(i)[0], 2) + " / " +
            Math.Round(profilingData.Values.ElementAt(i)[1], 2) + "%\n";
    return text;
}

void HandleError()
{
    String text = "Error occurred! \nPlease copy this and paste it \nin the \"Bugs and issues\" discussion.\n" +
        "Version: " + VERSION + "\n";
    SetLCDFormat(lcdPanels, setLCDFontAndSize, 0.9f, false);
    SetLCDFormat(textSurfaces, setLCDFontAndSize, 0.9f, true);
    for (int i = 0; i < lcdPanels.Count; i++) lcdPanels[i].WriteText(text + lastError.ToString());
    for (int i = 0; i < textSurfaces.Count; i++) textSurfaces[i].WriteText(text + lastError.ToString());
}


// ========================== PROFILING ==========================

void StartProfile()
{
    if (!debugMode) return;
    try { profilingStart = Runtime.CurrentInstructionCount; } catch { }
}

void EndProfile(String name)
{
    if (!debugMode) return;
    if (profilingStart == 0) return;
    try
    {
        float usage = (Runtime.CurrentInstructionCount - profilingStart) / Runtime.MaxInstructionCount * 100;
        if (!profilingData.ContainsKey(name))
            profilingData.Add(name, new float[] { usage, usage });
        else { profilingData[name][0] = usage; profilingData[name][1] = Math.Max(usage, profilingData[name][1]); }
    }
    catch { }
}

// ========================== 26_Broadcast.cs ==========================
// ========================== BROADCAST ==========================

const String BROADCAST_TAG = "[PAMCMD]";
const String BROADCAST_SELF_ID = "#";
const Char BROADCAST_SEP = ';';

void BroadcastShipState(String extra)
{
    if (!broadcastEnabled) return;
    String sep = "" + BROADCAST_SEP;
    String msg = VERSION + sep;
    msg += shipName + sep;
    msg += (int)shipMode + sep;
    msg += FormatFloat(shipPosition.X) + "" + sep;
    msg += FormatFloat(shipPosition.Y) + sep;
    msg += FormatFloat(shipPosition.Z) + sep;
    if (shipMode != ShipMode.Shuttle)
        msg += GetJobStateName(lastJobState) + (lastJobState == JobState.Active ? " " + Math.Round(jobProgress, 1) + "%" : "") + sep;
    else msg += GetJobStateName(lastJobState) + sep;
    msg += statusMessage + sep;
    msg += menuIndex + "" + sep;
    msg += menuCacheText + sep;
    msg += currentVolume + sep;
    msg += maxVolume + sep;
    for (int i = 0; i < itemList.Count; i++)
    {
        if (itemList[i].location != ItemLocation.InCargo) continue;
        msg += itemList[i].name + "/" + itemList[i].type + "/" + itemList[i].amount + sep;
    }
    SendBroadcast(msg, extra);
}

void SendBroadcast(String msg, String extra) { SendBroadcastTo(msg, BROADCAST_SELF_ID, extra); }

void SendBroadcastTo(String msg, String target, String extra)
{
    try
    {
        if (!broadcastEnabled) return;
        msg = BROADCAST_TAG + BROADCAST_SEP + GetBroadcastId() + BROADCAST_SEP + target + BROADCAST_SEP + broadcastChannel + BROADCAST_SEP + extra + BROADCAST_SEP + msg;
        this.IGC.SendBroadcastMessage(BROADCAST_TAG, msg);
    }
    catch (Exception e) { lastError = e; }
}

String GetBroadcastId()
{
    if (shipMode != ShipMode.Controller) return "" + Me.GetId();
    return BROADCAST_SELF_ID;
}

bool IsInternalMessage(String msg) { return msg.StartsWith(BROADCAST_TAG); }

bool ExtractField(ref String data, out string field, Char sep)
{
    int idx = data.IndexOf(sep);
    field = "";
    if (idx < 0) return false;
    field = data.Substring(0, idx);
    data = data.Remove(0, idx + 1);
    return true;
}

bool ParseBroadcastMessage(ref String msg, out String senderId, out String extra)
{
    senderId = "";
    extra = "";
    if (!broadcastEnabled) return false;
    String tag = "";
    if (!ExtractField(ref msg, out tag, BROADCAST_SEP) || !IsInternalMessage(tag)) return false;
    if (!ExtractField(ref msg, out senderId, BROADCAST_SEP)) return false;
    if (!ExtractField(ref msg, out tag, BROADCAST_SEP) || (tag != GetBroadcastId() && (tag != "*" && shipMode != ShipMode.Controller))) return false;
    if (!ExtractField(ref msg, out tag, BROADCAST_SEP) || (tag != broadcastChannel)) return false;
    if (!ExtractField(ref msg, out extra, BROADCAST_SEP)) return false;
    return true;
}

void InitBroadcast()
{
    bool configOk = true;
    if (firstBroadcastInit)
    {
        firstBroadcastInit = false;
        if (Me.CustomData.Contains("Antenna_Name"))
        {
            statusMessage = "Update custom data";
            Me.CustomData = "";
        }
    }

    String header = (shipMode != ShipMode.Controller ? "[PAM-Ship]" : "[PAM-Controller]") + " Broadcast-settings";
    try
    {
        if (Me.CustomData.Length == 0 || Me.CustomData.Split('\n')[0] != header)
            InitBroadcastCustomData(header);

        String[] lines = Me.CustomData.Split('\n');
        broadcastEnabled = bool.Parse(ParseCustomDataValue(FindCustomDataLine(lines, "Enable_Broadcast", ref configOk)));
        bool wasError = false;
        bool disableListener = true;

        if (broadcastEnabled)
        {
            if (shipMode != ShipMode.Controller)
                shipName = ParseCustomDataValue(FindCustomDataLine(lines, "Ship_Name", ref configOk)).Replace(BROADCAST_SELF_ID, "");
            broadcastChannel = ParseCustomDataValue(FindCustomDataLine(lines, "Broadcast_Channel", ref configOk)).ToLower();
            disableListener = false;

            if (broadcastListener == null)
            {
                broadcastListener = this.IGC.RegisterBroadcastListener(BROADCAST_TAG);
                broadcastListener.SetMessageCallback("");
            }

            List<IMyRadioAntenna> antennas = new List<IMyRadioAntenna>();
            gridTerminalSystem.GetBlocksOfType(antennas);
            bool antennaReady = false;
            for (int i = 0; i < antennas.Count; i++)
                if (antennas[i].EnableBroadcasting && antennas[i].Enabled) { antennaReady = true; break; }

            if (antennas.Count == 0) statusMessage = "No Antenna found";
            else if (!antennaReady) statusMessage = "Antenna not ready";
            wasError = antennas.Count == 0 || !antennaReady;
            if (antennaError && !wasError && shipMode != ShipMode.Controller) statusMessage = "Antenna ok";
        }
        else if (shipMode == ShipMode.Controller)
            statusMessage = "Offline - Enable in PB custom data";

        antennaError = wasError;
        if (disableListener) { if (broadcastListener != null) this.IGC.DisableBroadcastListener(broadcastListener); broadcastListener = null; }
    }
    catch { configOk = false; }
    if (!configOk) { statusMessage = "Reset custom data"; InitBroadcastCustomData(header); }
}

void InitBroadcastCustomData(String header)
{
    String data = header + "\n\n" + "Enable_Broadcast=" + (shipMode == ShipMode.Controller ? "true" : "false") + " \n";
    data += shipMode != ShipMode.Controller ? "Ship_Name=Your_ship_name_here\n" : "";
    Me.CustomData = data + "Broadcast_Channel=#default";
}

String ParseCustomDataValue(String line)
{
    int idx = line.IndexOf("//");
    if (idx != -1) line = line.Substring(0, idx);
    String[] parts = line.Split('=');
    if (parts.Length <= 1) return "";
    return parts[1].Trim();
}

String FindCustomDataLine(String[] lines, String key, ref bool ok)
{
    foreach (String line in lines)
        if (line.StartsWith(key)) return line;
    ok = false;
    return "";
}

// ========================== 27_Controller.cs ==========================
// ========================== CONTROLLER MODE ==========================
// (Full controller logic for managing remote ships)

void ControllerMainLoop(string argument)
{
    if (firstRun) connectedShips = new List<ShipInfo>();

    String senderId = "";
    String extra = "";

    if (!IsInternalMessage(argument)) ProcessControllerArgument(argument);

    if (broadcastListener != null && broadcastListener.HasPendingMessage)
    {
        MyIGCMessage msg = broadcastListener.AcceptMessage();
        String data = (string)msg.Data;
        if (ParseBroadcastMessage(ref data, out senderId, out extra) && senderId != "" && senderId != BROADCAST_SELF_ID)
        {
            ShipInfo ship = FindShip(senderId, "");
            if (ship == null) { ship = new ShipInfo(senderId); connectedShips.Add(ship); }
            ship.ProcessCommand(extra, false, false, true);
            UpdateShipInfo(ship, data);
            connectedShips.Sort(delegate (ShipInfo a, ShipInfo b)
            {
                if (a.name == null && b.name == null) return 0;
                else if (a.name == null) return -1;
                else if (b.name == null) return 1;
                else return a.name.CompareTo(b.name);
            });
        }
    }

    if (isSlowTick || firstRun)
    {
        if (scanCountdown <= 0 || firstRun)
        {
            statusMessage = "";
            firstRun = false;
            ScanControllerBlocks();
            InitBroadcast();
            lastScanTime = DateTime.Now;
        }
    }

    UpdateControllerDisplays();
}

void UpdateShipInfo(ShipInfo ship, String data)
{
    ship.lastMessageTime = DateTime.Now;
    String field = "";
    ExtractField(ref data, out ship.version, BROADCAST_SEP);
    ExtractField(ref data, out ship.name, BROADCAST_SEP);
    if (ship.version != VERSION) return;
    String modeStr = "";
    String[] posStr = new string[3];
    ExtractField(ref data, out modeStr, BROADCAST_SEP);
    ExtractField(ref data, out posStr[0], BROADCAST_SEP);
    ExtractField(ref data, out posStr[1], BROADCAST_SEP);
    ExtractField(ref data, out posStr[2], BROADCAST_SEP);
    ExtractField(ref data, out ship.stateText, BROADCAST_SEP);
    ExtractField(ref data, out ship.statusMessage, BROADCAST_SEP);
    ExtractField(ref data, out field, BROADCAST_SEP);
    ExtractField(ref data, out ship.menuText, BROADCAST_SEP);
    String volStr = "", maxVolStr = "";
    ExtractField(ref data, out volStr, BROADCAST_SEP);
    ExtractField(ref data, out maxVolStr, BROADCAST_SEP);

    ship.items.Clear();
    while (true)
    {
        String itemStr;
        if (!ExtractField(ref data, out itemStr, BROADCAST_SEP)) break;
        String[] itemParts = itemStr.Split('/');
        if (itemParts.Count() < 3) continue;
        int amount = 0;
        if (!int.TryParse(itemParts[2], out amount)) continue;
        ship.items.Add(new ItemInfo(itemParts[0], itemParts[1], amount, ItemLocation.InCargo));
    }

    int modeInt = 0;
    if (!int.TryParse(field, out ship.menuIndex)) ship.menuIndex = 0;
    if (!int.TryParse(modeStr, out modeInt)) ship.shipMode = ShipMode.Unknown;
    if (!float.TryParse(posStr[0], out ship.position.X)) ship.position.X = 0;
    if (!float.TryParse(posStr[1], out ship.position.Y)) ship.position.Y = 0;
    if (!float.TryParse(posStr[2], out ship.position.Z)) ship.position.Z = 0;
    if (!float.TryParse(volStr, out ship.currentVolume)) ship.currentVolume = 0;
    if (!float.TryParse(maxVolStr, out ship.maxVolume)) ship.maxVolume = 0;
    ship.shipMode = (ShipMode)modeInt;
    ship.distance = (int)Math.Round(Vector3.Distance(Me.GetPosition(), ship.position));
    ship.totalItems = 0;
    for (int i = 0; i < ship.items.Count(); i++) ship.totalItems += ship.items[i].amount;
}

ShipInfo FindShip(String id, String name)
{
    id = id.ToUpper(); name = name.ToUpper();
    for (int i = 0; i < connectedShips.Count; i++)
    {
        if (id != "" && connectedShips[i].id.ToUpper() == id) return connectedShips[i];
        if (name != "" && connectedShips[i].name.ToUpper() == name) return connectedShips[i];
    }
    return null;
}

void SendCommandToShip(ShipInfo ship, String cmd)
{
    if (ship == null)
    {
        for (int i = 0; i < connectedShips.Count; i++)
            connectedShips[i].ProcessCommand(cmd, true, false, false);
        SendBroadcastTo(cmd, "*", cmd);
    }
    else
    {
        ship.ProcessCommand(cmd, true, false, false);
        SendBroadcastTo(cmd, ship.id, cmd);
    }
}

void ProcessControllerArgument(string argument)
{
    if (argument == "") return;
    var parts = argument.ToUpper().Split(' ');
    parts.DefaultIfEmpty("");
    var cmd = parts.ElementAtOrDefault(0);
    var arg1 = parts.ElementAtOrDefault(1);
    String invalidMsg = "Invalid argument: " + argument;

    if (currentMenu == MenuPage.ShipList)
    {
        if (selectedShip != null)
        {
            switch (cmd)
            {
                case "UP": { if (menuIndex < 2) break; if (selectedShip.menuIndex == 0) { menuIndex = 1; return; } SendCommandToShip(selectedShip, "UP"); return; }
                case "DOWN": { if (menuIndex < 1) break; if (menuIndex == 1) { SendCommandToShip(selectedShip, "MRES"); break; } SendCommandToShip(selectedShip, "DOWN"); break; }
                case "APPLY": { if (menuIndex < 2) break; SendCommandToShip(selectedShip, "APPLY"); return; }
            }
        }
    }

    switch (cmd)
    {
        case "UP": this.MenuUp(false); return;
        case "DOWN": this.MenuDown(false); return;
        case "APPLY": this.BuildControllerScreen(true); return;
    }

    switch (cmd)
    {
        case "CLEAR": ClearShipList(); return;
        case "SENDALL": SendCommandToShip(null, arg1); return;
        case "SEND": SendToNamedShip(argument.Remove(0, "SEND".Length + 1)); return;
    }
    statusMessage = invalidMsg;
}

void SendToNamedShip(String arg)
{
    if (arg == "") return;
    var parts = arg.Split(':');
    if (parts.Length != 2) { statusMessage = "Missing separator \":\""; return; }
    parts.DefaultIfEmpty("");
    String shipName = parts.ElementAtOrDefault(0).Trim();
    ShipInfo ship = FindShip("", shipName);
    if (ship != null) SendCommandToShip(ship, parts.ElementAtOrDefault(1).Trim());
    else statusMessage = "Unknown ship: " + shipName;
}

void ClearShipList() { connectedShips.Clear(); selectedShip = null; NavigateToMenu(MenuPage.Main); menuIndex = 0; }

void ScanControllerBlocks()
{
    List<IMyTerminalBlock> taggedBlocks = new List<IMyTerminalBlock>();
    gridTerminalSystem.GetBlocksOfType(lcdPanels, IsOnSameGrid);
    gridTerminalSystem.SearchBlocksOfName(pamTag.Substring(0, pamTag.Length - 1) + ":", taggedBlocks,
        q => q.CubeGrid == Me.CubeGrid && q is IMyTextSurfaceProvider);
    lcdPanels = FilterByTag(lcdPanels, pamTag, true);
    SetupTaggedSurfaces(taggedBlocks);
    SetupDebugPanels();
    SetLCDFormat(lcdPanels, setLCDFontAndSize, 1.15f, false);
    SetLCDFormat(textSurfaces, setLCDFontAndSize, 1.15f, true);
}

void UpdateControllerDisplays()
{
    String echoText = "[PAM]-Controller\n\n" + "Run-arguments: (Type without:[ ])\n" + "———————————————\n" +
        "[UP] Menu navigation up\n" + "[DOWN] Menu navigation down\n" + "[APPLY] Apply menu point\n" +
        "[CLEAR] Clear miner list\n" + "[SEND ship:cmd] Send to a ship*\n" + "[SENDALL cmd] Send to all ships*\n" +
        "———————————————\n\n" + "*[SEND] = Cmd to one ship:\n" + " e.g.: \"SEND Miner 1:homepos\"\n\n" +
        "*[SENDALL] = Cmd to all ships:\n" + " e.g.: \"SENDALL homepos\"\n\n";

    for (int i = 0; i < textSurfaces.Count; i++)
        textSurfaces[i].WriteText(BuildControllerScreenForMode(LCDMode.MainPagedCompact, "0", false));
    for (int i = 0; i < lcdPanels.Count; i++)
    {
        LCDMode mode = LCDMode.Unknown;
        String param = "";
        ParseLCDMode(lcdPanels[i], out mode, out param);
        lcdPanels[i].WriteText(BuildControllerScreenForMode(mode, param, i == 0));
    }
    Echo(echoText);
    for (int i = 0; i < debugPanels.Count; i++)
    {
        IMyTextPanel panel = debugPanels[i];
        String data = panel.CustomData.ToUpper();
        if (data == "DEBUG") panel.WriteText("" + "\n" + "");
        if (data == "INSTRUCTIONS") panel.WriteText(GetInstructionInfo());
    }
}

String BuildControllerScreen(bool applyAction)
{
    int dummy = 0;
    return BuildControllerScreen(applyAction, 0, ref dummy, false, 1);
}

String BuildControllerScreen(bool applyAction, int pageOffset, ref int scrollState, bool compact, int pageSize)
{
    // Simplified - the full implementation handles all controller menu pages
    // (ship list, ship detail, send commands, inventory view)
    return "[PAM]-Controller | " + connectedShips.Count + " Connected ships\n";
}

String BuildControllerScreenForMode(LCDMode mode, String param, bool firstPanel)
{
    // Simplified - dispatches to appropriate display mode
    return BuildControllerScreen(false);
}

void ParseLCDMode(IMyTerminalBlock panel, out LCDMode mode, out String param)
{
    bool ok = true;
    String modeStr = ParseCustomDataValue(FindCustomDataLine(panel.CustomData.Split('\n'), "mode", ref ok)).ToUpper();
    String[] parts = modeStr.Split(':');
    String modeKey = parts.Length > 0 ? parts[0].Trim() : "";
    param = parts.Length > 1 ? parts[1].Trim() : "";
    mode = LCDMode.Unknown;
    if (modeKey == "MAIN") mode = LCDMode.MainPaged;
    else if (modeKey == "MAINX") mode = LCDMode.MainPagedCompact;
    else if (modeKey == "MENU") mode = LCDMode.Menu;
    else if (modeKey == "INVENTORY") mode = LCDMode.Inventory;
    else if (modeKey == "DEBUG") mode = LCDMode.Debug;
    if (mode == LCDMode.Unknown) param = modeStr;
}

// ========================== 28_UIHelpers.cs ==========================
// ========================== UI STRING HELPERS ==========================

String ScrollText(int visibleLines, String text, int selectedLine, ref int scrollOffset)
{
    String[] lines = text.Split('\n');
    if (selectedLine >= scrollOffset + visibleLines - 1) scrollOffset++;
    scrollOffset = Math.Min(lines.Count() - 1 - visibleLines, scrollOffset);
    if (selectedLine < scrollOffset + 1) scrollOffset--;
    scrollOffset = Math.Max(0, scrollOffset);
    String result = "";
    for (int i = 0; i < visibleLines; i++)
    {
        int lineIdx = i + scrollOffset;
        if (lineIdx >= lines.Count()) break;
        result += lines[lineIdx] + "\n";
    }
    return result;
}

String PageText(int page, int pageSize, String text)
{
    String[] lines = text.Split('\n');
    int start = page * pageSize;
    int end = (page + 1) * pageSize;
    String result = "";
    for (int i = start; i < end; i++)
    {
        if (i >= lines.Count()) break;
        result += lines[i] + "\n";
    }
    return result;
}

String Capitalize(String s)
{
    if (s == "") return s;
    return s.First().ToString().ToUpper() + s.Substring(1).ToLower();
}

String Truncate(String s, int maxLen)
{
    if (s == "") return s;
    if (s.Length > maxLen) s = s.Substring(0, maxLen - 1) + ".";
    return s;
}

string ProgressBar(String label, float value, float max, int barWidth, int labelWidth, int padding)
{
    float ratio = SafeDiv(value, max) * barWidth;
    String bar = "[";
    for (int i = 0; i < barWidth; i++) { if (i <= ratio) bar += "|"; else bar += "'"; }
    bar += "]";
    return bar + " " + Truncate(Capitalize(label), labelWidth).PadRight(labelWidth) + "".PadRight(padding) + FormatAmount(value);
}

String FormatAmount(float amount)
{
    if (amount >= 1000000) return Math.Round(amount / 1000000f, amount / 1000000f < 100 ? 1 : 0) + "M";
    if (amount >= 1000) return Math.Round(amount / 1000f, amount / 1000f < 100 ? 1 : 0) + "K";
    return "" + Math.Round(amount);
}

String FormatTime(int seconds)
{
    if (seconds >= 60 * 60) return Math.Round(seconds / (60f * 60f), 1) + " h";
    if (seconds >= 60) return Math.Round(seconds / 60f, 1) + " min";
    return "" + seconds + " s";
}

string FormatFloat(float v) { return Math.Round(v, 2) + " "; }
string FormatVector(Vector3 v) { return "X" + FormatFloat(v.X) + "Y" + FormatFloat(v.Y) + "Z" + FormatFloat(v.Z); }
