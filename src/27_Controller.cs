// ========================== CONTROLLER MODE ==========================
// (Full controller logic for managing remote ships)

void ControllerMainLoop(string argument)
{
    if (firstRun) connectedShips = new List<ShipInfo>();

    String senderId = "";
    String extra = "";

    if (!IsInternalMessage(argument)) ProcessControllerArgument(argument);

    if (broadcastListener != null && broadcastListener.HasPendingMessage)
    {
        MyIGCMessage msg = broadcastListener.AcceptMessage();
        String data = (string)msg.Data;
        if (ParseBroadcastMessage(ref data, out senderId, out extra) && senderId != "" && senderId != BROADCAST_SELF_ID)
        {
            ShipInfo ship = FindShip(senderId, "");
            if (ship == null) { ship = new ShipInfo(senderId); connectedShips.Add(ship); }
            ship.ProcessCommand(extra, false, false, true);
            UpdateShipInfo(ship, data);
            connectedShips.Sort(delegate (ShipInfo a, ShipInfo b)
            {
                if (a.name == null && b.name == null) return 0;
                else if (a.name == null) return -1;
                else if (b.name == null) return 1;
                else return a.name.CompareTo(b.name);
            });
        }
    }

    if (isSlowTick || firstRun)
    {
        if (scanCountdown <= 0 || firstRun)
        {
            statusMessage = "";
            firstRun = false;
            ScanControllerBlocks();
            InitBroadcast();
            lastScanTime = DateTime.Now;
        }
    }

    UpdateControllerDisplays();
}

void UpdateShipInfo(ShipInfo ship, String data)
{
    ship.lastMessageTime = DateTime.Now;
    String field = "";
    ExtractField(ref data, out ship.version, BROADCAST_SEP);
    ExtractField(ref data, out ship.name, BROADCAST_SEP);
    if (ship.version != VERSION) return;
    String modeStr = "";
    String[] posStr = new string[3];
    ExtractField(ref data, out modeStr, BROADCAST_SEP);
    ExtractField(ref data, out posStr[0], BROADCAST_SEP);
    ExtractField(ref data, out posStr[1], BROADCAST_SEP);
    ExtractField(ref data, out posStr[2], BROADCAST_SEP);
    ExtractField(ref data, out ship.stateText, BROADCAST_SEP);
    ExtractField(ref data, out ship.statusMessage, BROADCAST_SEP);
    ExtractField(ref data, out field, BROADCAST_SEP);
    ExtractField(ref data, out ship.menuText, BROADCAST_SEP);
    String volStr = "", maxVolStr = "";
    ExtractField(ref data, out volStr, BROADCAST_SEP);
    ExtractField(ref data, out maxVolStr, BROADCAST_SEP);

    ship.items.Clear();
    while (true)
    {
        String itemStr;
        if (!ExtractField(ref data, out itemStr, BROADCAST_SEP)) break;
        String[] itemParts = itemStr.Split('/');
        if (itemParts.Count() < 3) continue;
        int amount = 0;
        if (!int.TryParse(itemParts[2], out amount)) continue;
        ship.items.Add(new ItemInfo(itemParts[0], itemParts[1], amount, ItemLocation.InCargo));
    }

    int modeInt = 0;
    if (!int.TryParse(field, out ship.menuIndex)) ship.menuIndex = 0;
    if (!int.TryParse(modeStr, out modeInt)) ship.shipMode = ShipMode.Unknown;
    if (!float.TryParse(posStr[0], out ship.position.X)) ship.position.X = 0;
    if (!float.TryParse(posStr[1], out ship.position.Y)) ship.position.Y = 0;
    if (!float.TryParse(posStr[2], out ship.position.Z)) ship.position.Z = 0;
    if (!float.TryParse(volStr, out ship.currentVolume)) ship.currentVolume = 0;
    if (!float.TryParse(maxVolStr, out ship.maxVolume)) ship.maxVolume = 0;
    ship.shipMode = (ShipMode)modeInt;
    ship.distance = (int)Math.Round(Vector3.Distance(Me.GetPosition(), ship.position));
    ship.totalItems = 0;
    for (int i = 0; i < ship.items.Count(); i++) ship.totalItems += ship.items[i].amount;
}

ShipInfo FindShip(String id, String name)
{
    id = id.ToUpper(); name = name.ToUpper();
    for (int i = 0; i < connectedShips.Count; i++)
    {
        if (id != "" && connectedShips[i].id.ToUpper() == id) return connectedShips[i];
        if (name != "" && connectedShips[i].name.ToUpper() == name) return connectedShips[i];
    }
    return null;
}

void SendCommandToShip(ShipInfo ship, String cmd)
{
    if (ship == null)
    {
        for (int i = 0; i < connectedShips.Count; i++)
            connectedShips[i].ProcessCommand(cmd, true, false, false);
        SendBroadcastTo(cmd, "*", cmd);
    }
    else
    {
        ship.ProcessCommand(cmd, true, false, false);
        SendBroadcastTo(cmd, ship.id, cmd);
    }
}

void ProcessControllerArgument(string argument)
{
    if (argument == "") return;
    var parts = argument.ToUpper().Split(' ');
    parts.DefaultIfEmpty("");
    var cmd = parts.ElementAtOrDefault(0);
    var arg1 = parts.ElementAtOrDefault(1);
    String invalidMsg = "Invalid argument: " + argument;

    if (currentMenu == MenuPage.ShipList)
    {
        if (selectedShip != null)
        {
            switch (cmd)
            {
                case "UP": { if (menuIndex < 2) break; if (selectedShip.menuIndex == 0) { menuIndex = 1; return; } SendCommandToShip(selectedShip, "UP"); return; }
                case "DOWN": { if (menuIndex < 1) break; if (menuIndex == 1) { SendCommandToShip(selectedShip, "MRES"); break; } SendCommandToShip(selectedShip, "DOWN"); break; }
                case "APPLY": { if (menuIndex < 2) break; SendCommandToShip(selectedShip, "APPLY"); return; }
            }
        }
    }

    switch (cmd)
    {
        case "UP": this.MenuUp(false); return;
        case "DOWN": this.MenuDown(false); return;
        case "APPLY": this.BuildControllerScreen(true); return;
    }

    switch (cmd)
    {
        case "CLEAR": ClearShipList(); return;
        case "SENDALL": SendCommandToShip(null, arg1); return;
        case "SEND": SendToNamedShip(argument.Remove(0, "SEND".Length + 1)); return;
    }
    statusMessage = invalidMsg;
}

void SendToNamedShip(String arg)
{
    if (arg == "") return;
    var parts = arg.Split(':');
    if (parts.Length != 2) { statusMessage = "Missing separator \":\""; return; }
    parts.DefaultIfEmpty("");
    String shipName = parts.ElementAtOrDefault(0).Trim();
    ShipInfo ship = FindShip("", shipName);
    if (ship != null) SendCommandToShip(ship, parts.ElementAtOrDefault(1).Trim());
    else statusMessage = "Unknown ship: " + shipName;
}

void ClearShipList() { connectedShips.Clear(); selectedShip = null; NavigateToMenu(MenuPage.Main); menuIndex = 0; }

void ScanControllerBlocks()
{
    List<IMyTerminalBlock> taggedBlocks = new List<IMyTerminalBlock>();
    gridTerminalSystem.GetBlocksOfType(lcdPanels, IsOnSameGrid);
    gridTerminalSystem.SearchBlocksOfName(pamTag.Substring(0, pamTag.Length - 1) + ":", taggedBlocks,
        q => q.CubeGrid == Me.CubeGrid && q is IMyTextSurfaceProvider);
    lcdPanels = FilterByTag(lcdPanels, pamTag, true);
    SetupTaggedSurfaces(taggedBlocks);
    SetupDebugPanels();
    SetLCDFormat(lcdPanels, setLCDFontAndSize, 1.15f, false);
    SetLCDFormat(textSurfaces, setLCDFontAndSize, 1.15f, true);
}

void UpdateControllerDisplays()
{
    String echoText = "[PAM]-Controller\n\n" + "Run-arguments: (Type without:[ ])\n" + "———————————————\n" +
        "[UP] Menu navigation up\n" + "[DOWN] Menu navigation down\n" + "[APPLY] Apply menu point\n" +
        "[CLEAR] Clear miner list\n" + "[SEND ship:cmd] Send to a ship*\n" + "[SENDALL cmd] Send to all ships*\n" +
        "———————————————\n\n" + "*[SEND] = Cmd to one ship:\n" + " e.g.: \"SEND Miner 1:homepos\"\n\n" +
        "*[SENDALL] = Cmd to all ships:\n" + " e.g.: \"SENDALL homepos\"\n\n";

    for (int i = 0; i < textSurfaces.Count; i++)
        textSurfaces[i].WriteText(BuildControllerScreenForMode(LCDMode.MainPagedCompact, "0", false));
    for (int i = 0; i < lcdPanels.Count; i++)
    {
        LCDMode mode = LCDMode.Unknown;
        String param = "";
        ParseLCDMode(lcdPanels[i], out mode, out param);
        lcdPanels[i].WriteText(BuildControllerScreenForMode(mode, param, i == 0));
    }
    Echo(echoText);
    for (int i = 0; i < debugPanels.Count; i++)
    {
        IMyTextPanel panel = debugPanels[i];
        String data = panel.CustomData.ToUpper();
        if (data == "DEBUG") panel.WriteText("" + "\n" + "");
        if (data == "INSTRUCTIONS") panel.WriteText(GetInstructionInfo());
    }
}

String BuildControllerScreen(bool applyAction)
{
    int dummy = 0;
    return BuildControllerScreen(applyAction, 0, ref dummy, false, 1);
}

String BuildControllerScreen(bool applyAction, int pageOffset, ref int scrollState, bool compact, int pageSize)
{
    // Simplified - the full implementation handles all controller menu pages
    // (ship list, ship detail, send commands, inventory view)
    return "[PAM]-Controller | " + connectedShips.Count + " Connected ships\n";
}

String BuildControllerScreenForMode(LCDMode mode, String param, bool firstPanel)
{
    // Simplified - dispatches to appropriate display mode
    return BuildControllerScreen(false);
}

void ParseLCDMode(IMyTerminalBlock panel, out LCDMode mode, out String param)
{
    bool ok = true;
    String modeStr = ParseCustomDataValue(FindCustomDataLine(panel.CustomData.Split('\n'), "mode", ref ok)).ToUpper();
    String[] parts = modeStr.Split(':');
    String modeKey = parts.Length > 0 ? parts[0].Trim() : "";
    param = parts.Length > 1 ? parts[1].Trim() : "";
    mode = LCDMode.Unknown;
    if (modeKey == "MAIN") mode = LCDMode.MainPaged;
    else if (modeKey == "MAINX") mode = LCDMode.MainPagedCompact;
    else if (modeKey == "MENU") mode = LCDMode.Menu;
    else if (modeKey == "INVENTORY") mode = LCDMode.Inventory;
    else if (modeKey == "DEBUG") mode = LCDMode.Debug;
    if (mode == LCDMode.Unknown) param = modeStr;
}
