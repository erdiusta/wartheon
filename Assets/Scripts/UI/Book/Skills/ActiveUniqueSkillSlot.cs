using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActiveUniqueSkillSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int slotIndex = 1;

    [HideInInspector] public ActiveUniqueSkillDetailsSO activeUniqueSkillDetails;

    Image skillImage;
    Player player;

    private void Awake()
    {
        skillImage = GetComponent<Image>();
    }

    private void Start()
    {
        player = GameManager.Instance.GetPlayer();

        PopulateSkillIconToSlot();
    }

    private void PopulateSkillIconToSlot()
    {
        activeUniqueSkillDetails = player.currentlyUsedActiveUniqueSkills[slotIndex];
        skillImage.sprite = activeUniqueSkillDetails.activeUniqueSkillSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && eventData.pointerDrag.TryGetComponent(out DraggableSkillIcon draggableSkillIcon))
        {
            // Check if same skill is already occupied in another slot
            foreach (KeyValuePair<int, ActiveUniqueSkillDetailsSO> item in player.currentlyUsedActiveUniqueSkills)
            {
                if (item.Value == draggableSkillIcon.activeUniqueSkillDetails)
                {
                    Debug.Log("You are trying to place a skill already occupied.");
                    return; // Already exists abort the placement transaction
                }
            }

            // Remove current slot's skill info if is occupied
            if (player.currentlyUsedActiveUniqueSkills.ContainsKey(slotIndex))
            {
                player.currentlyUsedActiveUniqueSkills.Remove(slotIndex);
            }

            // Populate dragged skill 
            player.currentlyUsedActiveUniqueSkills.Add(slotIndex, draggableSkillIcon.activeUniqueSkillDetails);
            activeUniqueSkillDetails = draggableSkillIcon.activeUniqueSkillDetails;
            skillImage.sprite = draggableSkillIcon.image.sprite;

            // Call event in order to update SkillUI in Gameplay HUD
            StaticEventHandler.CallActiveUniqueSkillPlacedEvent(slotIndex, activeUniqueSkillDetails, true);
        }
    }
}
