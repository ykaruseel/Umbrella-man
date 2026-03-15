using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CameraSequenceController : MonoBehaviour
{
    [SerializeField] private Camera cam1;
    [SerializeField] private Camera cam2;
    [SerializeField] private Camera cam3;
    [SerializeField] private Camera cam4;

    [SerializeField] private CameraFade fade;

    private CameraPathFly fly1;
    private CameraPathFly fly2;

    [SerializeField] private PlayerController playerController;

    [SerializeField] private float defaultFOV = 50f;

    private const string FOV = "CameraFOV";

    void Start()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetVolumeImmediate(0f);
        }

        playerController.isCinematic = true;
        playerController.SetCanMove(false);

        fade.SetFadeImageActive(true);

        float savedFOV = PlayerPrefs.GetFloat(FOV, defaultFOV);

        cam1.fieldOfView = savedFOV;
        cam2.fieldOfView = savedFOV;

        fly1 = cam1.GetComponent<CameraPathFly>();
        fly2 = cam2.GetComponent<CameraPathFly>();

        DisableAllCameras();

        cam1.gameObject.SetActive(true);
        fly1.OnPathFinished += OnFirstFinished;

        StartCoroutine(fade.FadeIn());
    }

    void OnFirstFinished()
    {
        fly1.OnPathFinished -= OnFirstFinished;
        StartCoroutine(SwitchToSecond());
    }

    IEnumerator SwitchToSecond()
    {
        yield return fade.FadeOut();

        cam1.gameObject.SetActive(false);
        cam2.gameObject.SetActive(true);

        fly2.OnPathFinished += OnSecondFinished;

        yield return fade.FadeIn();
    }

    void OnSecondFinished()
    {
        fly2.OnPathFinished -= OnSecondFinished;
        StartCoroutine(SwitchToFinal());
    }

    IEnumerator SwitchToFinal()
    {
        yield return fade.FadeOut();

        cam2.gameObject.SetActive(false);

        cam3.gameObject.SetActive(true);
        cam4.gameObject.SetActive(true);

        playerController.isCinematic = false;
        playerController.SetCanMove(true);

        if (QuestManager.instance != null)
        {
            QuestManager.instance.StartFirstQuest();
        }


        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.StartTutorial();
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.EnsureMusicPlaying();
            MusicManager.Instance.FadeToVolume(1f, 1.2f);
        }


        fade.SetFadeImageActive(false);

        yield return fade.FadeIn();
    }

    void DisableAllCameras()
    {
        cam1.gameObject.SetActive(false);
        cam2.gameObject.SetActive(false);
        cam3.gameObject.SetActive(false);
        cam4.gameObject.SetActive(false);
    }
}
