// ========================== CLASSES ==========================

class WaypointInfo
{
    public bool isSet = false;
    public Vector3 position = new Vector3();
    public Vector3 downDir = new Vector3();
    public Vector3 forwardDir = new Vector3();
    public Vector3 leftDir = new Vector3();
    public Vector3 gravity = new Vector3();
    public Vector3 connectorGridPos = new Vector3();
    public float maxSpeed = 0;
    public float unused = 0;
    public float[] thrusterEfficiency = null;

    public WaypointInfo() { }

    public WaypointInfo(WaypointInfo other)
    {
        isSet = other.isSet;
        position = other.position;
        downDir = other.downDir;
        forwardDir = other.forwardDir;
        leftDir = other.leftDir;
        gravity = other.gravity;
        connectorGridPos = other.connectorGridPos;
        thrusterEfficiency = other.thrusterEfficiency;
    }

    public WaypointInfo(Vector3 position, Vector3 forwardDir, Vector3 downDir, Vector3 leftDir, Vector3 gravity)
    {
        this.position = position;
        this.downDir = downDir;
        this.forwardDir = forwardDir;
        this.leftDir = leftDir;
        this.maxSpeed = 0;
        this.gravity = gravity;
    }

    public void CalcThrusterEfficiency(List<IMyThrust> allThrusters, List<string> thrusterTypeNames)
    {
        thrusterEfficiency = new float[thrusterTypeNames.Count];
        for (int i = 0; i < thrusterEfficiency.Length; i++)
            thrusterEfficiency[i] = -1;

        for (int i = 0; i < allThrusters.Count; i++)
        {
            string typeName = GetThrusterType(allThrusters[i]);
            int idx = thrusterTypeNames.IndexOf(typeName);
            if (idx != -1)
                thrusterEfficiency[idx] = SafeDiv(allThrusters[i].MaxEffectiveThrust, allThrusters[i].MaxThrust);
        }
    }
}

class PathFollowData
{
    public WaypointInfo target = null;
    public List<Vector3> targetPositions = new List<Vector3>();
    public float directFlyDist = 0;
    public float useDockDirDist = 0;
    public float approachSpeed = 0;
    public float excludeDist = 0;
    public Vector3 excludePos = new Vector3();
}

class UndockConfig
{
    public UndockTrigger trigger = UndockTrigger.OnCommand;
    public float delay = 0;
    public float elapsedDelay = 0;
    public string dockTimerName = "";
    public string leaveTimerName = "";
    DateTime lastTimerTick;
    public bool playerWasInCockpit = false;
    private bool counting = false;

    public bool CheckDelay(bool reset)
    {
        if (reset) { elapsedDelay = 0; counting = false; return false; }
        counting = true;
        return elapsedDelay > delay;
    }

    public void Reset()
    {
        CheckDelay(true);
        playerWasInCockpit = false;
    }

    public void UpdateTimer()
    {
        if (counting)
            if ((DateTime.Now - lastTimerTick).TotalSeconds > 1)
            {
                elapsedDelay++;
                lastTimerTick = DateTime.Now;
            }
    }

    public bool HasDelay()
    {
        switch (trigger)
        {
            case UndockTrigger.OnPlayerEntered: return true;
            case UndockTrigger.OnTimerDelay: return true;
        }
        return false;
    }
}

class ItemInfo
{
    public String name = "";
    public String type = "";
    public int amount = 0;
    public ItemLocation location = ItemLocation.All;

    public ItemInfo(String name, String type, int amount, ItemLocation location)
    {
        this.name = name;
        this.type = type;
        this.amount = amount;
        this.location = location;
    }
}

class DistanceEntry
{
    public DistanceEntry(Vector3 pos, float dist)
    {
        this.pos = pos;
        this.dist = dist;
    }
    public Vector3 pos;
    public float dist;
}

class ShipInfo
{
    public DateTime lastMessageTime;
    public DateTime commandSentTime;
    public String id = "";
    public String name = "";
    public String version = "";
    public String statusMessage = "";
    public String stateText = "";
    public String menuText = "";
    public String pendingCommand = "";
    public Vector3 position = new Vector3();
    public List<ItemInfo> items = new List<ItemInfo>();
    public ShipMode shipMode = ShipMode.Unknown;
    public CommandState commandState;
    public float maxVolume;
    public float currentVolume;
    public int menuIndex;
    public int distance = 0;
    public int totalItems = 0;

    public bool IsTimedOut()
    {
        return (DateTime.Now - lastMessageTime).TotalSeconds > 10;
    }

    public bool IsCommandTimedOut()
    {
        if (commandState != CommandState.Pending) return false;
        return (DateTime.Now - commandSentTime).TotalSeconds >= 4;
    }

    public ShipInfo(String id) { this.id = id; }

    public CommandState ProcessCommand(String cmd, bool send, bool clear, bool checkReceived)
    {
        if (cmd == "" && !clear) return CommandState.None;
        if (commandState == CommandState.Pending && IsCommandTimedOut())
            commandState = CommandState.NoAnswer;

        if (send)
        {
            pendingCommand = cmd;
            commandState = CommandState.Pending;
            commandSentTime = DateTime.Now;
            return commandState;
        }
        else if (clear)
        {
            pendingCommand = "";
            commandState = CommandState.None;
        }
        else if (pendingCommand == cmd)
        {
            if (checkReceived) commandState = CommandState.Received;
            return commandState;
        }
        return CommandState.None;
    }
}


// ========================== ELEMENT CODE (user-customizable) ==========================

public String GetElementCode(String itemName)
{
    switch (itemName)
    {
        case "IRON": return "Fe";
        case "NICKEL": return "Ni";
        case "COBALT": return "Co";
        case "MAGNESIUM": return "Mg";
        case "SILICON": return "Si";
        case "SILVER": return "Ag";
        case "GOLD": return "Au";
        case "PLATINUM": return "Pt";
        case "URANIUM": return "U";
        default: return "";
    }
}
