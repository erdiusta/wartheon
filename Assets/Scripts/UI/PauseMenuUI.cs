using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    private void OnEnable()
    {
        Time.timeScale = 0f;

        if (GameManager.Instance.glossaryBookOpen)
        {
            GameManager.Instance.CloseBookInCasePauseClick();
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
}
