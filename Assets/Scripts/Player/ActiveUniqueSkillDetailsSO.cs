using UnityEngine;

[CreateAssetMenu(fileName = "UniqueSkillDetails_", menuName = "Scriptable Objects/Player/Active Skill Details")]
public class ActiveUniqueSkillDetailsSO : ScriptableObject
{
    [Header("Main Info")]

    [Space(10)]
    public string activeUniqueSkillName;
    public ActiveSkill activeSkill;
    public Sprite activeUniqueSkillSprite;

    [Space(10)]
    [Header("Sound Details")]
    public SoundEffectSO activeUniqueSkillSoundEffectOne;
    public SoundEffectSO activeUniqueSkillSoundEffectTwo;

    [Space(10)]
    [Header("Skill Duration Details")]
    public float activeUniqueSkillCooldownDuration = 20f;
    public float activeUniqueSkillEffectiveDuration = 0f;

    [Space(10)]
    [Header("Mana Details")]
    public int activeUniqueSkillManaCost = 10;
    public int activeUniqueSkillManaReserveCost = 15;
}
