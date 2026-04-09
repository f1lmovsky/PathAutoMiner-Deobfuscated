// ========================== DISPLAY ==========================

void SetupDebugPanels()
{
    debugPanels.Clear();
    for (int i = lcdPanels.Count - 1; i >= 0; i--)
    {
        String data = lcdPanels[i].CustomData.ToUpper();
        bool isSpecial = false;
        if (data == "INSTRUCTIONS") { isSpecial = true; debugMode = true; }
        if (data == "DEBUG") isSpecial = true;
        if (isSpecial) { debugPanels.Add(lcdPanels[i]); lcdPanels.RemoveAt(i); }
    }
    SetLCDFormat(debugPanels, false, 1, false);
}

void SetupTaggedSurfaces(List<IMyTerminalBlock> blocks)
{
    textSurfaces.Clear();
    for (int i = 0; i < blocks.Count; i++)
    {
        IMyTerminalBlock block = blocks[i];
        try
        {
            String prefix = pamTag.Substring(0, pamTag.Length - 1) + ":";
            int idx = block.CustomName.IndexOf(prefix);
            int surfaceIdx = -1;
            if (idx < 0 || !int.TryParse(block.CustomName.Substring(idx + prefix.Length, 1), out surfaceIdx)) continue;
            if (surfaceIdx == -1) continue;
            surfaceIdx--;
            IMyTextSurfaceProvider provider = (IMyTextSurfaceProvider)block;
            if (surfaceIdx < provider.SurfaceCount && surfaceIdx >= 0)
                textSurfaces.Add(provider.GetSurface(surfaceIdx));
        }
        catch { }
    }
}

String menuCacheText = "";
void PrintDisplays()
{
    String mainText = "";
    String echoText = "";
    menuCacheText = BuildMenuScreen(false);
    mainText += menuCacheText;
    echoText += menuCacheText;
    echoText += GetHelpText();

    for (int i = 0; i < lcdPanels.Count; i++) lcdPanels[i].WriteText(mainText);
    for (int i = 0; i < textSurfaces.Count; i++) textSurfaces[i].WriteText(mainText);
    Echo(echoText);

    for (int i = 0; i < debugPanels.Count; i++)
    {
        IMyTextPanel panel = debugPanels[i];
        String data = panel.CustomData.ToUpper();
        if (data == "DEBUG") panel.WriteText("" + "\n" + "");
        if (data == "INSTRUCTIONS") panel.WriteText(GetInstructionInfo());
    }
}

string GetInstructionInfo()
{
    String text = "";
    try
    {
        float maxInst = Runtime.MaxInstructionCount;
        text += "Inst: " + Runtime.CurrentInstructionCount + " Time: " + Math.Round(Runtime.LastRunTimeMs, 3) + "\n";
        text += "Inst. avg/max: " + (int)(avgInstructionCount * maxInst) + " / " + (int)(maxInstructionPercent * maxInst) + "\n";
        text += "Inst. avg/max: " + Math.Round(avgInstructionCount * 100f, 1) + "% / " + Math.Round(maxInstructionPercent * 100f, 1) + "% \n";
        text += "Time avg/max: " + Math.Round(avgRuntime, 2) + "ms / " + Math.Round(maxRuntime, 2) + "ms \n";
    }
    catch { }
    for (int i = 0; i < profilingData.Count; i++)
        text += "" + profilingData.Keys.ElementAt(i) + " = " + Math.Round(profilingData.Values.ElementAt(i)[0], 2) + " / " +
            Math.Round(profilingData.Values.ElementAt(i)[1], 2) + "%\n";
    return text;
}

void HandleError()
{
    String text = "Error occurred! \nPlease copy this and paste it \nin the \"Bugs and issues\" discussion.\n" +
        "Version: " + VERSION + "\n";
    SetLCDFormat(lcdPanels, setLCDFontAndSize, 0.9f, false);
    SetLCDFormat(textSurfaces, setLCDFontAndSize, 0.9f, true);
    for (int i = 0; i < lcdPanels.Count; i++) lcdPanels[i].WriteText(text + lastError.ToString());
    for (int i = 0; i < textSurfaces.Count; i++) textSurfaces[i].WriteText(text + lastError.ToString());
}


// ========================== PROFILING ==========================

void StartProfile()
{
    if (!debugMode) return;
    try { profilingStart = Runtime.CurrentInstructionCount; } catch { }
}

void EndProfile(String name)
{
    if (!debugMode) return;
    if (profilingStart == 0) return;
    try
    {
        float usage = (Runtime.CurrentInstructionCount - profilingStart) / Runtime.MaxInstructionCount * 100;
        if (!profilingData.ContainsKey(name))
            profilingData.Add(name, new float[] { usage, usage });
        else { profilingData[name][0] = usage; profilingData[name][1] = Math.Max(usage, profilingData[name][1]); }
    }
    catch { }
}
