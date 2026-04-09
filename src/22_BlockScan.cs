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
