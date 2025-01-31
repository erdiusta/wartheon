using UnityEngine;
using UnityEngine.Events;

public class HourglassEventHelper : MonoBehaviour
{
    public UnityEvent OnHourglassTimeUp;

    private void OnEnable()
    {
        OnHourglassTimeUp.AddListener(ResetTime);
    }

    private void OnDisable()
    {
        OnHourglassTimeUp.RemoveListener(ResetTime);
    }

    public void TriggerTimeUpEvent()
    {
        OnHourglassTimeUp?.Invoke();
    }

    private void ResetTime()
    {
        Time.timeScale = 1f;
        StaticEventHandler.CallHourglassExpired();
        Destroy(gameObject);
    }
}
