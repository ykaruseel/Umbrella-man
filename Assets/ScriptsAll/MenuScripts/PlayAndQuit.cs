using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayAndQuit : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        PauseAudioSnapshot.Instance?.ExitPause();
        Debug.Log("Trying to load scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void QuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }



    [Header("Camera Zoom")]
    public Camera cam;
    public Transform zoomTarget;
    public float zoomDuration = 1f;
    public float zoomFOV = 30f;

    [Header("UI")]
    public CanvasGroup blackScreen;
    public Slider progressBar;
    public GameObject progressBarGO;

    [Header("Scene")]
    public string sceneToLoad;

    private float originalFOV;
    private Coroutine zoomCoroutine;

    [SerializeField] private CanvasGroup UI;

    public LoadingScreenController loadingScreen;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        originalFOV = cam.fieldOfView;

        if (blackScreen != null)
        {
            blackScreen.alpha = 0;
            blackScreen.gameObject.SetActive(false);
        }
    }

    public void OnLoadLevelButton()
    {
        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);

        zoomCoroutine = StartCoroutine(ZoomToTarget());
    }

    private IEnumerator ZoomToTarget()
    {
        Quaternion startRot = cam.transform.rotation;
        Quaternion targetRot = zoomTarget.rotation;

        float startFOV = cam.fieldOfView;
        float elapsed = 0f;

        UI.interactable = false;
        UI.blocksRaycasts = false;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / zoomDuration);

            cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            cam.fieldOfView = Mathf.Lerp(startFOV, zoomFOV, t);

            while (UI.alpha > 0)
            {
                UI.alpha -= Time.unscaledDeltaTime / zoomDuration / 2f;
                yield return null;
            }
            yield return null;
        }

        UI.alpha = 0;

        cam.transform.rotation = targetRot;
        cam.fieldOfView = zoomFOV;

        Destroy(UI.gameObject);

        StartCoroutine(LoadSceneAsync());
    }

    [SerializeField] private float fadeDuration = 1f;

    private IEnumerator FadeToBlack()
    {
        if (blackScreen == null) yield break;

        blackScreen.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            blackScreen.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        blackScreen.alpha = 1f;
    }

    private IEnumerator LoadSceneAsync()
    {
        blackScreen.gameObject.SetActive(true);
        progressBarGO.SetActive(true);

        float fadeTime = 0.5f;
        float fade = 0f;
        while (fade < fadeTime)
        {
            fade += Time.deltaTime;
            blackScreen.alpha = fade / fadeTime;
            yield return null;
        }
        blackScreen.alpha = 1f;

        StartCoroutine(loadingScreen.SequenceRoutine());

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        op.allowSceneActivation = false;

        float displayProgress = 0f;
        float fakeDuration = 2.5f;
        float timer = 0f;

        while (!op.isDone)
        {
            timer += Time.deltaTime;

            float realProgress = Mathf.Clamp01(op.progress / 0.9f);

            float fakeProgress = Mathf.Clamp01(timer / fakeDuration);

            displayProgress = Mathf.Min(fakeProgress, realProgress);

            progressBar.value = displayProgress;

            if (realProgress >= 1f && fakeProgress >= 1f)
            {
                progressBarGO.SetActive(false);

                StartCoroutine(loadingScreen.TitleWrite());

                yield return new WaitUntil(() => LoadingScreenController.CanSwitchScenes);

                cam.gameObject.SetActive(false);

                op.allowSceneActivation = true;
            }

            yield return null;
        }

        Scene gameScene = SceneManager.GetSceneByName(sceneToLoad);
        SceneManager.SetActiveScene(gameScene);

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync("MenuScene");
        while (!unloadOp.isDone)
            yield return null;
    }

    //private IEnumerator LoadSceneAsync()
    //{
    //    blackScreen.gameObject.SetActive(true);

    //    float fadeTime = 0.5f;
    //    float fade = 0f;
    //    while (fade < fadeTime)
    //    {
    //        fade += Time.deltaTime;
    //        blackScreen.alpha = fade / fadeTime;
    //        yield return null;
    //    }
    //    blackScreen.alpha = 1f;

    //    loadingScreen.StartSequence();

    //    AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
    //    op.allowSceneActivation = false;

    //    float displayProgress = 0f;
    //    float fakeDuration = 2.5f;
    //    float timer = 0f;

    //    while (!op.isDone)
    //    {
    //        timer += Time.deltaTime;

    //        float realProgress = Mathf.Clamp01(op.progress / 0.9f);

    //        float fakeProgress = Mathf.Clamp01(timer / fakeDuration);

    //        displayProgress = Mathf.Min(fakeProgress, realProgress);

    //        progressBar.value = displayProgress;

    //        if (realProgress >= 1f && fakeProgress >= 1f)
    //        {
    //            cam.gameObject.SetActive(false);

    //            yield return new WaitForSeconds(1f);

    //            op.allowSceneActivation = true;
    //        }

    //        yield return null;
    //    }

    //    Scene gameScene = SceneManager.GetSceneByName(sceneToLoad);
    //    SceneManager.SetActiveScene(gameScene);

    //    AsyncOperation unloadOp = SceneManager.UnloadSceneAsync("MenuScene");
    //    while (!unloadOp.isDone)
    //        yield return null;

    //}
}