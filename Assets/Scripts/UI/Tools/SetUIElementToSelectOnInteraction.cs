using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SetUIElementToSelectOnInteraction : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] Selectable elementToSelect;

    [Header("Visualization")]
    [SerializeField] bool showVisualization;
    [SerializeField] Color navigationColor = Color.cyan;

    private void OnEnable()
    {
        if(GetComponent<SessionEntryUI>() != null)
        {
            Transform backButtonTransform = GetComponentInParent<MultiplayerEntryUI>().transform.Find("BackButton");
            elementToSelect = backButtonTransform.GetComponent<Selectable>();
        }
    }

    private void OnDrawGizmos()
    {
        if (!showVisualization) return;

        if (elementToSelect == null) return;

        Gizmos.color = navigationColor;
        Gizmos.DrawLine(transform.position, elementToSelect.transform.position);
    }

    private void Reset()
    {
        if (EventSystem.current == null) Debug.Log("Did not find an Event system in your Scene.", this);
    }

    public void JumpToElement()
    {
        if (EventSystem.current == null) Debug.Log("This item has no event system referenced yet.", this);

        if (elementToSelect == null) Debug.Log("This should jump where?", this);

        EventSystem.current.SetSelectedGameObject(elementToSelect.gameObject);
    }
}
