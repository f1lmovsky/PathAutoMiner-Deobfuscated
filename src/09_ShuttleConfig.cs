// ========================== SHUTTLE / ALIGNMENT / CONFIG ==========================

void EnableShuttleMode()
{
    StopAll();
    shipMode = ShipMode.Shuttle;
    ShowStatusMessage(StatusMsg.Shuttle);
    jobPosition.isSet = false;
    homeDock.isSet = false;
    sensor = null;
    workTools.Clear();
    jobState = JobState.NoJob;
}

void HandleAlignment(bool enable, bool done)
{
    if (!enable) statusMessage = "Aligning canceled";
    if (done) statusMessage = "Aligning done";
    if (done || !enable)
    {
        isAligning = false;
        StopGyroOverride();
        SetGyroOverride(false, 0, 0, 0, 0);
        return;
    }
    if (IsNearPlanet()) isAligning = true;
}

bool ConfigSize(String widthStr, String heightStr, String depthStr)
{
    bool wasActive = jobState == JobState.Active;
    int w, h, d;
    if (int.TryParse(widthStr, out w) && int.TryParse(heightStr, out h) && int.TryParse(depthStr, out d))
    {
        this.StopAll();
        configWidth = w; configHeight = h; configDepth = d;
        ValidateJobConfig(false);
        UpdateJobConfig(false, false);
        if (wasActive) ContinueJob();
        return true;
    }
    return false;
}

bool ConfigWeightLimit(String arg)
{
    if (arg == "ON") { weightLimitEnabled = true; return true; }
    if (arg == "OFF") { weightLimitEnabled = false; return true; }
    return false;
}

bool ConfigBehavior(String doneArg, String damageArg)
{
    bool ok = true;
    if (doneArg == "HOME") whenDoneReturnHome = true;
    else if (doneArg == "STOP") whenDoneReturnHome = false;
    else ok = false;

    if (damageArg == "HOME") onDamageBehavior = DamageBehavior.ReturnHome;
    else if (damageArg == "STOP") onDamageBehavior = DamageBehavior.Stop;
    else if (damageArg == "JOB") onDamageBehavior = DamageBehavior.FlyToJob;
    else if (damageArg == "IG") onDamageBehavior = DamageBehavior.Ignore;
    else ok = false;
    return ok;
}
