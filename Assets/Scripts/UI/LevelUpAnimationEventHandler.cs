using UnityEngine;
using UnityEngine.Events;

public class LevelUpAnimationEventHandler : MonoBehaviour
{
    public UnityEvent OnLevelUpPanelOpened;
    public UnityEvent OnLevelUpPanelClosed;

    Animator levelUpPanelAnimator;

    private void OnEnable()
    {
        levelUpPanelAnimator = GetComponent<Animator>();

        OnLevelUpPanelOpened.AddListener(OpenLevelUpPanel);
        OnLevelUpPanelClosed.AddListener(CloseLevelUpPanel);
    }

    private void OnDisable()
    {
        OnLevelUpPanelOpened.RemoveListener(OpenLevelUpPanel);
        OnLevelUpPanelClosed.RemoveListener(CloseLevelUpPanel);
    }

    public void CallLevelUpPanelOpenEvent()
    {
        OnLevelUpPanelOpened?.Invoke();
    }

    public void CallLevelUpPanelCloseEvent()
    {
        OnLevelUpPanelClosed?.Invoke();
    }

    private void OpenLevelUpPanel()
    {
        GameManager.Instance.levelUpZoomInFinished = true;
    }

    private void CloseLevelUpPanel()
    {
        GameManager.Instance.levelUpZoomOutFinished = true;
    }
}
