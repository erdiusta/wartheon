using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public static int currentDungeonLevelListIndex = 0;

    [SerializeField] GameObject playButton;
    [SerializeField] GameObject quitButton;

    void Start()
    {
        // Play music
        MusicManager.Instance.PlayMusic(GameResources.Instance.mainMenuMusic, 0f, 2f);

        //// Load character selector scene additively
        //SceneManager.LoadScene("CharacterSelectorScene", LoadSceneMode.Additive);
    }

    private void Update()
    {
        if (InputManager.Instance.levelOneButton.action.WasPressedThisFrame())
        {
            currentDungeonLevelListIndex = 0;
        }
        else if (InputManager.Instance.levelTwoButton.action.WasPressedThisFrame())
        {
            currentDungeonLevelListIndex = 1;
        }
        else if (InputManager.Instance.levelThreeButton.action.WasPressedThisFrame())
        {
            currentDungeonLevelListIndex = 2;
        }
        else if (InputManager.Instance.levelFourButton.action.WasPressedThisFrame())
        {
            currentDungeonLevelListIndex = 3;
        }
        else if (InputManager.Instance.levelFiveButton.action.WasPressedThisFrame())
        {
            currentDungeonLevelListIndex = 4;
        }
        else if (InputManager.Instance.levelSixButton.action.WasPressedThisFrame())
        {
            currentDungeonLevelListIndex = 5;
        }
        else if (InputManager.Instance.levelSevenButton.action.WasPressedThisFrame())
        {
            currentDungeonLevelListIndex = 6;
        }
        else if (InputManager.Instance.levelEightButton.action.WasPressedThisFrame())
        {
            currentDungeonLevelListIndex = 7;
        }
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
