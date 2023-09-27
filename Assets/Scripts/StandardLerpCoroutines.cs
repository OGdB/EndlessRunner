using System.Collections;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public static class StandardLerpCoroutines
{
    /// <summary>
    /// Lerp to a certain scaling
    /// </summary>
    /// <param name="objectTransform">Reference to the Transform.</param>
    /// <param name="targetScale">The desired scaling.</param>
    /// <param name="speed">The amount of seconds it should take to reach the desired scaling.</param>
    public static IEnumerator LerpToScale(Transform objectTransform, float speed, Vector3 targetScale)
    {
        WaitForEndOfFrame frame = new();
        float startTime = Time.unscaledTime;
        Vector3 startSize = objectTransform.localScale;

        while (objectTransform.localScale != targetScale)
        {
            float timeSinceStarted = Time.unscaledTime - startTime;
            float progress = timeSinceStarted / speed;
            objectTransform.localScale = Vector3.Lerp(startSize, targetScale, progress);

            yield return frame;
        }
    }

    /// <summary>
    /// Lerp text to a specified alpha
    /// </summary>
    /// <param name="text">the text to fade out.</param> the text to fade out.
    /// <param name="targetAlpha">the alpha to fade towards.</param> the alpha to fade towards.
    /// <param name="speed">the amount of seconds the fading should take.</param>
    public static IEnumerator FadeTextAlpha(TMPro.TextMeshProUGUI text, float targetAlpha, float speed)
    {
        WaitForEndOfFrame frame = new();

        float startTime = Time.unscaledTime;
        float startAlpha = text.alpha;

        while (text.alpha != targetAlpha)
        {
            float timeSinceStarted = Time.unscaledTime - startTime;
            float progress = timeSinceStarted / speed;

            text.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

            yield return frame;
        }
    }

    /// <summary>
    /// Lerp a canvas to a specified alpha
    /// </summary>
    /// <param name="canvas">the canvas to fade out.</param>
    /// <param name="targetAlpha"></param> the alpha to fade towards.
    /// <param name="speed">the amount of seconds the fading should take.</param>
    public static IEnumerator FadeCanvasAlpha(CanvasGroup canvas, float targetAlpha, float speed)
    {
        WaitForEndOfFrame frame = new();
        float startTime = Time.unscaledTime;
        float startAlpha = canvas.alpha;

        bool enabling = targetAlpha > 0;  // Showing or hiding the canvas.
        canvas.interactable = enabling;
        canvas.blocksRaycasts = enabling;

        while (canvas.alpha != targetAlpha)
        {
            float timeSinceStarted = Time.unscaledTime - startTime;
            float progress = timeSinceStarted / speed;

            canvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

            yield return frame;
        }
    }


    /// <summary>
    /// Wait a specified amount of seconds
    /// </summary>
    /// <param name="secondsToWait">Amount of seconds to wait</param>
    public static IEnumerator WaitForSecondsLerp(float secondsToWait)
    {
        WaitForEndOfFrame frame = new();

        float startTime = Time.unscaledTime;
        float progress = 0;

        while (progress <= 1f)
        {
            float timeSinceStarted = Time.unscaledTime - startTime;
            progress = timeSinceStarted / secondsToWait;

            yield return frame;
        }
    }
}