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
