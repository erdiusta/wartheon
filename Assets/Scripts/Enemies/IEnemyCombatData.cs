public interface IEnemyCombatData
{
    public bool Isboss { get; }
    public float ElementalForgeRate { get; }
    public int ExperiencePoints { get; }
    public EnemyBehaviour EnemyBehaviour { get; }
    public EnemyCategory EnemyCategory { get; }

    // RESISTANCE
    public float PhysicalResistance { get; }
    public float MagicResistance { get; }
    public bool HasShield { get; }
    public float DeflectChance { get; }
    public float DeflectionValue { get; }

    // IMMUNITY
    public bool IsImmuneToBleeding { get; }
    public bool IsImmuneToStun { get; }
    public bool IsImmuneToSlow { get; }
    public bool IsImmuneToBurn { get; }
    public bool IsImmuneToPoison { get; }
    public bool IsImmuneToRoot { get; }
    public bool IsImmuneToFrost { get; }
    public bool IsImmuneToParalyze { get; }
    public bool IsImmuneToBlind { get; }
    public bool IsImmuneToCurse { get; }
    public bool IsImmuneToFear { get; }

    // ATTACK
    public int DealtMeleeDamageMin { get; }
    public int DealtMeleeDamageMax { get; }
    public bool CanWarm { get; }
    public float WarmChance { get; }
    public bool CanBurn { get; }
    public float BurnChance { get; }
    public bool HasBleedingDamage { get; }
    public float BleedingChance { get; }
    public bool HasSlowDamage { get; }
    public float SlowChance { get; }
    public bool IsPoisonous { get; }
    public float PoisonChance { get; }
    public bool HasAcid { get; }
    public float AcidEfficiency { get; }
    public bool HasStunDamage { get; }
    public float StunChance { get; }
    public bool HasRootDamage { get; }
    public float RootChance { get; }
    public bool HasChillDamage { get; }
    public float ChillChance { get; }
    public bool HasFrostDamage { get; }
    public float FrostChance { get; }
    public bool HasStaticDamage { get; }
    public float StaticChance { get; }
    public bool HasParalyzeDamage { get; }
    public float ParalyzeChance { get; }
    public bool HasCurseDamage { get; }
    public float CurseChance { get; }
    public bool HasFearDamage { get; }
    public float FearChance { get; }
    public bool CanDrainHealth { get; }
    public float HealthDrainChance { get; }
    public bool HasBlindDamage { get; }
    public float BlindChance { get; }
}
