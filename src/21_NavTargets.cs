// ========================== NAV TARGET SETTERS ==========================

void StopGyroOverride() { this.gyroActive = false; }

void AlignToDown(Vector3 down, Vector3 fwd, Vector3 left, float threshold, bool useGravity)
{
    AlignToGravity(down, threshold, useGravity);
    alignThreshold = threshold;
    lookAtTarget = false;
    this.targetForward = fwd;
    this.targetLeft = left;
}

void AlignToDown(Vector3 down, Vector3 fwd, Vector3 left, bool useGravity)
{
    AlignToDown(down, fwd, left, 2f, useGravity);
}

void AlignToGravity(Vector3 down, float threshold, bool useGravity)
{
    alignThreshold = threshold;
    this.gyroActive = true;
    this.alignToGravity = useGravity;
    lookAtTarget = true;
    isAligned = false;
    this.targetDown = down;
}

void StopFlight()
{
    SetFlightTargetFull(false, false, false, navTargetPos, 0);
    thrustActive = false;
}

void SetFlightTarget(Vector3 target, float speed)
{
    SetFlightTargetFull(true, false, false, target, speed);
}

void SetFlightTargetFull(bool enable, bool soft, bool useSegment, Vector3 target, float speed)
{
    SetFlightTargetFull(enable, soft, useSegment, target, target - shipPosition, 0.0f, speed);
}

void SetFlightTargetFull(bool enable, bool soft, bool useSegment, Vector3 target, Vector3 segDir, float segSpeed, float speed)
{
    thrustActive = true;
    this.thrustEnabled = enable;
    navTargetPos = target;
    this.maxApproachSpeed = speed;
    this.pathSpeed = segSpeed;
    this.softApproach = soft;
    this.slowOnMisalign = useSegment;
    this.flightPathDir = segDir;
    distToTarget = Vector3.Distance(target, shipPosition);
}
