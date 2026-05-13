using UnityEngine;

public struct AttackContext
{
    public EnemyType enemyType;
    public bool useOverrideDirection;
    public Vector2 overrideDirection;

    // Boss phases
    public ProjectileKind projectileKind;
    public MoravellePhase moravellePhase;
    public SylvarokPhase sylvarokPhase;
    public GalvanusPhase galvanusPhase;
    public SepharothPhase sepharothPhase;
    public CryotharPhase cryotharPhase;
    public VenomancerPhase venomancerPhase;
    public PyrotharPhase pyrotharPhase;
    public MoldranPhase moldranPhase;

    // Other skill flags
    public ChainLightningPhase chainLightningPhase;
    public bool isPenetrationArrow;
    public bool isTripleThreat;
    public bool isBindingArrow;
}
