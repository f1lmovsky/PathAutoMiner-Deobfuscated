// ========================== CONFIG HELPERS ==========================

void ChangeOverlap(bool isWidth, float step)
{
    StopAll();
    UpdateJobConfig(true, false);
    if (isWidth) AdjustConfig(ref widthOverlap, step, ConfigType.Overlap, false);
    else AdjustConfig(ref heightOverlap, step, ConfigType.Overlap, false);
    CalculateToolDimensions();
    UpdateSensor(true, true, 0, 0);
}

float CycleArrayValue(float current, float[] values)
{
    float result = values[0];
    for (int i = values.Length - 1; i >= 0; i--)
        if (current < values[i]) result = values[i];
    return result;
}

String NextTimerBlock(ref int index)
{
    String result = "";
    if (index >= timerBlocks.Count) index = -1;
    if (index >= 0) result = timerBlocks[index].CustomName;
    index++;
    return result;
}

void TriggerTimerBlock(string name)
{
    if (jobState != JobState.Active) return;
    if (name == "") return;
    IMyTerminalBlock block = gridTerminalSystem.GetBlockWithName(name);
    if (block == null || !(block is IMyTimerBlock))
    {
        statusMessage = "Timerblock " + name + " not found!";
        return;
    }
    ((IMyTimerBlock)block).Trigger();
}

void AdjustValue(ref int value, int bigStep, int threshold, int smallStep)
{
    if (bigStep == 0) return;
    if (value < threshold && smallStep > 0 || value <= threshold && smallStep < 0)
    {
        value += smallStep;
        return;
    }
    int absStep = Math.Abs(bigStep);
    int accumulated = 0;
    int multiplier = 1;
    while (true)
    {
        accumulated += multiplier * absStep * 10;
        if (bigStep < 0 && value - threshold <= accumulated) break;
        if (bigStep > 0 && value - threshold < accumulated) break;
        multiplier++;
    }
    value += multiplier * bigStep;
}

void ValidateJobConfig(bool updateSensor)
{
    configWidth = Math.Max(configWidth, 1);
    configHeight = Math.Max(configHeight, 1);
    configDepth = Math.Max(configDepth, 0);
    UpdateSensor(currentMenu == MenuPage.JobSetup, false, 0, 0);
}

bool ParseConfig(ref float value, bool round, ConfigType type, String input, String ignoreKeyword)
{
    if (input == "") return false;
    float parsed = -1;
    bool ignore = false;
    if (input.ToUpper() == ignoreKeyword) ignore = true;
    else if (!float.TryParse(input, out parsed)) return false;
    else parsed = Math.Max(0, parsed);
    if (round) parsed = (float)Math.Round(parsed);
    AdjustConfig(ref value, parsed, type, ignore, false);
    return true;
}

void AdjustConfig(ref float value, float step, ConfigType type, bool toggleIgnore)
{
    AdjustConfig(ref value, value + step, type, toggleIgnore, true);
}

void AdjustConfig(ref float value, float newValue, ConfigType type, bool toggleIgnore, bool wrap)
{
    float minVal = 0, maxVal = 0;
    if (type == ConfigType.WorkSpeed) { minVal = 0.5f; maxVal = 10f; }
    if (type == ConfigType.Acceleration) { minVal = 0.1f; maxVal = 1f; }
    if (type == ConfigType.Ignore) { minVal = 50f; maxVal = 100f; }
    if (type == ConfigType.MinBattery) { minVal = 5f; maxVal = 30f; }
    if (type == ConfigType.MinUranium) { minVal = 1f; maxVal = 25f; }
    if (type == ConfigType.MaxLoad) { minVal = 10f; maxVal = 95f; }
    if (type == ConfigType.MinHydrogen) { minVal = 10f; maxVal = 90f; }
    if (type == ConfigType.TimerDelay) { minVal = 10f; maxVal = 1800; }
    if (type == ConfigType.Overlap) { minVal = 0.0f; maxVal = 0.75f; }

    if (newValue == -1 && toggleIgnore) { value = -1; return; }
    if (value == -1) toggleIgnore = false;

    bool outOfRange = newValue < minVal || newValue > maxVal;
    if (outOfRange && wrap)
    {
        if (newValue < value) value = maxVal;
        else if (newValue > value) value = minVal;
    }
    else value = newValue;

    if (outOfRange && toggleIgnore) value = -1;
    else value = Math.Max(minVal, Math.Min(value, maxVal));
    value = (float)Math.Round(value, 2);
}

void AdvanceHole(bool previous)
{
    if (previous) currentHoleIndex = Math.Max(0, currentHoleIndex - 1);
    else currentHoleIndex++;
    UpdateProgress(true);
}

T CycleEnum<T>(T current)
{
    int idx = Array.IndexOf(Enum.GetValues(current.GetType()), current);
    idx++;
    if (idx >= EnumCount(current)) idx = 0;
    return (T)Enum.GetValues(current.GetType()).GetValue(idx);
}

int EnumCount<T>(T value)
{
    return Enum.GetValues(value.GetType()).Length;
}
