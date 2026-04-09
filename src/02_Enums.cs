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
                             SendCommand, SendCommandAll, ShuttlePage1, ShuttlePage2, ShuttleBehavior }
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
