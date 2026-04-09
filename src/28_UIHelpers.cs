// ========================== UI STRING HELPERS ==========================

String ScrollText(int visibleLines, String text, int selectedLine, ref int scrollOffset)
{
    String[] lines = text.Split('\n');
    if (selectedLine >= scrollOffset + visibleLines - 1) scrollOffset++;
    scrollOffset = Math.Min(lines.Count() - 1 - visibleLines, scrollOffset);
    if (selectedLine < scrollOffset + 1) scrollOffset--;
    scrollOffset = Math.Max(0, scrollOffset);
    String result = "";
    for (int i = 0; i < visibleLines; i++)
    {
        int lineIdx = i + scrollOffset;
        if (lineIdx >= lines.Count()) break;
        result += lines[lineIdx] + "\n";
    }
    return result;
}

String PageText(int page, int pageSize, String text)
{
    String[] lines = text.Split('\n');
    int start = page * pageSize;
    int end = (page + 1) * pageSize;
    String result = "";
    for (int i = start; i < end; i++)
    {
        if (i >= lines.Count()) break;
        result += lines[i] + "\n";
    }
    return result;
}

String Capitalize(String s)
{
    if (s == "") return s;
    return s.First().ToString().ToUpper() + s.Substring(1).ToLower();
}

String Truncate(String s, int maxLen)
{
    if (s == "") return s;
    if (s.Length > maxLen) s = s.Substring(0, maxLen - 1) + ".";
    return s;
}

string ProgressBar(String label, float value, float max, int barWidth, int labelWidth, int padding)
{
    float ratio = SafeDiv(value, max) * barWidth;
    String bar = "[";
    for (int i = 0; i < barWidth; i++) { if (i <= ratio) bar += "|"; else bar += "'"; }
    bar += "]";
    return bar + " " + Truncate(Capitalize(label), labelWidth).PadRight(labelWidth) + "".PadRight(padding) + FormatAmount(value);
}

String FormatAmount(float amount)
{
    if (amount >= 1000000) return Math.Round(amount / 1000000f, amount / 1000000f < 100 ? 1 : 0) + "M";
    if (amount >= 1000) return Math.Round(amount / 1000f, amount / 1000f < 100 ? 1 : 0) + "K";
    return "" + Math.Round(amount);
}

String FormatTime(int seconds)
{
    if (seconds >= 60 * 60) return Math.Round(seconds / (60f * 60f), 1) + " h";
    if (seconds >= 60) return Math.Round(seconds / 60f, 1) + " min";
    return "" + seconds + " s";
}

string FormatFloat(float v) { return Math.Round(v, 2) + " "; }
string FormatVector(Vector3 v) { return "X" + FormatFloat(v.X) + "Y" + FormatFloat(v.Y) + "Z" + FormatFloat(v.Z); }
