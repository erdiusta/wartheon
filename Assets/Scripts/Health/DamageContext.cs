using UnityEngine;

public struct DamageContext
{
    public DamageSourceType source;
    public DamageOwner owner;
    public MeleeHand hand;
    public Vector2 dealerPosition;
    public Vector2 receiverPosition;
    public bool bypassImmunity;
    public uint dealerNetId;
}
