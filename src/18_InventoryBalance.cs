// ========================== INVENTORY BALANCING ==========================

void BalanceInventory()
{
    if (!invBalancingEnabled) return;
    if (workTools.Count <= 1) return;

    float totalMax = 0, totalCurrent = 0;
    for (int i = 0; i < workTools.Count; i++)
    {
        IMyTerminalBlock tool = workTools[i];
        if (IsBlockDamaged(tool, true)) continue;
        totalMax += (float)tool.GetInventory(0).MaxVolume;
        totalCurrent += (float)tool.GetInventory(0).CurrentVolume;
    }
    float avgFill = (float)Math.Round(SafeDiv(totalCurrent, totalMax), 5);

    for (int pass = 0; pass < Math.Max(1, Math.Floor(workTools.Count / 10f)); pass++)
    {
        float highestFill = 0, lowestFill = 0;
        float highestMax = 0, lowestMax = 0;
        IMyTerminalBlock fullest = null, emptiest = null;

        for (int i = 0; i < workTools.Count; i++)
        {
            IMyTerminalBlock tool = workTools[i];
            if (IsBlockDamaged(tool, true)) continue;
            float maxVol = (float)tool.GetInventory(0).MaxVolume;
            float fillRatio = SafeDiv((float)tool.GetInventory(0).CurrentVolume, maxVol);

            if (fullest == null || fillRatio > highestFill) { fullest = tool; highestFill = fillRatio; highestMax = maxVol; }
            if (emptiest == null || fillRatio < lowestFill) { emptiest = tool; lowestFill = fillRatio; lowestMax = maxVol; }
        }

        if (fullest == null || emptiest == null || fullest == emptiest) return;

        if (checkConveyorSystem && !fullest.GetInventory(0).IsConnectedTo(emptiest.GetInventory(0)))
        {
            if (balanceFailCount > 20) statusMessage = "Inventory balancing failed";
            else balanceFailCount++;
            return;
        }
        balanceFailCount = 0;

        List<MyInventoryItem> items = new List<MyInventoryItem>();
        fullest.GetInventory(0).GetItems(items);
        float volPerUnit = 0;
        if (items.Count == 0) continue;
        MyInventoryItem firstItem = items[0];
        String itemKey = firstItem.Type.TypeId + firstItem.Type.SubtypeId;

        if (!volumePerItem.TryGetValue(itemKey, out volPerUnit))
        {
            if (MeasureItemVolume(fullest.GetInventory(0), 0, emptiest.GetInventory(0), out volPerUnit))
                volumePerItem.Add(itemKey, volPerUnit);
            else return;
        }

        float toTransferFromFull = ((highestFill - avgFill) * highestMax / volPerUnit);
        float toTransferToEmpty = ((avgFill - lowestFill) * lowestMax / volPerUnit);
        int transferAmount = (int)Math.Min(toTransferToEmpty, toTransferFromFull);
        if (transferAmount <= 0) return;

        if ((float)firstItem.Amount < transferAmount)
            fullest.GetInventory(0).TransferItemTo(emptiest.GetInventory(0), 0, null, null, firstItem.Amount);
        else
            fullest.GetInventory(0).TransferItemTo(emptiest.GetInventory(0), 0, null, null, transferAmount);
    }
}

bool MeasureItemVolume(IMyInventory source, int idx, IMyInventory dest, out float volume)
{
    volume = 0;
    float beforeVol = (float)source.CurrentVolume;
    List<MyInventoryItem> beforeItems = new List<MyInventoryItem>();
    source.GetItems(beforeItems);
    float beforeCount = 0;
    for (int i = 0; i < beforeItems.Count; i++) beforeCount += (float)beforeItems[i].Amount;

    source.TransferItemTo(dest, idx, null, null, 1);
    float volDiff = beforeVol - (float)source.CurrentVolume;

    List<MyInventoryItem> afterItems = new List<MyInventoryItem>();
    source.GetItems(afterItems);
    float afterCount = 0;
    for (int i = 0; i < afterItems.Count; i++) afterCount += (float)afterItems[i].Amount;

    if (volDiff == 0f || !InRange(0.9999, beforeCount - afterCount, 1.0001))
        return false;
    volume = volDiff;
    return true;
}
