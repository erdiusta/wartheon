using UnityEngine;
using UnityEngine.Events;

public class RiftEventAnimationHandler : MonoBehaviour
{
    public UnityEvent OnRiftOpened;

    private void OnEnable()
    {
        OnRiftOpened.AddListener(CallCinematicScenePhaseChange);
    }

    private void OnDisable()
    {
        OnRiftOpened.RemoveListener(CallCinematicScenePhaseChange);
    }

    public void TriggerRiftOpenedEvent()
    {
        OnRiftOpened?.Invoke();
    }

    private void CallCinematicScenePhaseChange()
    {
        CinematicSceneManager.Instance.cinematicPhase = CinematicPhase.mobSpilledFromRift;
    }
}
