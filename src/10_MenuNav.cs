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
