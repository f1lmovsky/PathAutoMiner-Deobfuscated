// ========================== THRUST SYSTEM ==========================

void CalculateThrustVectors(IMyTerminalBlock reference)
{
    if (reference == null) return;
    totalThrustMap = new float[3, 2];
    thrustByType = new Dictionary<string, float[,]>();

    for (int i = 0; i < thrusters.Count; i++)
    {
        IMyThrust thruster = thrusters[i];
        if (!thruster.IsFunctional) continue;
        Vector3 dir = LocalTransformDirection(reference, thruster.WorldMatrix.Backward);
        float effective = thruster.MaxEffectiveThrust;

        if (Math.Round(dir.X, 2) != 0.0)
            if (dir.X >= 0) totalThrustMap[0, 0] += effective; else totalThrustMap[0, 1] -= effective;
        if (Math.Round(dir.Y, 2) != 0.0)
            if (dir.Y >= 0) totalThrustMap[1, 0] += effective; else totalThrustMap[1, 1] -= effective;
        if (Math.Round(dir.Z, 2) != 0.0)
            if (dir.Z >= 0) totalThrustMap[2, 0] += effective; else totalThrustMap[2, 1] -= effective;

        String typeName = GetThrusterType(thruster);
        float[,] typeMap = null;
        if (thrustByType.ContainsKey(typeName)) typeMap = thrustByType[typeName];
        else { typeMap = new float[3, 2]; thrustByType.Add(typeName, typeMap); }

        float maxThrust = thruster.MaxThrust;
        if (Math.Round(dir.X, 2) != 0.0)
            if (dir.X >= 0) typeMap[0, 0] += maxThrust; else typeMap[0, 1] -= maxThrust;
        if (Math.Round(dir.Y, 2) != 0.0)
            if (dir.Y >= 0) typeMap[1, 0] += maxThrust; else typeMap[1, 1] -= maxThrust;
        if (Math.Round(dir.Z, 2) != 0.0)
            if (dir.Z >= 0) typeMap[2, 0] += maxThrust; else typeMap[2, 1] -= maxThrust;
    }
}

static String GetThrusterType(IMyThrust thruster)
{
    return thruster.BlockDefinition.SubtypeId;
}

Vector3 LookupThrustMap(Vector3 direction, float[,] map)
{
    return new Vector3(
        direction.X >= 0 ? map[0, 0] : map[0, 1],
        direction.Y >= 0 ? map[1, 0] : map[1, 1],
        direction.Z >= 0 ? map[2, 0] : map[2, 1]);
}

bool GetThrusterEfficiency(WaypointInfo wp, String typeName, out float efficiency)
{
    efficiency = 0;
    int idx = thrusterTypeNames.IndexOf(typeName);
    if (idx == -1 || wp.thrusterEfficiency == null || idx >= wp.thrusterEfficiency.Length)
        return false;
    efficiency = wp.thrusterEfficiency[idx];
    if (efficiency == -1) return false;
    return true;
}

Vector3 GetThrustMap(Vector3 direction, WaypointInfo wp)
{
    if (wp != null)
    {
        Vector3 result = new Vector3();
        for (int i = 0; i < thrustByType.Keys.Count; i++)
        {
            String typeName = thrustByType.Keys.ElementAt(i);
            float eff = 0;
            if (!GetThrusterEfficiency(wp, typeName, out eff))
                return LookupThrustMap(direction, totalThrustMap);
            result += LookupThrustMap(direction, thrustByType.Values.ElementAt(i)) * eff;
        }
        return result;
    }
    return LookupThrustMap(direction, totalThrustMap);
}

float GetThrustForDirection(Vector3 direction, WaypointInfo wp)
{
    return GetThrustForDirection(direction, new Vector3(), wp);
}

float GetThrustForDirection(Vector3 direction, Vector3 gravity, WaypointInfo wp)
{
    Vector3 thrust = GetThrustMap(direction, wp);
    Vector3 effective = thrust + gravity * shipMass;
    float ratio = (effective / direction).AbsMin();
    return (float)(direction * ratio).Length();
}

float CalculateMaxSpeed(Vector3 localDir, Vector3 localGravity, WaypointInfo wp)
{
    if (localDir.Length() == 0) return 0;
    float alignFactor = 1;
    if (localGravity.Length() > 0) alignFactor = Math.Min(1, AngleBetween(-localGravity, localDir) / 90) * 0.4f + 0.6f;
    float thrustForce = GetThrustForDirection(localDir, localGravity, wp);
    if (thrustForce == 0) return 0.1f;
    float accel = SafeDiv(thrustForce, shipMass);
    float timeToStop = (float)Math.Sqrt(SafeDiv(localDir.Length(), accel * 0.5f));
    return accel * timeToStop * alignFactor * accelerationFactor;
}

float GravityThrustRatio(Vector3 forward, Vector3 up, Vector3 gravity, WaypointInfo wp)
{
    if (gravity.Length() == 0f) return 0;
    Vector3 localGrav = TransformToLocal(forward, up, Vector3.Normalize(gravity));
    float force = GetThrustForDirection(-localGrav, wp);
    return force / gravity.Length();
}

void CalculateMaxWeight(bool init)
{
    float weight = 0;
    float factor = 0.9f;
    if (init)
    {
        maxWeight = -1;
        waypointCalcIndex = 0;
        currentPathWaypoint = null;

        if (jobState != JobState.NoJob && jobPosition.gravity.Length() != 0)
        {
            weight = factor * GravityThrustRatio(jobPosition.forwardDir, jobPosition.downDir * -1, jobPosition.gravity, null);
            if (weight < maxWeight || maxWeight == -1) maxWeight = weight;
        }
        if (homeDock.isSet && homeDock.gravity.Length() != 0)
        {
            weight = factor * GravityThrustRatio(homeDock.forwardDir, homeDock.downDir * -1, homeDock.gravity, null);
            if (weight < maxWeight || maxWeight == -1) maxWeight = weight;
        }
        return;
    }

    // Process waypoints incrementally
    if (waypointCalcIndex == -1) return;
    if (waypointCalcIndex >= 0)
    {
        int processed = 0;
        while (waypointCalcIndex < waypoints.Count)
        {
            if (processed > 100) return;
            processed++;
            WaypointInfo wp = waypoints[waypointCalcIndex];
            if (wp.gravity.Length() != 0f)
            {
                weight = factor * Math.Min(
                    GravityThrustRatio(wp.forwardDir, wp.downDir * -1, wp.gravity, wp),
                    GravityThrustRatio(wp.forwardDir * -1, wp.downDir * -1, wp.gravity, wp));
                if (weight < maxWeight || maxWeight == -1) maxWeight = weight;
            }
            else currentPathWaypoint = wp;
            waypointCalcIndex++;
        }
        waypointCalcIndex = -1;
    }

    bool hasAtmoThrusters = true;
    float minThrust = 0;
    if (currentPathWaypoint != null)
    {
        for (int i = 0; i < thrustByType.Count; i++)
        {
            String typeName = thrustByType.Keys.ElementAt(i);
            float[,] typeMap = thrustByType.Values.ElementAt(i);
            float eff = 0;
            if (!GetThrusterEfficiency(currentPathWaypoint, typeName, out eff))
            {
                hasAtmoThrusters = false;
                break;
            }
            for (int a = 0; a < typeMap.GetLength(0); a++)
                for (int b = 0; b < typeMap.GetLength(1); b++)
                {
                    float absThrust = Math.Abs(typeMap[a, b] * eff);
                    if (absThrust == 0) continue;
                    hasAtmoThrusters = true;
                    if (minThrust == 0 || absThrust < minThrust) minThrust = absThrust;
                }
        }
    }

    if (!hasAtmoThrusters)
    {
        for (int a = 0; a < totalThrustMap.GetLength(0); a++)
            for (int b = 0; b < totalThrustMap.GetLength(1); b++)
            {
                float absThrust = Math.Abs(totalThrustMap[a, b]);
                if (absThrust == 0) continue;
                if (minThrust == 0 || absThrust < minThrust) minThrust = absThrust;
            }
    }

    if (minThrust > 0)
    {
        float minAccel = Me.CubeGrid.GridSizeEnum == MyCubeSize.Small ? minAccelerationSmall : minAccelerationLarge;
        weight = SafeDiv(minThrust, minAccel);
        if (weight > 0)
            if (weight < maxWeight || maxWeight == -1) maxWeight = weight;
    }
}

void ApplyThrustOverride(Vector3 force, bool enable)
{
    if (!enable)
    {
        for (int i = 0; i < thrusters.Count; i++)
            thrusters[i].SetValueFloat("Override", 0.0f);
        return;
    }

    Vector3 available = GetThrustMap(force, null);
    float xRatio = Math.Min(1, Math.Abs(SafeDiv(force.X, available.X)));
    float yRatio = Math.Min(1, Math.Abs(SafeDiv(force.Y, available.Y)));
    float zRatio = Math.Min(1, Math.Abs(SafeDiv(force.Z, available.Z)));

    for (int i = 0; i < thrusters.Count; i++)
    {
        IMyThrust thruster = thrusters[i];
        Vector3 dir = RoundVector(LocalTransformDirection(remoteControl, thruster.WorldMatrix.Backward), 1);
        if (dir.X != 0 && Math.Sign(dir.X) == Math.Sign(force.X))
            thruster.SetValueFloat("Override", thruster.MaxThrust * xRatio);
        else if (dir.Y != 0 && Math.Sign(dir.Y) == Math.Sign(force.Y))
            thruster.SetValueFloat("Override", thruster.MaxThrust * yRatio);
        else if (dir.Z != 0 && Math.Sign(dir.Z) == Math.Sign(force.Z))
            thruster.SetValueFloat("Override", thruster.MaxThrust * zRatio);
        else
            thruster.SetValueFloat("Override", 0.0f);
    }
}

void SetGyroOverride(bool enable, float power, float pitch, float roll, float yaw)
{
    for (int i = 0; i < gyros.Count; i++)
    {
        IMyGyro gyro = gyros[i];
        gyro.GyroOverride = enable;
        if (!enable) gyro.GyroPower = 100;
        else gyro.GyroPower = power;
        if (!enable) continue;

        Vector3 fwd = remoteControl.WorldMatrix.Forward;
        Vector3 right = remoteControl.WorldMatrix.Right;
        Vector3 up = remoteControl.WorldMatrix.Up;
        Vector3 gFwd = gyro.WorldMatrix.Forward;
        Vector3 gUp = gyro.WorldMatrix.Up;
        Vector3 gRight = gyro.WorldMatrix.Left * -1;

        if (gFwd == fwd) gyro.SetValueFloat("Roll", yaw);
        else if (gFwd == (fwd * -1)) gyro.SetValueFloat("Roll", yaw * -1);
        else if (gUp == (fwd * -1)) gyro.SetValueFloat("Yaw", yaw);
        else if (gUp == fwd) gyro.SetValueFloat("Yaw", yaw * -1);
        else if (gRight == fwd) gyro.SetValueFloat("Pitch", yaw);
        else if (gRight == (fwd * -1)) gyro.SetValueFloat("Pitch", yaw * -1);

        if (gRight == (right * -1)) gyro.SetValueFloat("Pitch", pitch);
        else if (gRight == right) gyro.SetValueFloat("Pitch", pitch * -1);
        else if (gUp == right) gyro.SetValueFloat("Yaw", pitch);
        else if (gUp == (right * -1)) gyro.SetValueFloat("Yaw", pitch * -1);
        else if (gFwd == (right * -1)) gyro.SetValueFloat("Roll", pitch);
        else if (gFwd == right) gyro.SetValueFloat("Roll", pitch * -1);

        if (gUp == (up * -1)) gyro.SetValueFloat("Yaw", roll);
        else if (gUp == up) gyro.SetValueFloat("Yaw", roll * -1);
        else if (gRight == up) gyro.SetValueFloat("Pitch", roll);
        else if (gRight == (up * -1)) gyro.SetValueFloat("Pitch", roll * -1);
        else if (gFwd == up) gyro.SetValueFloat("Roll", roll);
        else if (gFwd == (up * -1)) gyro.SetValueFloat("Roll", roll * -1);
    }
}
