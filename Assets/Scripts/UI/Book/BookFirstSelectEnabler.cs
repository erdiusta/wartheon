using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BookFirstSelectEnabler : MonoBehaviour
{
    public Button charButton;

    private void OnEnable()
    {
        // Delay setting selection to next frame to ensure layout is updated
        StartCoroutine(SetResumeButtonAsFirstSelected());
    }

    IEnumerator SetResumeButtonAsFirstSelected()
    {
        yield return null; // wait one frame

        EventSystem.current.SetSelectedGameObject(charButton.gameObject);
    }
}
