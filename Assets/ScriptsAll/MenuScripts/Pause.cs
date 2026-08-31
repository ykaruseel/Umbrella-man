using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public static bool isPaused = false;

    public static bool canPause;

    [SerializeField] private PlayerController _playerController;
    [SerializeField] private UmbrellaManChase _umbrellaManChase;
    //[SerializeField] private GameObject _pauseMenuUI;
    [SerializeField] private List<GameObject> _UIElements = new();
    [SerializeField] private EnemyLookDistortionSingleVolume _enemyLookDistortionSingleVolume;

    [SerializeField] private KnifeManAI _knifeManAI;

    private float fadeDuration = 0.5f;

    private bool isTransitioning = false;

    [SerializeField] private CanvasGroup _pauseMenu;

    //[SerializeField] private GameObject _mainButtons;
    //[SerializeField] private GameObject _settings;
    //[SerializeField] private GameObject _credits;

    [SerializeField] private MenuFader _menuFader;

    private float contrast;
    private float chromatic;
    private float grainIntensity;
    private float saturation;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (!canPause) return;
            if(_knifeManAI != null && _knifeManAI.isChasing) return;
            foreach (GameObject obj in _UIElements)
            {
                if (obj == null) continue;

                if (obj.activeSelf) return;
            }
            if (isPaused)
            {
                Debug.Log("Resuming Game from Pause Menu");
                ResumeGame();
            }
            else
            {
                Debug.Log("Pausing Game from Pause Menu");
                PauseGame();
            }
        }
    }

    private void PauseGame()
    {
        if (isTransitioning) return;
        StartCoroutine(FadeIn());
    }

    public void ResumeGame()
    {
        if (isTransitioning) return;
        StartCoroutine(FadeOut());
        //ResetPause();
    }

    public void ResetPause()
    {
        /*isPaused = false;
        _mainButtons.SetActive(true);
        _settings.SetActive(false);
        if(_credits != null)
            _credits.SetActive(false);
        _pauseMenuUI.SetActive(false);*/
    }

    private IEnumerator FadeOut()
    {
        PauseAudioSnapshot.Instance?.ExitPause();
        isTransitioning = true;

        _pauseMenu.interactable = false;
        _pauseMenu.blocksRaycasts = false;

        float elapsed = 0f;

        float startAlpha = _pauseMenu.alpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;

            _pauseMenu.alpha = Mathf.Lerp(startAlpha, 0f, t);
            _enemyLookDistortionSingleVolume.baseSaturation = Mathf.Lerp(_enemyLookDistortionSingleVolume.baseSaturation, -10f, t);
            _enemyLookDistortionSingleVolume.baseContrast = Mathf.Lerp(_enemyLookDistortionSingleVolume.baseContrast, -2f, t);
            _enemyLookDistortionSingleVolume.baseChromatic = Mathf.Lerp(_enemyLookDistortionSingleVolume.baseChromatic, 0.3f, t);
            _enemyLookDistortionSingleVolume.baseGrainIntensity = Mathf.Lerp(_enemyLookDistortionSingleVolume.baseGrainIntensity, 0.4f, t);

            yield return null;
        }

        _pauseMenu.alpha = 0f;

        _enemyLookDistortionSingleVolume.baseSaturation = -10f;
        _enemyLookDistortionSingleVolume.baseContrast = -2f;
        _enemyLookDistortionSingleVolume.baseChromatic = 0.3f;
        _enemyLookDistortionSingleVolume.baseGrainIntensity = 0.4f;

        isPaused = false;
        _menuFader.ResetFade();

        _playerController.SetCanMove(true);
        _playerController.SetDialogueZoom(true);

        if (_umbrellaManChase!= null && _umbrellaManChase.gameObject.activeSelf)
            _umbrellaManChase.ResumeChase();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        DialogueManager.instance?.SetPaused();

        isTransitioning = false;

    }

    private IEnumerator FadeIn()
    {
        PauseAudioSnapshot.Instance?.EnterPause();
        isPaused = true;
        //_pauseMenuUI.SetActive(true);

        _playerController.SetCanMove(false);
        _playerController.SetDialogueZoom(false);

        if (_umbrellaManChase != null && _umbrellaManChase.gameObject.activeSelf)
            _umbrellaManChase.PauseChase();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DialogueManager.instance?.SetPaused();

        isTransitioning = true;

        float elapsed = 0f;

        float startAlpha = _pauseMenu.alpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;

            _pauseMenu.alpha = Mathf.Lerp(startAlpha, 1f, t);
            _enemyLookDistortionSingleVolume.baseSaturation = Mathf.Lerp(_enemyLookDistortionSingleVolume.baseSaturation, -50f, t);
            _enemyLookDistortionSingleVolume.baseContrast = Mathf.Lerp(_enemyLookDistortionSingleVolume.baseContrast, 30f, t);
            _enemyLookDistortionSingleVolume.baseChromatic = Mathf.Lerp(_enemyLookDistortionSingleVolume.baseChromatic, 1f, t);
            _enemyLookDistortionSingleVolume.baseGrainIntensity = Mathf.Lerp(_enemyLookDistortionSingleVolume.baseGrainIntensity, 1f, t);

            yield return null;
        }

        _pauseMenu.alpha = 1;

        _enemyLookDistortionSingleVolume.baseSaturation = -50f;
        _enemyLookDistortionSingleVolume.baseContrast = 30f;
        _enemyLookDistortionSingleVolume.baseChromatic = 1f;
        _enemyLookDistortionSingleVolume.baseGrainIntensity = 1f;

        isTransitioning = false;

        _pauseMenu.interactable = true;
        _pauseMenu.blocksRaycasts = true;
    }

    private void OnDestroy()
    {
        isPaused = false;
        PauseAudioSnapshot.Instance?.ExitPause();
    }
}
