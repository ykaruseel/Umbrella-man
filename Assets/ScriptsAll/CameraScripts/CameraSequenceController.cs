using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CameraSequenceController : MonoBehaviour
{
    [SerializeField] private Camera cam1;
    [SerializeField] private Camera cam2;
    [SerializeField] private Camera cam3;
    [SerializeField] private Camera cam4;

    [SerializeField] private Camera cam5;

    [SerializeField] private CameraFade fade;

    public IntroText introText;

    private CameraPathFly fly1;
    private CameraPathFly fly2;
    private CameraPathFly fly3;

    [SerializeField] private PlayerController playerController;

    [SerializeField] private float defaultFOV = 50f;

    private const string FOV = "CameraFOV";

    private bool intro;


    void Start()
    {
        intro = true;
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetVolumeImmediate(0f);
        }

        playerController.isCinematic = true;
        playerController.SetCanMove(false);    

        float savedFOV = PlayerPrefs.GetFloat(FOV, defaultFOV);

        cam1.fieldOfView = savedFOV;
        cam2.fieldOfView = savedFOV;
        cam5.fieldOfView = savedFOV;

        fly1 = cam1.GetComponent<CameraPathFly>();
        fly2 = cam2.GetComponent<CameraPathFly>();
        fly3 = cam5.GetComponent<CameraPathFly>();

        TutorialManager.Instance.ShowHint(HintType.Move);

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

        TutorialManager.Instance.ShowHint(HintType.ViewQuest);

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
            //QuestManager.instance.StartFirstQuest();
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.EnsureMusicPlaying();
            MusicManager.Instance.FadeToVolume(1f, 1.2f);
        }

        //fade.SetFadeImageActive(false);
        intro = false;
        yield return fade.FadeIn();
    }

    void DisableAllCameras()
    {
        cam1.gameObject.SetActive(false);
        cam2.gameObject.SetActive(false);
        cam3.gameObject.SetActive(false);
        cam4.gameObject.SetActive(false);
        cam5.gameObject.SetActive(false);
    }

    //dla debuga

    private void Update()
    {
        if (intro && Input.GetKeyDown(KeyCode.Space))
        {
            Skip();
        }
    }

    public void StartThirdAnim()
    {
        DisableAllCameras();

        cam5.gameObject.SetActive(true);
        StartCoroutine(fade.FadeIn());

        fly3.OnPathFinished += OnThirdFinished;
    }

    private void OnThirdFinished()
    {
        fly3.OnPathFinished -= OnThirdFinished;
        StartCoroutine(ThirdAnimFinish());
    }

    private IEnumerator ThirdAnimFinish()
    {
        yield return fade.FadeOut();
        
        //cam5.gameObject.SetActive(false);

        cam5.enabled = false;

        cam3.gameObject.SetActive(true);
        cam4.gameObject.SetActive(true);

        yield return fade.FadeIn();

        yield return cam5.GetComponent<IntroText>().SequenceRoutine(0f);

        playerController.isCinematic = false;
        playerController.SetCanMove(true);

    }

    private void Skip()
    {
        StopAllCoroutines();
        intro = false;

        fade.SetFadeAlpha(0f);
        fade.SetFadeImageActive(false);

        cam1.gameObject.SetActive(false);
        cam2.gameObject.SetActive(false);

        cam3.gameObject.SetActive(true);
        cam4.gameObject.SetActive(true);


        playerController.isCinematic = false;
        playerController.SetCanMove(true);

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.EnsureMusicPlaying();
            MusicManager.Instance.FadeToVolume(1f, 1.2f);
        }
    }
}
