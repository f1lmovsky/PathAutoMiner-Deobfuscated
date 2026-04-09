// ========================== FLIGHT CONTROL ==========================

void UpdateFlight()
{
    Vector3 toTarget = navTargetPos - shipPosition;
    if (toTarget.Length() == 0) toTarget = new Vector3(0, 0, -1);
    Vector3 localTarget = LocalTransformDirection(remoteControl, toTarget);
    Vector3 targetDir = Vector3.Normalize(localTarget);
    Vector3 localGravity = LocalTransformDirection(remoteControl, remoteControl.GetNaturalGravity());

    float speedFactorPath = pathSpeed > 0 ? Math.Max(0, 1 - AngleBetween(toTarget, flightPathDir) / 5) : 0;
    float maxSpeed = (float)Math.Min((maxApproachSpeed > 0 ? maxApproachSpeed : 1000f), Math.Max(CalculateMaxSpeed(-localTarget, localGravity, null), pathSpeed * speedFactorPath));

    if (!thrustEnabled) maxSpeed = 0;
    if (slowOnMisalign) maxSpeed = Math.Max(0, 1 - currentAngleError / alignThreshold) * maxSpeed;
    if (generalSpeedLimit > 0) maxSpeed = Math.Min(generalSpeedLimit, maxSpeed);
    if (softApproach) maxSpeed *= (float)Math.Min(1, SafeDiv(toTarget.Length(), wpReachedDist) / 2);

    Vector3 localVelocity = LocalTransformDirection(remoteControl, remoteControl.GetShipVelocities().LinearVelocity);
    float alignmentFactor = (float)(Math.Max(0, 15 - AngleBetween(-targetDir, -localVelocity)) / 15) * 0.85f + 0.15f;
    thrustEfficiency += Math.Sign(alignmentFactor - thrustEfficiency) / 10f;

    Vector3 desiredVelocity = targetDir * maxSpeed * thrustEfficiency - (localVelocity);
    Vector3 availableThrust = GetThrustMap(desiredVelocity, null);

    if (thrustActive && distToTarget > 0.1f)
    {
        desiredVelocity.X *= SmoothThrust(desiredVelocity.X, ref thrustMultiplier.X, 1f, availableThrust.X, 20);
        desiredVelocity.Y *= SmoothThrust(desiredVelocity.Y, ref thrustMultiplier.Y, 1f, availableThrust.Y, 20);
        desiredVelocity.Z *= SmoothThrust(desiredVelocity.Z, ref thrustMultiplier.Z, 1f, availableThrust.Z, 20);
    }
    else thrustMultiplier = new Vector3(1, 1, 1);

    thrustForce = shipMass * desiredVelocity - localGravity * shipMass;
    ApplyThrustOverride(thrustForce, thrustActive);
    distToTarget = Vector3.Distance(shipPosition, navTargetPos);
}

float SmoothThrust(float value, ref float state, float step, float maxThrust, float maxMultiplier)
{
    value = Math.Sign(Math.Round(value, 2));
    if (value == Math.Sign(state)) state += Math.Sign(state) * step;
    else state = value;
    if (value == 0) state = 1;
    float result = Math.Abs(state);
    if (result < maxMultiplier || maxThrust == 0) return result;
    state = Math.Min(maxMultiplier, Math.Max(-maxMultiplier, state));
    result = Math.Abs(maxThrust);
    return result;
}

void UpdateGyros()
{
    float pitch = 90, yaw = 90, roll = 90;
    float speed = (float)(Me.CubeGrid.GridSizeEnum == MyCubeSize.Small ? gyroSpeedSmall : gyroSpeedLarge) / 100f;

    Vector3 forward, localDown, localForward, localLeft;

    if (lookAtTarget)
    {
        forward = Vector3.Normalize(navTargetPos - shipPosition);
        localForward = LocalTransformDirection(remoteControl, forward);
        localDown = LocalTransformDirection(remoteControl, targetDown);
        pitch = AngleBetween(localForward, new Vector3(0, -1, 0)) - 90;
        yaw = AngleWithSign(localDown, new Vector3(-1, 0, 0), localDown.Y);
        roll = AngleWithSign(localForward, new Vector3(-1, 0, 0), localForward.Z);
    }
    else
    {
        forward = targetForward;
        localDown = LocalTransformDirection(remoteControl, targetDown);
        localForward = LocalTransformDirection(remoteControl, targetForward);
        localLeft = LocalTransformDirection(remoteControl, targetLeft);
        pitch = AngleWithSign(localDown, new Vector3(0, 0, 1), localDown.Y);
        yaw = AngleWithSign(localDown, new Vector3(-1, 0, 0), localDown.Y);
        roll = AngleWithSign(localLeft, new Vector3(0, 0, 1), localLeft.X);
    }

    if (alignToGravity && IsNearPlanet())
    {
        Vector3 gravDir = remoteControl.GetNaturalGravity();
        localDown = LocalTransformDirection(remoteControl, gravDir);
        pitch = AngleWithSign(localDown, new Vector3(0, 0, 1), localDown.Y);
        yaw = AngleWithSign(localDown, new Vector3(-1, 0, 0), localDown.Y);
    }

    if (!InRange(-45, yaw, 45)) { pitch = 0; roll = 0; }
    if (!InRange(-45, roll, 45)) pitch = 0;

    SetGyroOverride(gyroActive, 1, (-pitch) * speed, (-roll) * speed, (-yaw) * speed);
    currentAngleError = Math.Max(Math.Abs(pitch), Math.Max(Math.Abs(yaw), Math.Abs(roll)));
    isAligned = currentAngleError <= alignThreshold;
}
