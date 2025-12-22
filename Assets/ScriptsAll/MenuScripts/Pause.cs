using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public static bool isPaused = false;

    [SerializeField] private PlayerController _playerController;
    [SerializeField] private UmbrellaManChase _umbrellaManChase;
    [SerializeField] private GameObject _pauseMenuUI;
    [SerializeField] private List<GameObject> _UIElements = new();

    [SerializeField] private GameObject _mainButtons;
    [SerializeField] private GameObject _settings;
    [SerializeField] private GameObject _credits;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            foreach (GameObject obj in _UIElements)
            {
                if (obj == null) continue;

                if (obj.activeSelf) return;
            }
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void PauseGame()
    {
        isPaused = true;
        _pauseMenuUI.SetActive(true);

        _playerController.SetCanMove(false);
        _playerController.SetDialogueZoom(false);

        if(_umbrellaManChase.gameObject.activeSelf)
            _umbrellaManChase.PauseChase();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DialogueManager.instance?.SetPaused();
    }

    public void ResumeGame()
    {
        isPaused = false;
        _pauseMenuUI.SetActive(false);

        _playerController.SetCanMove(true);
        _playerController.SetDialogueZoom(true);

        if (_umbrellaManChase.gameObject.activeSelf)
            _umbrellaManChase.ResumeChase();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        DialogueManager.instance?.SetPaused();
        ResetPause();
    }

    public void ResetPause()
    {
        isPaused = false;
        _mainButtons.SetActive(true);
        _settings.SetActive(false);
        if(_credits != null)
            _credits.SetActive(false);
        _pauseMenuUI.SetActive(false);
    }
}
