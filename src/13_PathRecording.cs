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
