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
