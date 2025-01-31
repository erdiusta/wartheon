using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] GameObject playButton;
    [SerializeField] GameObject quitButton;

    void Start()
    {
        // Play music
        MusicManager.Instance.PlayMusic(GameResources.Instance.mainMenuMusic, 0f, 2f);

        //// Load character selector scene additively
        //SceneManager.LoadScene("CharacterSelectorScene", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Called from the Play Game Button
    /// </summary>
    public void PlayGame()
    {
        CanvasGroup canvasGroup = playButton.GetComponentInParent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        playButton.GetComponent<Button>().interactable = false;
        quitButton.GetComponent<Button>().interactable = false;

        playButton.SetActive(false);
        quitButton.SetActive(false);

        // Load character selector scene additively
        SceneManager.LoadScene("CharacterSelectorScene", LoadSceneMode.Additive);

        //SceneManager.LoadScene("CharacterSelectorScene");
    }

    /// <summary>
    /// Called from the Exit Game Button
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }
}
