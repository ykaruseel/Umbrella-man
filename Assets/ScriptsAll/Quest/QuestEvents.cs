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
    [Header("Quest 3")]
    public List<GameObject> objectsToEnable;

    public List<GameObject> objectsToDisable;

    public GameObject lesterDoor;

    [Header("Quest 5")]
    public GameObject tvToEndable;

    public GameObject tvToDisable;

    [Header("Quest 7")]
    public List<Light> lightsToDisable;

    [Header("Quest 9-11")]
    public PlayerController player;

    public GameObject knifeMan;

    public GameObject umbrellaMan;

    public NPC_Dialogue knifeManDialogue;

    public CameraFade cameraFade;

    public Camera playerCamera1;
    public Camera playerCamera2;

    public Camera cinematicCamera;

    public Transform retreatPoint;

    public static QuestEvents Instance;

    public List<DoorController> doors;

    [Header("Quest 11")]
    public PlayerComments comments;

    public Light flickerLight;

    public Volume postProcessVolume;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator QuestEvent3()
    {
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

        if (player)
        {
            CharacterController cc = player.transform.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;

            player.SetCanMove(false);
            player.isCinematic = true;

            StartCoroutine(cameraFade.FadeOut());

            yield return new WaitForSeconds(1.25f);

            player.transform.position = new Vector3(-22.77f, -8.31f, -1.53f);
            player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            player.SetRotation(180f, 0f);

            yield return new WaitForSeconds(0.25f);

            StartCoroutine(cameraFade.FadeIn());

            yield return new WaitForSeconds(1f);

            player.SetCanMove(true);
            player.isCinematic = false;
            if (cc) cc.enabled = true;
            player.enabled = true;
        }
    }


    public void QuestEvent5()
    {
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.tag = "Pickable";
        }

        tvToEndable.SetActive(true);
        tvToDisable.SetActive(false);
    }

    public void QuestEvent7()
    {
        foreach (Light light in lightsToDisable)
        {
            if (light != null)
                light.enabled = false;
        }
    }

    public IEnumerator QuestEvent9()
    {
        player.SetCanMove(false);

        player.isCinematic = true;


        StartCoroutine(cameraFade.FadeOut());

        yield return new WaitForSeconds(1.25f);

        cinematicCamera.gameObject.SetActive(true);
        playerCamera1.gameObject.SetActive(false);
        playerCamera2.gameObject.SetActive(false);
        knifeMan.SetActive(true);

        yield return new WaitForSeconds(0.25f);

        StartCoroutine(cameraFade.FadeIn());

        CharacterController cc = player.transform.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        player.transform.rotation = Quaternion.Euler(0f, -0.853f, 0f);
        player.SetRotation(-0.853f, 0f);

        float elapsed = 0;
        Vector3 startPos = player.transform.position;
        while (elapsed < 2.5f)
        {
            player.transform.position = Vector3.Lerp(startPos, retreatPoint.position, elapsed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(cameraFade.FadeOut());

        yield return new WaitForSeconds(1.25f);

        playerCamera1.gameObject.SetActive(true);
        playerCamera2.gameObject.SetActive(true);
        cinematicCamera.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.25f);

        StartCoroutine(cameraFade.FadeIn());

        yield return new WaitForSeconds(1.25f);

        knifeManDialogue.TriggerDialogue();
    }

    public IEnumerator QuestEvent10()
    {
        CharacterController cc = player.transform.GetComponent<CharacterController>();
        if (cc) cc.enabled = true;
        player.SetCanMove(true);
        player.isCinematic = false;

        yield return new WaitForSeconds(2f);

        knifeManDialogue.gameObject.GetComponent<KnifeManAI>().StartChasing();

        foreach (DoorController door in doors)
        {
            if (door != null)
                door.isLockedWithQTE = true;
        }
    }

    public IEnumerator QuestEvent11()
    {
        knifeMan.SetActive(false);

        umbrellaMan.SetActive(true);

        player.SetCanMove(false);

        player.isCinematic = true;

        flickerLight.enabled = true;

        player.StartCinematicPan(umbrellaMan.transform, 2f);

        yield return new WaitForSeconds(2f);

        StartCoroutine(LightFlickerRoutine());

        StartCoroutine(SmoothPostProcess(2f));

        player.ZoomIn(0.4f);

        yield return new WaitForSeconds(2f);

        StartCoroutine(cameraFade.FadeOut());

        yield return new WaitForSeconds(1.25f);

        flickerLight.enabled = false;

        if (comments != null)
        {
            comments.StartDialogue();

            while (comments != null && comments.IsDialogueActive())
            {
                yield return null;
            }
        }
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
