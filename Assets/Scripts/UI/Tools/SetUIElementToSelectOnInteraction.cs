using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SetUIElementToSelectOnInteraction : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] EventSystem eventSystem;
    [SerializeField] Selectable elementToSelect;

    [Header("Visualization")]
    [SerializeField] bool showVisualization;
    [SerializeField] Color navigationColor = Color.cyan;

    private void OnDrawGizmos()
    {
        if (!showVisualization) return;

        if (elementToSelect == null) return;

        Gizmos.color = navigationColor;
        Gizmos.DrawLine(transform.position, elementToSelect.transform.position);
    }

    private void Reset()
    {
        eventSystem = FindObjectOfType<EventSystem>();

        if (eventSystem == null)
            Debug.Log("Did not find an Event system in your Scene.", this);
    }

    public void JumpToElement()
    {
        if (eventSystem == null) Debug.Log("This item has no event system referenced yet.", this);

        if (elementToSelect == null) Debug.Log("This should jump where?", this);

        eventSystem.SetSelectedGameObject(elementToSelect.gameObject);
    }
}
