using System;

[Serializable]
public class WeaponModifier
{
    public string id; // stable key, e.g. "axe_crit_chance"
    public string label; // UI label, e.g. "+10% Crit Chance"
    public ModifierSource source;
    public ModifierStat stat;
    public float flat;  // use either flat OR pct depending on stat
    public float pct;
}

public static class WeaponModifierSystem
{
    // Apply one weapon’s modifiers to player.
    public static void Apply(Player p, Weapon w, Hand hand)
    {

    }
}
