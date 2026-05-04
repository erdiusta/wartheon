public static class SkillSelectionManager
{
    public static DraggableSkillIcon PickedSkill { get; private set; }

    public static void Pick(DraggableSkillIcon skill)
    {
        if (PickedSkill != null) Clear(); // only one at a time

        PickedSkill = skill;
        skill.Highlight(true);
    }

    public static void Clear()
    {
        if(PickedSkill != null)
        {
            PickedSkill.Highlight(false);
            PickedSkill = null;
        }
    }
}
