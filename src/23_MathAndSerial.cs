// ========================== MATH UTILITIES ==========================

static float SafeDiv(float a, float b) { if (b == 0) return 0; return a / b; }

Vector3 RoundVector(Vector3 v, int decimals)
{
    return new Vector3(Math.Round(v.X, decimals), Math.Round(v.Y, decimals), Math.Round(v.Z, decimals));
}

float AngleBetween(Vector3 a, Vector3 b)
{
    if (a == b) return 0;
    float dot = (a * b).Sum;
    float lenA = a.Length();
    float lenB = b.Length();
    if (lenA == 0 || lenB == 0) return 0;
    float result = (float)((180.0f / Math.PI) * Math.Acos(dot / (lenA * lenB)));
    if (float.IsNaN(result)) return 0;
    return result;
}

float AngleWithSign(Vector3 v, Vector3 reference, float signValue)
{
    float angle = AngleBetween(v, reference);
    if (signValue > 0f) angle *= -1;
    if (angle > -90f) return angle - 90f;
    else return 180f - (-angle - 90f);
}

double DegToRad(float degrees) { return (Math.PI / 180) * degrees; }

bool InRange(double min, double value, double max) { return (value >= min && value <= max); }

Vector3 TransformToWorld(IMyTerminalBlock block, Vector3 localPos)
{
    return Vector3D.Transform(localPos, block.WorldMatrix);
}

Vector3 GetLocalPosition(IMyTerminalBlock block, Vector3 worldPos)
{
    return LocalTransformDirection(block, worldPos - block.GetPosition());
}

Vector3 LocalTransformDirection(IMyTerminalBlock block, Vector3 worldDir)
{
    return Vector3D.TransformNormal(worldDir, MatrixD.Transpose(block.WorldMatrix));
}

Vector3 TransformToLocal(Vector3 forward, Vector3 up, Vector3 worldDir)
{
    MatrixD matrix = MatrixD.CreateFromDir(forward, up);
    return Vector3D.TransformNormal(worldDir, MatrixD.Transpose(matrix));
}

Vector3 TransformLocalToWorld(Vector3 forward, Vector3 up, Vector3 localPos)
{
    MatrixD matrix = MatrixD.CreateFromDir(forward, up);
    return Vector3D.Transform(localPos, matrix);
}


// ========================== SERIALIZATION ==========================

String VectorToString(Vector3 v) { return "" + v.X + "|" + v.Y + "|" + v.Z; }

Vector3 StringToVector(String s)
{
    String[] parts = s.Split('|');
    return new Vector3(float.Parse(SafeElement(parts, 0)), float.Parse(SafeElement(parts, 1)), float.Parse(SafeElement(parts, 2)));
}

String WaypointToString(WaypointInfo wp)
{
    String sep = ":";
    String result = VectorToString(wp.position) + sep + VectorToString(wp.forwardDir) + sep +
        VectorToString(wp.downDir) + sep + VectorToString(wp.leftDir) + sep + VectorToString(wp.gravity);
    for (int i = 0; i < wp.thrusterEfficiency.Length; i++)
        result += sep + Math.Round(wp.thrusterEfficiency[i], 3);
    return result;
}

WaypointInfo StringToWaypoint(String s)
{
    String[] parts = s.Split(':');
    WaypointInfo wp = new WaypointInfo(
        StringToVector(SafeElement(parts, 0)), StringToVector(SafeElement(parts, 1)),
        StringToVector(SafeElement(parts, 2)), StringToVector(SafeElement(parts, 3)),
        StringToVector(SafeElement(parts, 4)));
    int idx = 5;
    List<float> effList = new List<float>();
    while (idx < parts.Length)
    {
        String val = SafeElement(parts, idx);
        float f = 0;
        if (!float.TryParse(val, out f)) break;
        effList.Add(f);
        idx++;
    }
    wp.thrusterEfficiency = effList.ToArray();
    return wp;
}

void AppendStorage<T>(T value, bool newLine) { if (newLine) Storage += "\n"; Storage += value; }
void AppendStorage<T>(T value) { AppendStorage(value, true); }

String SafeElement(String[] arr, int idx)
{
    String val = arr.ElementAtOrDefault(idx);
    if (String.IsNullOrEmpty(val)) return "";
    return val;
}
