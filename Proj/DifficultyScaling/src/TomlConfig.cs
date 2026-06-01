using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DifficultyScaling;

public static class TomlConfig
{
    public static Dictionary<string, string> Parse(string path)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path))
            return result;

        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("["))
                continue;

            var eqIdx = line.IndexOf('=');
            if (eqIdx < 0) continue;

            var key = line[..eqIdx].Trim();
            var val = line[(eqIdx + 1)..].Trim();

            if (key.Length > 0 && val.Length > 0)
                result[key] = val;
        }
        return result;
    }

    public static float GetFloat(Dictionary<string, string> cfg, string key, float defaultValue)
    {
        if (cfg.TryGetValue(key, out var raw) &&
            float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
            return val;
        return defaultValue;
    }

    public static bool GetBool(Dictionary<string, string> cfg, string key, bool defaultValue)
    {
        if (cfg.TryGetValue(key, out var raw))
        {
            if (raw.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
            if (raw.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
        }
        return defaultValue;
    }
}
