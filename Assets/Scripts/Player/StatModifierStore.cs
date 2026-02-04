using System.Collections.Generic;

public class StatModifierStore
{
    // Key by reference (e.g., the PassiveItem instance). Use string IDs if you need persistence.
    private readonly Dictionary<object, List<StatBonus>> _bySource = new();

    public void ApplySource(object sourceKey, IEnumerable<StatBonus> bonuses) => _bySource[sourceKey] = new List<StatBonus>(bonuses);

    public void RemoveSource(object sourceKey) => _bySource.Remove(sourceKey);

    public void Aggregate(out Dictionary<BoostType, float> add, out Dictionary<BoostType, float> mult, out Dictionary<BoostType, float> ovr)
    {
        add = new(); mult = new(); ovr = new();

        foreach (var kv in _bySource)
        {
            foreach (var b in kv.Value)
            {
                switch (b.op)
                {
                    case StatOp.Add:
                        add[b.boostType] = (add.TryGetValue(b.boostType, out var s) ? s : 0f) + b.value;
                        break;

                    case StatOp.Multiplier:
                        var factor = 1f + b.value; // e.g., +0.10 => *1.10
                        mult[b.boostType] = (mult.TryGetValue(b.boostType, out var m) ? m : 1f) * factor;
                        break;

                    case StatOp.Override:
                        ovr[b.boostType] = b.value; // last wins
                        break;
                }
            }
        }
    }
}