// ========================== PROGRESS / DETECTION / SPEED ==========================

void UpdateProgress(bool reset)
{
    if (reset) { jobProgress = 0; return; }
    if (shipMode == ShipMode.Shuttle) return;
    float done = currentHoleIndex * Math.Max(1, configDepth);
    if (lastHoleIndex == currentHoleIndex) done += Math.Min(configDepth, currentMineDepth);
    float total = jobWidth * jobHeight * Math.Max(1, configDepth);
    jobProgress = Math.Max(jobProgress, (float)Math.Min(done / total * 100.0, 100));
}

MyDetectedEntityType GetDetectedEntityType()
{
    try { if (IsBlockValid(sensor, true) && !sensor.LastDetectedEntity.IsEmpty()) return sensor.LastDetectedEntity.Type; }
    catch { }
    return MyDetectedEntityType.None;
}

float GetWorkSpeed(bool forward)
{
    if (shipMode == ShipMode.Grinder && GetDetectedEntityType() == MyDetectedEntityType.None && !IsBlockDamaged(sensor, true))
        return fastSpeed;
    else return forward ? workSpeedForward : workSpeedBackward;
}
