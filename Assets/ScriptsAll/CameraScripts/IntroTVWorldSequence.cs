
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class IntroTVWorldSequence : MonoBehaviour
{
    [Serializable]
    public class IntroLine
    {
        [TextArea(2, 4)]
        public string subtitle;

        public AudioClip voice;
        public float duration = 5f;

        [Range(0, 3)]
        public int shotIndex;

        public float cameraMoveTime = 2f;
    }

    [Header("Televisions")]
    [SerializeField] private GameObject originalTV;
    [SerializeField] private GameObject introTV;
    [SerializeField] private Renderer tvScreen;
    [SerializeField] private Material screenOnMaterial;
    [SerializeField] private Material screenOffMaterial;

    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private float prepareTimeout = 15f;

    [Header("Lighting")]
    [SerializeField] private Light tvSpotlight;
    [SerializeField] private Light tvPointLight;

    [Header("Cameras")]
    [SerializeField] private Camera cinematicCamera;
    [SerializeField] private Transform[] shots = new Transform[4];

    [Header("Daniel")]
    [SerializeField] private GameObject danielVisual;

    [Header("Dialogue")]
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private float newsLeadIn = 2f;
    [SerializeField] private float gapBetweenLines = 0.3f;

    [SerializeField] private IntroLine[] lines =
    {
        new IntroLine {
            subtitle = "A couple of days ago, Tommy Ray died.",
            duration = 4f, shotIndex = 1
        },
        new IntroLine {
            subtitle = "We worked for the same company, lived in the same building, and were good friends.",
            duration = 6f, shotIndex = 2
        },
        new IntroLine {
            subtitle = "This morning, on my way to work, I walked to his apartment out of habit, expecting us to leave together.",
            duration = 8f, shotIndex = 3
        },
        new IntroLine {
            subtitle = "Then I remembered he was gone.",
            duration = 4f, shotIndex = 3
        },
        new IntroLine {
            subtitle = "Tommy and I had more in common than I'd like to admit... and that's what scares me.",
            duration = 7f, shotIndex = 3
        },
        new IntroLine {
            subtitle = "I think I've had enough TV for one night.",
            duration = 4f, shotIndex = 3
        }
    };

    [Header("Transitions")]
    [SerializeField] private CameraFade fade;
    [SerializeField] private float tvOffHold = 1f;

    private bool hasPlayed;
    private string videoError;
    private bool originalTVWasActive;
    private Material previousScreenMaterial;
    private float spotIntensity;
    private float pointIntensity;

    private void Awake()
    {
        if (introTV != null)
            introTV.SetActive(false);

        if (cinematicCamera != null)
            cinematicCamera.gameObject.SetActive(false);

        if (danielVisual != null)
            danielVisual.SetActive(false);

        if (subtitleText != null)
            subtitleText.text = "";

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
        }

        if (tvSpotlight != null)
            spotIntensity = tvSpotlight.intensity;

        if (tvPointLight != null)
            pointIntensity = tvPointLight.intensity;
    }

    public IEnumerator PlaySequence()
    {
        if (hasPlayed)
            yield break;

        if (originalTV == null || introTV == null ||
            tvScreen == null || videoPlayer == null ||
            cinematicCamera == null || fade == null ||
            shots == null || shots.Length < 4 ||
            subtitleText == null)
        {
            Debug.LogError(
                "[IntroTV] Missing references in Inspector."
            );
            yield break;
        }

        for (int i = 0; i < 4; i++)
        {
            if (shots[i] == null)
            {
                Debug.LogError("[IntroTV] Missing shot " + i);
                yield break;
            }
        }

        hasPlayed = true;

        // Remember the original television state.
        originalTVWasActive = originalTV.activeSelf;

        // Intro begins under the existing black fade.
        fade.SetFadeImageActive(true);
        fade.SetFadeAlpha(1f);

        originalTV.SetActive(false);
        introTV.SetActive(true);

        previousScreenMaterial = tvScreen.sharedMaterial;

        if (screenOnMaterial != null)
            tvScreen.sharedMaterial = screenOnMaterial;

        if (danielVisual != null)
            danielVisual.SetActive(true);

        if (tvSpotlight != null)
        {
            tvSpotlight.intensity = spotIntensity;
            tvSpotlight.enabled = true;
        }

        if (tvPointLight != null)
        {
            tvPointLight.intensity = pointIntensity;
            tvPointLight.enabled = true;
        }

        cinematicCamera.transform.SetPositionAndRotation(
            shots[0].position,
            shots[0].rotation
        );

        cinematicCamera.gameObject.SetActive(true);

        videoError = null;
        videoPlayer.errorReceived += OnVideoError;

        // Prepare the video before showing the TV.
        videoPlayer.Prepare();

        float preparationTime = 0f;

        while (!videoPlayer.isPrepared &&
               string.IsNullOrEmpty(videoError) &&
               preparationTime < prepareTimeout)
        {
            preparationTime += Time.unscaledDeltaTime;
            yield return null;
        }

        
if (videoPlayer.isPrepared &&
    string.IsNullOrEmpty(videoError))
{
    videoPlayer.waitForFirstFrame = true;
    videoPlayer.Play();

    Debug.Log("[IntroTV] Waiting for first video frame...");

    float firstFrameTimeout = 5f;
    float elapsed = 0f;

    while (videoPlayer.frame < 0 &&
           string.IsNullOrEmpty(videoError) &&
           elapsed < firstFrameTimeout)
    {
        elapsed += Time.unscaledDeltaTime;
        yield return null;
    }

    if (videoPlayer.frame >= 0)
    {
        // Keep the first frame visible while the scene fades in.
        videoPlayer.Pause();

        Debug.Log(
            "[IntroTV] First frame ready: " +
            videoPlayer.frame
        );
    }
    else
    {
        Debug.LogWarning(
            "[IntroTV] First frame was not confirmed."
        );
    }
}
else
{
    Debug.LogWarning(
        "[IntroTV] Video could not be prepared."
    );
}


        yield return fade.FadeIn();

if (videoPlayer.isPrepared && videoPlayer.isPaused)
{
    videoPlayer.Play();
}

// Let the news report establish the scene.
yield return new WaitForSeconds(newsLeadIn);

        // Daniel's internal monologue.
        foreach (IntroLine line in lines)
        {
            if (line == null)
                continue;

            if (line.shotIndex >= 0 &&
                line.shotIndex < shots.Length &&
                shots[line.shotIndex] != null)
            {
                StartCoroutine(
                    MoveCameraTo(
                        shots[line.shotIndex],
                        line.cameraMoveTime
                    )
                );
            }

            subtitleText.text = line.subtitle;

            float lineDuration = Mathf.Max(0.1f, line.duration);

            if (voiceSource != null && line.voice != null)
            {
                voiceSource.Stop();
                voiceSource.clip = line.voice;
                voiceSource.Play();

                lineDuration = Mathf.Max(
                    lineDuration,
                    line.voice.length
                );
            }

            yield return new WaitForSeconds(lineDuration);

            if (voiceSource != null)
                voiceSource.Stop();

            subtitleText.text = "";

            yield return new WaitForSeconds(gapBetweenLines);
        }

        // The last line has finished: TV OFF.
        TurnOffTV();

        yield return new WaitForSeconds(tvOffHold);

        // Hide the model swap with a black screen.
        yield return fade.FadeOut();

        cinematicCamera.gameObject.SetActive(false);

        if (danielVisual != null)
            danielVisual.SetActive(false);

        introTV.SetActive(false);
        originalTV.SetActive(originalTVWasActive);

        if (previousScreenMaterial != null)
            tvScreen.sharedMaterial = previousScreenMaterial;

        videoPlayer.errorReceived -= OnVideoError;

        Debug.Log("[IntroTV] World intro finished.");
    }

    private IEnumerator MoveCameraTo(
        Transform target,
        float duration)
    {
        if (duration <= 0f)
        {
            cinematicCamera.transform.SetPositionAndRotation(
                target.position,
                target.rotation
            );
            yield break;
        }

        Vector3 startPosition =
            cinematicCamera.transform.position;

        Quaternion startRotation =
            cinematicCamera.transform.rotation;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsed / duration)
            );

            cinematicCamera.transform.position =
                Vector3.Lerp(
                    startPosition,
                    target.position,
                    t
                );

            cinematicCamera.transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    target.rotation,
                    t
                );

            yield return null;
        }

        cinematicCamera.transform.SetPositionAndRotation(
            target.position,
            target.rotation
        );
    }

    private void TurnOffTV()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();

        if (screenOffMaterial != null && tvScreen != null)
            tvScreen.sharedMaterial = screenOffMaterial;

        if (tvSpotlight != null)
            tvSpotlight.enabled = false;

        if (tvPointLight != null)
            tvPointLight.enabled = false;

        Debug.Log("[IntroTV] TV switched OFF.");
    }

    private void OnVideoError(
        VideoPlayer source,
        string message)
    {
        videoError = message;
        Debug.LogError("[IntroTV] " + message);
    }
}
