using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WeaponSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public WeaponDetailsSO weaponDetails;

    [HideInInspector] public bool weaponUnlocked = false;

    private void Start()
    {
        switch (GameManager.Instance.GetPlayer().playerDetails.playerCharacterIndex)
        {
            case Character.Caelion:
                if (weaponDetails.weaponTitle == WeaponTitle.Hatchet || weaponDetails.weaponTitle == WeaponTitle.Shield)
                {
                    weaponUnlocked = true;
                    transform.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                    transform.GetComponent<Button>().interactable = true;
                }
                break;
            case Character.Morven:
                if (weaponDetails.weaponTitle == WeaponTitle.Dirk)
                {
                    weaponUnlocked = true;
                    transform.GetComponent<Image>().color = Color.white;
                    transform.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                    transform.GetComponent<Button>().interactable = true;
                }
                break;
            case Character.Nyveran:
                if (weaponDetails.weaponTitle == WeaponTitle.CrudeBow || weaponDetails.weaponTitle == WeaponTitle.Dirk)
                {
                    weaponUnlocked = true;
                    transform.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                    transform.GetComponent<Button>().interactable = true;
                }
                break;
            case Character.Mycara:
                if (weaponDetails.weaponTitle == WeaponTitle.OldStaff)
                {
                    weaponUnlocked = true;
                    transform.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                    transform.GetComponent<Button>().interactable = true;
                }
                break;
            default:
                break;
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (weaponUnlocked)
        {
            StaticEventHandler.CallWeaponHoveredEvent(weaponDetails.weaponTitle);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StaticEventHandler.CallWeaponUnhoveredEvent();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (weaponUnlocked)
        {
            StaticEventHandler.CallWeaponHoveredEvent(weaponDetails.weaponTitle);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        StaticEventHandler.CallWeaponUnhoveredEvent();
    }
}
