using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActiveUniqueSkillSlot : MonoBehaviour, IDropHandler, ISelectHandler, IDeselectHandler
{
    public int slotIndex = 1;

    [HideInInspector] public ActiveUniqueSkillDetailsSO activeUniqueSkillDetails;

    Image skillImage;
    Player player;

    bool isSelected;

    private void Awake()
    {
        skillImage = GetComponent<Image>();
    }

    private void Start()
    {
        player = GameManager.Instance.GetPlayer();
        PopulateSkillIconToSlot();
    }

    private void Update()
    {
        if (!isSelected) return;

        if (InputManager.Instance.click.action.WasPerformedThisFrame())
        {
            TryAssignPickedSkill();
        }
    }

    private void PopulateSkillIconToSlot()
    {
        if (player.currentlyUsedActiveUniqueSkills.TryGetValue(slotIndex, out ActiveUniqueSkillDetailsSO skill))
        {
            activeUniqueSkillDetails = skill;
            skillImage.sprite = activeUniqueSkillDetails.activeUniqueSkillSprite;
        }
    }

    // Drop via drag & drop
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null &&
            eventData.pointerDrag.TryGetComponent(out DraggableSkillIcon draggableSkillIcon))
        {
            TryPlaceSkill(draggableSkillIcon.activeUniqueSkillDetails, draggableSkillIcon.image.sprite);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
    }

    public void PlacementForGamepad()
    {
        if (!isSelected) return;

        // Act like "drop" instantly if something is picked
        var picked = SkillSelectionManager.PickedSkill;

        if (picked != null)
        {
            if (TryPlaceSkill(picked.activeUniqueSkillDetails, picked.image.sprite))
            {
                SkillSelectionManager.Clear(); // clear buffer if placed successfully
            }
        }
    }

    // Shared placement logic
    private bool TryPlaceSkill(ActiveUniqueSkillDetailsSO newSkill, Sprite newSprite)
    {
        // Prevent duplicates
        foreach (var item in player.currentlyUsedActiveUniqueSkills)
        {
            if (item.Value == newSkill)
            {
                Debug.Log("You are trying to place a skill already occupied.");
                return false;
            }
        }

        // Replace if slot is already filled
        if (player.currentlyUsedActiveUniqueSkills.ContainsKey(slotIndex))
            player.currentlyUsedActiveUniqueSkills.Remove(slotIndex);

        // Assign
        player.currentlyUsedActiveUniqueSkills.Add(slotIndex, newSkill);
        activeUniqueSkillDetails = newSkill;
        skillImage.sprite = newSprite;

        // Notify gameplay HUD
        StaticEventHandler.CallActiveUniqueSkillPlacedEvent(slotIndex, activeUniqueSkillDetails, true);

        return true;
    }

    private void TryAssignPickedSkill()
    {
        var picked = SkillSelectionManager.PickedSkill;
        if (picked == null) return;

        if (TryPlaceSkill(picked.activeUniqueSkillDetails, picked.image.sprite))
        {
            SkillSelectionManager.Clear();
        }
    }
}
