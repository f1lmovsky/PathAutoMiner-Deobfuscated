// ========================== SHIP MAIN LOOP ==========================

void ShipMainLoop(string argument)
{
    bool shouldBroadcast = false;
    String broadcastExtra = "";

    if (setupErrorLevel <= 1 && !IsInternalMessage(argument))
        ProcessShipArgument(argument);

    if (broadcastListener != null && broadcastListener.HasPendingMessage)
    {
        MyIGCMessage msg = broadcastListener.AcceptMessage();
        String data = (string)msg.Data;
        String senderId = "";
        if (setupErrorLevel <= 1 && ParseBroadcastMessage(ref data, out senderId, out broadcastExtra) && senderId == BROADCAST_SELF_ID)
        {
            ProcessShipArgument(data);
            shouldBroadcast = true;
        }
    }

    bool isIdleOptimize = initialized && currentNavState == NavState.Idle && !isAligning && !shouldBroadcast && scanPhase == 0 && !isRecording;
    if (isSlowTick && currentNavState != NavState.Idle) shouldBroadcast = true;

    if ((isTick && !isIdleOptimize) || (isSlowTick && isIdleOptimize))
    {
        // Scan phase 1
        if (scanPhase == 0 && (scanCountdown <= 0 || firstRun))
        {
            wasSetupError = setupErrorLevel > 0;
            setupErrorLevel = 0;
            scanPhase = 1;
            StartProfile();
            ScanBlocks1();
            InitBroadcast();
            EndProfile("Scan 1");
        }
        // Scan phase 2
        else if (scanPhase == 1)
        {
            scanPhase = 2;
            StartProfile();
            ScanBlocks2();
            EndProfile("Scan 2");
        }
        // Scan phase 3
        else if (scanPhase == 2)
        {
            scanPhase = 0;
            StartProfile();
            ScanBlocks3();
            EndProfile("Scan 3");
            lastScanTime = DateTime.Now;

            if (setupErrorLevel <= 1 && needPositionUpdate)
                referencePosition = GetLocalPosition(remoteControl, remoteControl.CenterOfMass);
            needPositionUpdate = false;

            if (firstRun) { firstRun = false; StopAll(); }
            if (wasSetupError && setupErrorLevel == 0) statusMessage = "Setup complete";
        }
        else
        {
            // Inventory balancing
            if (jobState == JobState.Active && shipMode != ShipMode.Shuttle)
            {
                StartProfile();
                BalanceInventory();
                EndProfile("Inv balance");
            }

            // Cycled updates
            StartProfile();
            switch (updatePhase)
            {
                case 0: UpdateCargoLoad(); break;
                case 1: UpdateInventory(); break;
                case 2: UpdateBatteryState(); break;
                case 3: UpdateUranium(); break;
                case 4: UpdateHydrogen(); break;
                case 5: CheckDamage(); break;
                case 6: CalculateThrustVectors(remoteControl); break;
            }
            EndProfile("Update: " + updatePhase);

            updatePhase++;
            if (updatePhase > 6)
            {
                updatePhase = 0;
                initialized = true;

                if (savedJobState != JobState.NoJob)
                {
                    switch (savedJobState)
                    {
                        case JobState.ActiveHome: ContinueJob(); break;
                        case JobState.ActiveJob: ContinueJob(); break;
                        case JobState.Active: ContinueJob(); break;
                        case JobState.MoveHome: FlyToHomePosition(); break;
                        case JobState.MoveToJob: FlyToJobPosition(); break;
                    }
                    savedJobState = JobState.NoJob;
                }
            }
        }

        if (!firstRun)
        {
            if (!IsBlockValid(remoteControl, true))
            {
                remoteControl = null;
                needPositionUpdate = true;
                setupErrorLevel = 2;
            }

            if (setupErrorLevel >= 2 && currentNavState != NavState.Idle)
                StopAll();

            if (setupErrorLevel <= 1)
            {
                shipMass = remoteControl.CalculateShipMass().PhysicalMass;
                shipSpeed = (float)remoteControl.GetShipSpeed();
                shipPosition = TransformToWorld(remoteControl, referencePosition);
                forwardDirection = remoteControl.WorldMatrix.Forward;
                leftDirection = remoteControl.WorldMatrix.Left;
                downDirection = remoteControl.WorldMatrix.Down;
                UpdatePathRecording();

                if (currentNavState != NavState.Idle)
                {
                    isAligning = false;
                    SetDampeners(false);
                    CalculateMaxWeight(false);

                    String stateName = GetNavStateName(currentNavState) + " " + (int)currentNavState;
                    StartProfile();
                    UpdateNavigation();
                    UpdateProgress(false);
                    EndProfile(stateName);

                    StartProfile();
                    UpdateFlight();
                    EndProfile("Thruster");

                    StartProfile();
                    UpdateGyros();
                    EndProfile("Gyroscope");
                }
                else
                {
                    if (isAligning)
                    {
                        if (IsNearPlanet())
                        {
                            AlignToDown(downDirection, forwardDirection, leftDirection, 0.25f, true);
                            UpdateGyros();
                            statusMessage = "Aligning to planet: " + Math.Round(currentAngleError - 0.25f, 2) + "°";
                            if (isAligned) HandleAlignment(true, true);
                        }
                        else HandleAlignment(true, true);
                    }
                }
                undockRequested = false;
            }
        }
    }

    StartProfile();
    PrintDisplays();
    EndProfile("Print");

    if (shouldBroadcast || broadcastCountdown <= 0)
    {
        StartProfile();
        BroadcastShipState(broadcastExtra);
        EndProfile("Broadcast");
        broadcastCountdown = 4;
    }
    else if (isSlowTick) broadcastCountdown--;
}
