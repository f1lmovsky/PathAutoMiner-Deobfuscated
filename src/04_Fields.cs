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
