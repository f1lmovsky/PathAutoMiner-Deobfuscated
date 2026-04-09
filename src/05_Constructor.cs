// ========================== CONSTRUCTOR ==========================

Program()
{
    gridTerminalSystem = GridTerminalSystem;
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    if (HasTag(Me, controllerTag, true))
        shipMode = ShipMode.Controller;

    NavigateToMenu(MenuPage.Main);

    if (shipMode != ShipMode.Controller)
    {
        statusMessage = "Welcome to [PAM]!";
        DataResult restoreResult = RestoreData();

        if (shipMode == ShipMode.Unknown)
        {
            List<IMyShipDrill> drills = new List<IMyShipDrill>();
            List<IMyShipGrinder> grinders = new List<IMyShipGrinder>();
            gridTerminalSystem.GetBlocksOfType(drills, q => q.CubeGrid == Me.CubeGrid);
            gridTerminalSystem.GetBlocksOfType(grinders, q => q.CubeGrid == Me.CubeGrid);

            if (drills.Count > 0)
            {
                shipMode = ShipMode.Miner;
                statusMessage = "Miner mode enabled!";
            }
            else if (grinders.Count > 0)
            {
                shipMode = ShipMode.Grinder;
                statusMessage = "Grinder mode enabled!";
            }
            else
            {
                shipMode = ShipMode.Shuttle;
                ShowStatusMessage(StatusMsg.Shuttle);
            }
        }

        if (restoreResult == DataResult.Success) needPositionUpdate = false;
        if (restoreResult == DataResult.Failed) statusMessage = "Data restore failed!";
        if (restoreResult == DataResult.NewVersion) statusMessage = "New version, wipe data";
    }
}
