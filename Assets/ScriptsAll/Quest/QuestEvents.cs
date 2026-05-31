using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class QuestEvents : MonoBehaviour
{

    [Header("Jumpscares")]
    public GameObject pictureJumpscare;
    public GameObject shadowJumpscare;
    public GameObject bulbJumpscare;
    public GameObject doorJumpscare;


    [Header("Soft Boundaries")]
    public GameObject lesterStairsBlock;
    public GameObject basementBlock;

    [Header("Quest 3")]
    public List<GameObject> objectsToEnable;

    public List<GameObject> objectsToDisable;

    public GameObject lesterDoor;

    public StudioEventEmitter emitterLester;

    public DoorController doorController;

    public CameraSequenceController cameraSequenceController;

    public MeshRenderer entranceDoorMR;

    public GameObject brokenDoor;

    [Header("Quest 5")]
    public GameObject tvToEndable;

    public GameObject tvToDisable;

    public List<PulseHighlight> pulseHighlights;

    public List<OutlineInteractable> outlineInteractables;

    [Header("Quest 7")]
    public List<Light> lightsToDisable;

    public PulseHighlight pulseHighlightsToEnable;

    [Header("Quest 9-11")]
    [SerializeField] private FMODUnity.EventReference umbrellaAppearSound;
    public PlayerController player;

    public Light knifemanLight;

    public StudioEventEmitter emitterKnifeman;

    public GameObject knifeMan;

    public GameObject umbrellaMan;

    public GameObject umbrellaManTarget;

    public NPC_Dialogue knifeManDialogue;

    public CameraFade cameraFade;

    public Camera playerCamera1;
    public Camera playerCamera2;

    public Camera cinematicCamera;

    public Transform retreatPoint;

    public static QuestEvents Instance;

    public List<DoorController> doors;

    public Flashlight flashlight;

    [Header("Quest 11")]
    public GameObject endGame;

    public Light flickerLight;

    public Volume postProcessVolume;

    public GameObject DanielsModel;

    private void Awake()
    {
        Instance = this;

        foreach (OutlineInteractable interactable in outlineInteractables)
        {
            if (interactable != null)
                interactable.isBlocked = true;
        }
    }

    public IEnumerator QuestEvent3()
    {
        Pause.canPause = false;
        if (lesterStairsBlock != null) lesterStairsBlock.SetActive(false);
        
        if (shadowJumpscare != null) shadowJumpscare.SetActive(true);

        MusicManagerv2.Instance.StartMusic();
        MusicManagerv2.Instance.SetMusicState(0);
        DanielsModel.SetActive(false);
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        lesterDoor.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        var instance = emitterLester.EventInstance;

        if (instance.isValid())
        {
            float startVolume;
            instance.getVolume(out startVolume);
            float currentTime = 0;

            while (currentTime < 2f)
            {
                currentTime += Time.deltaTime;
                float newVolume = Mathf.Lerp(startVolume, 0f, currentTime / 2f);
                instance.setVolume(newVolume);
                yield return null;
            }

            instance.setVolume(0f);
            emitterLester.Stop();
        }

        if (player)
        {
            CharacterController cc = player.transform.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;

            player.SetCanMove(false);
            player.isCinematic = true;

            StartCoroutine(cameraFade.FadeOut());

            yield return new WaitForSeconds(1.25f);

            player.transform.position = new Vector3(-41.4f, -10.17f, -36.6f);
            player.transform.rotation = Quaternion.Euler(0f, 40f, 0f);
            player.SetRotation(40f, 0f);

            if (cc) cc.enabled = true;
            player.enabled = true;

            cameraSequenceController.StartThirdAnim();
        }

        doorController.DoorEvent();

        entranceDoorMR.enabled = true;
        brokenDoor.SetActive(false);
    }


    public void QuestEvent5()
    {
        tvToEndable.SetActive(true);
        tvToDisable.SetActive(false);

        if (basementBlock != null) basementBlock.SetActive(false);

        if (pictureJumpscare != null) pictureJumpscare.SetActive(true);
        
        if (bulbJumpscare != null) bulbJumpscare.SetActive(true);
        if (doorJumpscare != null) doorJumpscare.SetActive(true);


        if (bulbJumpscare != null) bulbJumpscare.SetActive(true);
        if (doorJumpscare != null) doorJumpscare.SetActive(true);

        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.tag = "Pickable";
        }

        foreach (OutlineInteractable interactable in outlineInteractables)
        {
            if (interactable != null)
                interactable.isBlocked = false;
        }

        foreach (PulseHighlight highlight in pulseHighlights)
        {
            if (highlight != null)
                highlight.Show();
        }
    }

    public void QuestEvent7()
    {
        foreach (Light light in lightsToDisable)
        {
            if (light != null)
                light.enabled = false;
        }

        pulseHighlightsToEnable.Show();
        pulseHighlightsToEnable.transform.GetComponent<OutlineInteractable>().isBlocked = false;
    }

    public IEnumerator QuestEvent9()
    {
        Pause.canPause = false;

        player.SetCanMove(false);

        player.isCinematic = true;

        StartCoroutine(cameraFade.FadeOut());

        yield return new WaitForSeconds(1.25f);

        flashlight.SetBlocked(true);

        CharacterController cc = player.transform.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        player.SetRotation(180f, 0f);

        player.transform.position = retreatPoint.position;

        DanielsModel.SetActive(true);

        knifeMan.SetActive(true);

        cinematicCamera.gameObject.SetActive(true);
        playerCamera1.gameObject.SetActive(false);
        playerCamera2.gameObject.SetActive(false);
        knifeMan.transform.position = new Vector3(-13.768f, -13.51372f, -19.388f);

        GramophoneRotation.GramophoneIsPlaying = false;
        emitterKnifeman.Stop();

        StartCoroutine(cameraFade.FadeIn());

        yield return new WaitForSeconds(3f);

        StartCoroutine(cameraFade.FadeOut());

        yield return new WaitForSeconds(1.25f);

        knifeMan.transform.position = new Vector3(-13.905f, -13.51372f, -17.796f);
        player.transform.rotation = Quaternion.Euler(0f, -0.853f, 0f);
        player.SetRotation(-0.853f, 0f);

        playerCamera1.gameObject.SetActive(true);
        playerCamera2.gameObject.SetActive(true);
        cinematicCamera.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.25f);

        DanielsModel.SetActive(false);

        StartCoroutine(cameraFade.FadeIn());

        yield return new WaitForSeconds(1.25f);

        knifeManDialogue.TriggerDialogue();

        knifemanLight.enabled = true;
    }

    public IEnumerator QuestEvent10()
    {
        CharacterController cc = player.transform.GetComponent<CharacterController>();
        if (cc) cc.enabled = true;
        player.SetCanMove(true);
        player.isCinematic = false;
        flashlight.SetBlocked(false);
        knifemanLight.enabled = false;

        yield return new WaitForSeconds(2f);

        knifeManDialogue.gameObject.GetComponent<KnifeManAI>().StartChasing();

        foreach (DoorController door in doors)
        {
            if (door != null)
                door.isLockedWithQTE = true;
        }

        TutorialManager.Instance.ShowHint(HintType.Sprint);
    }

    public IEnumerator QuestEvent11()
    {
        Pause.canPause = false;
        knifeMan.SetActive(false);

        FMODUnity.RuntimeManager.PlayOneShot(umbrellaAppearSound);

        umbrellaMan.SetActive(true);

        player.SetCanMove(false);

        player.isCinematic = true;

        flickerLight.enabled = true;

        player.StartCinematicPan(umbrellaManTarget.transform, 2f);

        yield return new WaitForSeconds(2f);

        StartCoroutine(LightFlickerRoutine());

        StartCoroutine(SmoothPostProcess(6f));

        player.ZoomIn(0.5f, 4f);

        yield return new WaitForSeconds(6f);

        StartCoroutine(cameraFade.FadeOut());

        yield return new WaitForSeconds(1.25f);

        flickerLight.enabled = false;

        endGame.SetActive(true);
    }

    private IEnumerator LightFlickerRoutine()
    {
        if (flickerLight == null) yield break;

        flickerLight.enabled = true;

        float minDuration = 0.5f;
        float maxDuration = 1f;
        float decreaseFactor = 0.6f;

        float targetMin = 1.5f;
        float targetMax = 4f;

        while (flickerLight.enabled)
        {
            float startIntensity = flickerLight.intensity;
            float targetIntensity = Random.Range(targetMin, targetMax);

            float duration = Random.Range(minDuration, maxDuration);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                flickerLight.intensity = Mathf.Lerp(
                    startIntensity,
                    targetIntensity,
                    Mathf.SmoothStep(0f, 1f, t)
                );

                yield return null;
            }

            flickerLight.intensity = targetIntensity;

            minDuration *= decreaseFactor;
            maxDuration *= decreaseFactor;

            minDuration = Mathf.Max(minDuration, 0.05f);
            maxDuration = Mathf.Max(maxDuration, 0.1f);
        }
    }

    private IEnumerator SmoothPostProcess(float duration)
    {
        if (postProcessVolume == null || postProcessVolume.profile == null) yield break;

        postProcessVolume.profile.TryGet(out Vignette vignette);
        postProcessVolume.profile.TryGet(out ColorAdjustments colorAdjust);
        postProcessVolume.profile.TryGet(out LensDistortion lensDist);
        postProcessVolume.profile.TryGet(out ChromaticAberration chromAb);
        postProcessVolume.profile.TryGet(out FilmGrain grain);

        float startVignette = vignette != null ? vignette.intensity.value : 0;
        float startExposure = colorAdjust != null ? colorAdjust.postExposure.value : 0;
        float startContrast = colorAdjust != null ? colorAdjust.contrast.value : 0;
        float startDistortion = lensDist != null ? lensDist.intensity.value : 0;
        float startChrom = chromAb != null ? chromAb.intensity.value : 0;
        float startGrainInt = grain != null ? grain.intensity.value : 0;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float curve = Mathf.SmoothStep(0, 1, t);

            if (vignette != null) vignette.intensity.value = Mathf.Lerp(startVignette, 0.6f, curve);

            if (colorAdjust != null)
            {
                colorAdjust.postExposure.value = Mathf.Lerp(startExposure, 3f, curve);
                colorAdjust.contrast.value = Mathf.Lerp(startContrast, 70f, curve);
            }

            if (lensDist != null) lensDist.intensity.value = Mathf.Lerp(startDistortion, 0.5f, curve);
            if (chromAb != null) chromAb.intensity.value = Mathf.Lerp(startChrom, 1f, curve);

            if (grain != null)
            {
                grain.intensity.value = Mathf.Lerp(startGrainInt, 1f, curve);
                grain.response.value = Mathf.Lerp(grain.response.value, 0f, curve);
            }

            yield return null;
        }
    }
}
