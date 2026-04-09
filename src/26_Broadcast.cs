// ========================== BROADCAST ==========================

const String BROADCAST_TAG = "[PAMCMD]";
const String BROADCAST_SELF_ID = "#";
const Char BROADCAST_SEP = ';';

void BroadcastShipState(String extra)
{
    if (!broadcastEnabled) return;
    String sep = "" + BROADCAST_SEP;
    String msg = VERSION + sep;
    msg += shipName + sep;
    msg += (int)shipMode + sep;
    msg += FormatFloat(shipPosition.X) + "" + sep;
    msg += FormatFloat(shipPosition.Y) + sep;
    msg += FormatFloat(shipPosition.Z) + sep;
    if (shipMode != ShipMode.Shuttle)
        msg += GetJobStateName(lastJobState) + (lastJobState == JobState.Active ? " " + Math.Round(jobProgress, 1) + "%" : "") + sep;
    else msg += GetJobStateName(lastJobState) + sep;
    msg += statusMessage + sep;
    msg += menuIndex + "" + sep;
    msg += menuCacheText + sep;
    msg += currentVolume + sep;
    msg += maxVolume + sep;
    for (int i = 0; i < itemList.Count; i++)
    {
        if (itemList[i].location != ItemLocation.InCargo) continue;
        msg += itemList[i].name + "/" + itemList[i].type + "/" + itemList[i].amount + sep;
    }
    SendBroadcast(msg, extra);
}

void SendBroadcast(String msg, String extra) { SendBroadcastTo(msg, BROADCAST_SELF_ID, extra); }

void SendBroadcastTo(String msg, String target, String extra)
{
    try
    {
        if (!broadcastEnabled) return;
        msg = BROADCAST_TAG + BROADCAST_SEP + GetBroadcastId() + BROADCAST_SEP + target + BROADCAST_SEP + broadcastChannel + BROADCAST_SEP + extra + BROADCAST_SEP + msg;
        this.IGC.SendBroadcastMessage(BROADCAST_TAG, msg);
    }
    catch (Exception e) { lastError = e; }
}

String GetBroadcastId()
{
    if (shipMode != ShipMode.Controller) return "" + Me.GetId();
    return BROADCAST_SELF_ID;
}

bool IsInternalMessage(String msg) { return msg.StartsWith(BROADCAST_TAG); }

bool ExtractField(ref String data, out string field, Char sep)
{
    int idx = data.IndexOf(sep);
    field = "";
    if (idx < 0) return false;
    field = data.Substring(0, idx);
    data = data.Remove(0, idx + 1);
    return true;
}

bool ParseBroadcastMessage(ref String msg, out String senderId, out String extra)
{
    senderId = "";
    extra = "";
    if (!broadcastEnabled) return false;
    String tag = "";
    if (!ExtractField(ref msg, out tag, BROADCAST_SEP) || !IsInternalMessage(tag)) return false;
    if (!ExtractField(ref msg, out senderId, BROADCAST_SEP)) return false;
    if (!ExtractField(ref msg, out tag, BROADCAST_SEP) || (tag != GetBroadcastId() && (tag != "*" && shipMode != ShipMode.Controller))) return false;
    if (!ExtractField(ref msg, out tag, BROADCAST_SEP) || (tag != broadcastChannel)) return false;
    if (!ExtractField(ref msg, out extra, BROADCAST_SEP)) return false;
    return true;
}

void InitBroadcast()
{
    bool configOk = true;
    if (firstBroadcastInit)
    {
        firstBroadcastInit = false;
        if (Me.CustomData.Contains("Antenna_Name"))
        {
            statusMessage = "Update custom data";
            Me.CustomData = "";
        }
    }

    String header = (shipMode != ShipMode.Controller ? "[PAM-Ship]" : "[PAM-Controller]") + " Broadcast-settings";
    try
    {
        if (Me.CustomData.Length == 0 || Me.CustomData.Split('\n')[0] != header)
            InitBroadcastCustomData(header);

        String[] lines = Me.CustomData.Split('\n');
        broadcastEnabled = bool.Parse(ParseCustomDataValue(FindCustomDataLine(lines, "Enable_Broadcast", ref configOk)));
        bool wasError = false;
        bool disableListener = true;

        if (broadcastEnabled)
        {
            if (shipMode != ShipMode.Controller)
                shipName = ParseCustomDataValue(FindCustomDataLine(lines, "Ship_Name", ref configOk)).Replace(BROADCAST_SELF_ID, "");
            broadcastChannel = ParseCustomDataValue(FindCustomDataLine(lines, "Broadcast_Channel", ref configOk)).ToLower();
            disableListener = false;

            if (broadcastListener == null)
            {
                broadcastListener = this.IGC.RegisterBroadcastListener(BROADCAST_TAG);
                broadcastListener.SetMessageCallback("");
            }

            List<IMyRadioAntenna> antennas = new List<IMyRadioAntenna>();
            gridTerminalSystem.GetBlocksOfType(antennas);
            bool antennaReady = false;
            for (int i = 0; i < antennas.Count; i++)
                if (antennas[i].EnableBroadcasting && antennas[i].Enabled) { antennaReady = true; break; }

            if (antennas.Count == 0) statusMessage = "No Antenna found";
            else if (!antennaReady) statusMessage = "Antenna not ready";
            wasError = antennas.Count == 0 || !antennaReady;
            if (antennaError && !wasError && shipMode != ShipMode.Controller) statusMessage = "Antenna ok";
        }
        else if (shipMode == ShipMode.Controller)
            statusMessage = "Offline - Enable in PB custom data";

        antennaError = wasError;
        if (disableListener) { if (broadcastListener != null) this.IGC.DisableBroadcastListener(broadcastListener); broadcastListener = null; }
    }
    catch { configOk = false; }
    if (!configOk) { statusMessage = "Reset custom data"; InitBroadcastCustomData(header); }
}

void InitBroadcastCustomData(String header)
{
    String data = header + "\n\n" + "Enable_Broadcast=" + (shipMode == ShipMode.Controller ? "true" : "false") + " \n";
    data += shipMode != ShipMode.Controller ? "Ship_Name=Your_ship_name_here\n" : "";
    Me.CustomData = data + "Broadcast_Channel=#default";
}

String ParseCustomDataValue(String line)
{
    int idx = line.IndexOf("//");
    if (idx != -1) line = line.Substring(0, idx);
    String[] parts = line.Split('=');
    if (parts.Length <= 1) return "";
    return parts[1].Trim();
}

String FindCustomDataLine(String[] lines, String key, ref bool ok)
{
    foreach (String line in lines)
        if (line.StartsWith(key)) return line;
    ok = false;
    return "";
}
