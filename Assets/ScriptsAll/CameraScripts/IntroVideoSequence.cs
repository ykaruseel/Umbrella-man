
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class IntroVideoSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Safety")]
    [SerializeField] private float prepareTimeout = 15f;
    [SerializeField] private float playbackTimeout = 180f;

    private bool finished;
    private string playbackError;

    private void Awake()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
        }
    }

    public IEnumerator PlaySequence()
    {
        if (videoPlayer == null ||
            canvasGroup == null ||
            videoPlayer.clip == null)
        {
            Debug.LogWarning(
                "[IntroVideo] Missing references. Skipping video."
            );
            yield break;
        }

        Debug.Log("[IntroVideo] Preparing video...");

        finished = false;
        playbackError = null;

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;

        videoPlayer.Prepare();

        float elapsed = 0f;

        while (!videoPlayer.isPrepared &&
               string.IsNullOrEmpty(playbackError) &&
               elapsed < prepareTimeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (videoPlayer.isPrepared &&
            string.IsNullOrEmpty(playbackError))
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            Debug.Log("[IntroVideo] Video started.");

            videoPlayer.Play();

            elapsed = 0f;

            while (!finished &&
                   string.IsNullOrEmpty(playbackError) &&
                   elapsed < playbackTimeout)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!finished &&
                string.IsNullOrEmpty(playbackError))
            {
                Debug.LogWarning(
                    "[IntroVideo] Playback timeout."
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "[IntroVideo] Could not prepare video."
            );
        }

        videoPlayer.Stop();

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.errorReceived -= OnVideoError;

        Debug.Log("[IntroVideo] Sequence finished.");
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        finished = true;
    }

    private void OnVideoError(
        VideoPlayer source,
        string message)
    {
        playbackError = message;

        Debug.LogError(
            "[IntroVideo] " + message
        );
    }

    [ContextMenu("TEST: Play Intro Video")]
    private void TestVideo()
    {
        if (Application.isPlaying)
            StartCoroutine(PlaySequence());
    }
}
