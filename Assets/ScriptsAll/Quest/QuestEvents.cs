using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class QuestEvents : MonoBehaviour
{
    [Header("Quest 3")]
    public List<GameObject> objectsToEnable;

    public List<GameObject> objectsToDisable;

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

    public void QuestEvent11() 
    {
        knifeMan.SetActive(false);

        umbrellaMan.SetActive(true);

        player.SetCanMove(false);

        player.isCinematic = true;

        Debug.Log("Конец прототипа");
    }
}
