using System.Collections;
using UnityEngine;

public class GameplayUI : MonoBehaviour
{
    [HideInInspector] public CanvasGroup canvasGroup;

    Coroutine gameplayUIRoutine;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void FadeGameplayUI(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        if (gameplayUIRoutine != null) StopCoroutine(gameplayUIRoutine);

        gameplayUIRoutine = StartCoroutine(FadeCanvasGroup(canvasGroup, targetAlpha, duration));
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration)
    {
        float startAlpha = cg.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha; // Ensure it reaches the final value precisely

        canvasGroup.blocksRaycasts = canvasGroup.alpha <= 0 + Mathf.Epsilon ? false : true;

        gameplayUIRoutine = null;
    }
}
