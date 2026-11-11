
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using TMPro;
using FMODUnity;
using FMOD.Studio;

public class TVReportIntroController : MonoBehaviour
{
    [Serializable]
    public class SubtitleCue
    {
        public float startTime;
        public float duration = 4f;

        [TextArea(2, 5)]
        public string text;

        public EventReference voice;
    }

    [Header("Timelines")]
    [SerializeField] private PlayableDirector cameraTimeline;
    [SerializeField] private PlayableDirector newsTextTimeline;

    [Header("3D Television")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Renderer tvScreenRenderer;
    [SerializeField] private int screenMaterialIndex;
    [SerializeField] private Material tvOnMaterial;
    [SerializeField] private Material tvOffMaterial;
    [SerializeField] private Light[] televisionLights;

    [Header("Daniel")]
    [SerializeField] private Transform danielTransform;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private SubtitleCue[] subtitles;

    [Header("Scene Transition")]
    [SerializeField] private CanvasGroup blackScreen;
    [SerializeField] private string nextScene = "MainScene";
    [SerializeField] private float fadeDuration = 0.7f;

    private bool started;
    private bool tvTurnedOff;
    private int currentSubtitle = -1;

    private EventInstance currentVoice;

    private IEnumerator Start()
    {
        if (started)
            yield break;

        started = true;
        Pause.canPause = false;

        if (cameraTimeline == null ||
            videoPlayer == null ||
            blackScreen == null)
        {
            Debug.LogError(
                "[TV INTRO] Required references are missing."
            );
            yield break;
        }

        blackScreen.alpha = 1f;
        blackScreen.blocksRaycasts = true;

        if (subtitleText != null)
            subtitleText.text = "";

        // Stop any automatically started Timeline.
        cameraTimeline.Stop();
        cameraTimeline.time = 0;

        if (newsTextTimeline != null)
        {
            newsTextTimeline.Stop();
            newsTextTimeline.time = 0;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;

        // Prepare the television video before showing the scene.
        videoPlayer.Prepare();

        float prepareElapsed = 0f;

        while (!videoPlayer.isPrepared &&
               prepareElapsed < 15f)
        {
            prepareElapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!videoPlayer.isPrepared)
        {
            Debug.LogWarning(
                "[TV INTRO] Video preparation timed out."
            );
        }

        SetScreenMaterial(tvOnMaterial);

        foreach (Light light in televisionLights)
        {
            if (light != null)
                light.enabled = true;
        }

        videoPlayer.Play();

        cameraTimeline.Play();

        if (newsTextTimeline != null)
            newsTextTimeline.Play();

        Debug.Log("[TV INTRO] Cinematic started.");

        yield return Fade(1f, 0f);

        float elapsed = 0f;

        float lastSubtitleEnd = 0f;

        if (subtitles != null)
        {
            foreach (SubtitleCue cue in subtitles)
            {
                lastSubtitleEnd = Mathf.Max(
                    lastSubtitleEnd,
                    cue.startTime + cue.duration
                );
            }
        }

        float totalDuration = Mathf.Max(
            (float)cameraTimeline.duration,
            lastSubtitleEnd + 0.7f
        );

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            UpdateSubtitles(elapsed);

            if (!tvTurnedOff &&
                lastSubtitleEnd > 0 &&
                elapsed >= lastSubtitleEnd)
            {
                TurnOffTelevision();
            }

            yield return null;
        }

        if (!tvTurnedOff)
            TurnOffTelevision();

        StopVoice();

        if (subtitleText != null)
            subtitleText.text = "";

        yield return Fade(0f, 1f);

        cameraTimeline.Stop();

        if (newsTextTimeline != null)
            newsTextTimeline.Stop();

        Debug.Log(
            "[TV INTRO] Finished. Loading MainScene."
        );

        if (!Application.CanStreamedLevelBeLoaded(nextScene))
        {
            Debug.LogError(
                "[TV INTRO] Scene not in Build Settings: " +
                nextScene
            );
            yield break;
        }

        yield return SceneManager.LoadSceneAsync(
            nextScene,
            LoadSceneMode.Single
        );
    }

    private void UpdateSubtitles(float elapsed)
    {
        if (subtitles == null)
            return;

        int activeIndex = -1;

        for (int i = 0; i < subtitles.Length; i++)
        {
            SubtitleCue cue = subtitles[i];

            if (elapsed >= cue.startTime &&
                elapsed < cue.startTime + cue.duration)
            {
                activeIndex = i;
                break;
            }
        }

        if (activeIndex == currentSubtitle)
            return;

        StopVoice();
        currentSubtitle = activeIndex;

        if (subtitleText != null)
        {
            subtitleText.text = activeIndex >= 0
                ? subtitles[activeIndex].text
                : "";
        }

        if (activeIndex < 0)
            return;

        EventReference voice = subtitles[activeIndex].voice;

        if (!voice.IsNull)
        {
            currentVoice = RuntimeManager.CreateInstance(voice);

            if (danielTransform != null)
            {
                currentVoice.set3DAttributes(
                    RuntimeUtils.To3DAttributes(
                        danielTransform.position
                    )
                );
            }

            currentVoice.start();
        }
    }

    private void StopVoice()
    {
        if (!currentVoice.isValid())
            return;

        currentVoice.stop(
            FMOD.Studio.STOP_MODE.ALLOWFADEOUT
        );

        currentVoice.release();
        currentVoice.clearHandle();
    }

    private void TurnOffTelevision()
    {
        tvTurnedOff = true;

        videoPlayer.Stop();

        SetScreenMaterial(tvOffMaterial);

        foreach (Light light in televisionLights)
        {
            if (light != null)
                light.enabled = false;
        }

        Debug.Log("[TV INTRO] Television switched off.");
    }

    private void SetScreenMaterial(Material material)
    {
        if (tvScreenRenderer == null || material == null)
            return;

        Material[] materials = tvScreenRenderer.materials;

        if (screenMaterialIndex < 0 ||
            screenMaterialIndex >= materials.Length)
        {
            Debug.LogError(
                "[TV INTRO] Incorrect TV material index."
            );
            return;
        }

        materials[screenMaterialIndex] = material;
        tvScreenRenderer.materials = materials;
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        blackScreen.alpha = from;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = fadeDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / fadeDuration);

            blackScreen.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        blackScreen.alpha = to;
    }

    private void OnDisable()
    {
        StopVoice();
    }
}
