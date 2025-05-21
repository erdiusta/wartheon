using UnityEngine;
using UnityEngine.Events;

public class BookAnimationEventHandler : MonoBehaviour
{
    public UnityEvent OnTurnPageCompleted;
    public UnityEvent OnBookClosed;
    public UnityEvent OnBookOpened;

    private void OnEnable()
    {
        OnTurnPageCompleted.AddListener(CompleteTurnPage);
        OnBookClosed.AddListener(CloseBook);
        OnBookOpened.AddListener(OpenBook);
    }

    private void OnDisable()
    {
        OnTurnPageCompleted.RemoveListener(CompleteTurnPage);
        OnBookClosed.RemoveListener(CloseBook);
        OnBookOpened.RemoveListener(OpenBook);
    }

    public void CallTurnPageCompletedEvent()
    {
        OnTurnPageCompleted?.Invoke();
    }

    public void CallOpenBookEvent()
    {
        OnBookOpened?.Invoke();
    }

    public void CallCloseBookEvent()
    {
        OnBookClosed?.Invoke();
    }

    private void CompleteTurnPage()
    {
        GameManager.Instance.turnPageCompleted = true;
    }

    private void OpenBook()
    {
        GameManager.Instance.bookZoomInFinished = true;
    }

    private void CloseBook()
    {
        GameManager.Instance.bookZoomOutFinished = true;
    }
}
