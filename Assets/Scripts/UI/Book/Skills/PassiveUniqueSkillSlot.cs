using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PassiveUniqueSkillSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    Image skillImage;
    Player player;

    private void Awake()
    {
        skillImage = GetComponent<Image>();
    }

    private void Start()
    {
        player = GameManager.Instance.GetPlayer();

        skillImage.sprite = player.playerDetails.passiveSkillImage;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void OnDrop(PointerEventData eventData)
    {

    }
}
