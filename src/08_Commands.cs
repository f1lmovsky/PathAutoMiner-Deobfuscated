// ========================== ARGUMENT PROCESSING ==========================

void ProcessShipArgument(string argument)
{
    if (argument == "") return;
    var parts = argument.ToUpper().Split(' ');
    parts.DefaultIfEmpty("");
    var cmd = parts.ElementAtOrDefault(0);
    var arg1 = parts.ElementAtOrDefault(1);
    var arg2 = parts.ElementAtOrDefault(2);
    var arg3 = parts.ElementAtOrDefault(3);
    String invalidMsg = "Invalid argument: " + argument;
    bool unhandled = false;

    switch (cmd)
    {
        case "UP": this.MenuUp(false); break;
        case "DOWN": this.MenuDown(false); break;
        case "UPLOOP": this.MenuUp(true); break;
        case "DOWNLOOP": this.MenuDown(true); break;
        case "APPLY": this.BuildMenuScreen(true); break;
        case "MRES": menuIndex = 0; break;
        case "STOP": this.StopAll(); break;
        case "PATHHOME": { this.StopAll(); this.StartPathRecording(); } break;
        case "PATH": { this.StopAll(); this.StartPathRecording(); homeDock.isSet = true; } break;
        case "START": { this.StopAll(); StartNewJob(); } break;
        case "ALIGN": { HandleAlignment(!isAligning, false); } break;
        case "CONT": { this.StopAll(); this.ContinueJob(); } break;
        case "JOBPOS": { this.StopAll(); this.FlyToJobPosition(); } break;
        case "HOMEPOS": { this.StopAll(); this.FlyToHomePosition(); } break;
        case "FULL": { simulateShipFull = true; } break;
        case "RESET": { resetRequested = true; setupErrorLevel = 2; } break;
        default: unhandled = true; break;
    }

    if (shipMode != ShipMode.Shuttle)
    {
        switch (cmd)
        {
            case "SHUTTLE": { EnableShuttleMode(); } break;
            case "CFGS": { if (!ConfigSize(arg1, arg2, arg3)) statusMessage = invalidMsg; } break;
            case "CFGB": { if (!ConfigBehavior(arg1, arg2)) statusMessage = invalidMsg; } break;
            case "CFGL":
            {
                if (!ParseConfig(ref maxLoadPercent, true, ConfigType.MaxLoad, arg1, "") || !ConfigWeightLimit(arg2))
                    statusMessage = invalidMsg;
            } break;
            case "CFGE":
            {
                if (!ParseConfig(ref minUraniumKg, true, ConfigType.MinUranium, arg1, "IG") ||
                    !ParseConfig(ref minBatteryPercent, true, ConfigType.MinBattery, arg2, "IG") ||
                    !ParseConfig(ref minHydrogenPercent, true, ConfigType.MinHydrogen, arg3, "IG"))
                    statusMessage = invalidMsg;
            } break;
            case "CFGA": { if (!ParseConfig(ref accelerationFactor, false, ConfigType.Acceleration, arg1, "")) statusMessage = invalidMsg; } break;
            case "CFGW":
            {
                if (!ParseConfig(ref workSpeedForward, false, ConfigType.WorkSpeed, arg1, "") ||
                    !ParseConfig(ref workSpeedBackward, false, ConfigType.WorkSpeed, arg2, ""))
                    statusMessage = invalidMsg;
            } break;
            case "NEXT": { AdvanceHole(false); } break;
            case "PREV": { AdvanceHole(true); } break;
            default: if (unhandled) statusMessage = invalidMsg; break;
        }
    }
    else
    {
        switch (cmd)
        {
            case "UNDOCK": { undockRequested = true; } break;
            default: if (unhandled) statusMessage = invalidMsg; break;
        }
    }
}


// ========================== HELP TEXT ==========================

String GetHelpText()
{
    String text = "\n\n" + "Run-arguments: (Type without:[ ])\n" + "———————————————\n" +
        "[UP] Menu navigation up\n" + "[DOWN] Menu navigation down\n" + "[APPLY] Apply menu point\n\n" +
        "[UPLOOP] \"UP\" + looping\n" + "[DOWNLOOP] \"DOWN\" + looping\n" +
        "[PATHHOME] Record path, set home\n" + "[PATH] Record path, use old home\n" +
        "[START] Start job\n" + "[STOP] Stop every process\n" + "[CONT] Continue last job\n" +
        "[JOBPOS] Move to job position\n" + "[HOMEPOS] Move to home position\n\n" +
        "[FULL] Simulate ship is full\n" + "[ALIGN] Align the ship to planet\n" + "[RESET] Reset all data\n";

    if (shipMode != ShipMode.Shuttle)
        text += "[SHUTTLE] Enable shuttle mode\n" + "[NEXT] Next hole\n" + "[PREV] Previous hole\n\n" +
            "[CFGS width height depth]*\n" + "[CFGB done damage]*\n" + "[CFGL maxload weightLimit]*\n" +
            "[CFGE minUr minBat minHyd]*\n" + "[CFGW forward backward]*\n" + "[CFGA acceleration]*\n" +
            "———————————————\n" +
            "*[CFGS] = Config Size:\n" + " e.g.: \"CFGS 5 3 20\"\n\n" +
            "*[CFGB] = Config Behaviour:\n" + " When done: [HOME,STOP]\n" + " On Damage: [HOME,JOB,STOP,IG]\n" +
            " e.g.: \"CFGB HOME IG\"\n\n" +
            "*[CFGL] = Config max load:\n" + " maxload: [10..95]\n" + " weight limit: [On/Off]\n" +
            " e.g.: \"CFGL 70 on\"\n\n" +
            "*[CFGE] = Config energy:\n" + " minUr (Uranium): [1..25, IG]\n" + " minBat (Battery): [5..30, IG]\n" +
            " minHyd (Hydrogen): [10..90, IG]\n" + " e.g.: \"CFGE 20 10 IG\"\n\n" +
            "*[CFGW] = Config work speed:\n" + " fwd: [0.5..10]\n" + " bwd: [0.5..10]\n" +
            " e.g.: \"CFGW 1.5 2\"\n\n" +
            "*[CFGA] = Config acceleration:\n" + " acceleration: [10..100]\n" + " e.g.: \"CFGA 80\"\n";
    else
        text += "[UNDOCK] Leave current connector\n\n";

    return text;
}
