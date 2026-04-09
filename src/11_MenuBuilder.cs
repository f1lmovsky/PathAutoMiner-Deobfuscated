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
