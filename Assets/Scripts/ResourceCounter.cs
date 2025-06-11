using System.Collections.Generic;

public static class ResourceCounter
{
    private static readonly Dictionary<int, int> _counts = new Dictionary<int, int>();

    public static void Add(int factionId, int amount = 1)
    {
        if (!_counts.ContainsKey(factionId))
            _counts[factionId] = 0;
        _counts[factionId] += amount;
    }

    public static int GetCountForFaction(int factionId) =>
        _counts.TryGetValue(factionId, out var v) ? v : 0;
}
