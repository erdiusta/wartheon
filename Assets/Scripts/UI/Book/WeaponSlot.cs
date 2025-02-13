using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WeaponSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public WeaponDetailsSO weaponDetails;

    [HideInInspector] public bool weaponUnlocked = false;

    private void Start()
    {
        switch (GameManager.Instance.GetPlayer().playerDetails.playerCharacterIndex)
        {
            case Character.Astraeus:
                if (weaponDetails.weaponTitle == WeaponTitle.Hatchet || weaponDetails.weaponTitle == WeaponTitle.Shield)
                {
                    weaponUnlocked = true;
                    transform.GetComponent<Image>().color = Color.white;
                }
                break;
            case Character.Erebus:
                if (weaponDetails.weaponTitle == WeaponTitle.Dirk)
                {
                    weaponUnlocked = true;
                    transform.GetComponent<Image>().color = Color.white;
                }
                break;
            case Character.Orion:
                if (weaponDetails.weaponTitle == WeaponTitle.CrudeBow || weaponDetails.weaponTitle == WeaponTitle.Dirk)
                {
                    weaponUnlocked = true;
                    transform.GetComponent<Image>().color = Color.white;
                }
                break;
            case Character.Lyrisa:
                if (weaponDetails.weaponTitle == WeaponTitle.OldStaff)
                {
                    weaponUnlocked = true;
                    transform.GetComponent<Image>().color = Color.white;
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
}
