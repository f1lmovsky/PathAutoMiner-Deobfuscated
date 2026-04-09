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
