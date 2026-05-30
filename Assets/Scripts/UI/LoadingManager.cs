using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance { get; private set; }

    [HideInInspector] public float maxWidth = 120f;
    [HideInInspector] public float minWidth = 0f;

    [Header("UI References")]
    public GameObject loadingScreen;
    public CanvasGroup loadingCanvasGroup;
    public RectTransform maskTransform;

    [Header("Smoothing")]
    public float smoothSpeed = 2f;
    public float minSmoothWidth = 5f;

    private float _targetProgress = 0f;
    private float _currentProgress = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeLoadingScreen();
    }

    private void InitializeLoadingScreen()
    {
        loadingScreen.SetActive(false);
        loadingCanvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset loading screen when certain scenes load
        if (scene.buildIndex == 0 || scene.buildIndex == 1) // Cinematic and Main Menu
        {
            ForceHideLoadingScreen();
        }
    }

    public void ForceHideLoadingScreen()
    {
        StopAllCoroutines();
        loadingCanvasGroup.alpha = 0f;
        loadingScreen.SetActive(false);
        _currentProgress = 0f;
        _targetProgress = 0f;
    }

    public static LoadingManager SafeInstance
    {
        get
        {
            if (Instance == null)
            {
                Instance = FindAnyObjectByType<LoadingManager>();

                if (Instance == null)
                {
                    GameObject manager = Instantiate(GameResources.Instance.loadingManager);
                    Instance = manager.GetComponent<LoadingManager>();
                }

                DontDestroyOnLoad(Instance.gameObject);
            }
            return Instance;
        }
    }

    private void Update()
    {
        if (_currentProgress < _targetProgress)
        {
            _currentProgress = Mathf.MoveTowards(_currentProgress, _targetProgress, Time.unscaledDeltaTime * smoothSpeed);
            float visibleProgress = Mathf.Max(_currentProgress, minSmoothWidth / maxWidth);
            float visibleWidth = Mathf.Lerp(minWidth, maxWidth, visibleProgress);
            maskTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, visibleWidth);
        }
    }

    public void ShowLoadingScreen()
    {
        StopAllCoroutines();

        loadingScreen.SetActive(true);
        loadingCanvasGroup.alpha = 1f;

        _currentProgress = 0f;
        _targetProgress = 1f;

        maskTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minWidth);
    }

    // Replace string parameter with int (scene index)
    public IEnumerator LoadGameScene(int sceneBuildIndex)
    {
        _currentProgress = 0f;
        _targetProgress = 0f;
        loadingScreen.SetActive(true);
        loadingCanvasGroup.alpha = 1f;

        yield return null; // Ensure UI initializes

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneBuildIndex);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            _targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            if (operation.progress >= 0.9f && _currentProgress >= 0.99f)
                operation.allowSceneActivation = true;

            yield return null;
        }

        yield return StartCoroutine(FadeOutLoadingScreen());
    }

    public void HideLoadingScreen(float fadeDuration = 0.5f)
    {
        StartCoroutine(FadeOutLoadingScreen(fadeDuration));
    }

    IEnumerator FadeOutLoadingScreen(float fadeDuration = 0.5f)
    {
        float time = 0f, startAlpha = loadingCanvasGroup.alpha;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            loadingCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, time / fadeDuration);
            yield return null;
        }
        loadingCanvasGroup.alpha = 0f;
        loadingScreen.SetActive(false);
    }
}
