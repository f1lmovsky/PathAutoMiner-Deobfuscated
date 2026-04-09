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
