using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class QuestEvents : MonoBehaviour
{
    // Versia prototip, perenesti vse eventy siuda potom i dielat vizov iz QMV2 czerez switch i ID kvesta
    public List<GameObject> objectsToEnable;

    public List<GameObject> objectsToDisable;

    public GameObject tvToEndable;

    public GameObject tvToDisable;

    public PlayerController player;

    public GameObject knifeMan;

    public GameObject umbrellaMan;

    public NPC_Dialogue knifeManDialogue;

    public Camera cinematicCamera1;
    public Camera cinematicCamera2;

    public Camera playerCamera;

    public Transform retreatPoint;

    public float waitTimeBeforeStop = 5f;

    public static QuestEvents Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void VremennoQ3()
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
    }


    public void VremennoQ5()
    {
        tvToEndable.SetActive(true);
        tvToDisable.SetActive(false);
    }

    public IEnumerator VremennoQ9()
    {
        player.SetCanMove(false);

        player.isCinematic = true;

        knifeMan.SetActive(true);

        playerCamera.gameObject.SetActive(true);
        cinematicCamera1.gameObject.SetActive(false);
        cinematicCamera2.gameObject.SetActive(false);



        yield return new WaitForSeconds(waitTimeBeforeStop);

        
        // Здесь можно вызвать метод поворота в твоем PlayerController

        CharacterController cc = player.transform.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        player.transform.rotation = Quaternion.Euler(0f, 48.611f, 0f);
        player.SetRotation(48.611f, 0f);

        float elapsed = 0;
        Vector3 startPos = player.transform.position;
        while (elapsed < 1f)
        {
            player.transform.position = Vector3.Lerp(startPos, retreatPoint.position, elapsed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cinematicCamera1.gameObject.SetActive(true);
        cinematicCamera2.gameObject.SetActive(true);
        playerCamera.gameObject.SetActive(false);

        knifeManDialogue.TriggerDialogue();
    }

    public void VremennoQ10()
    {
        CharacterController cc = player.transform.GetComponent<CharacterController>();
        if (cc) cc.enabled = true;
        player.SetCanMove(true);
        player.isCinematic = false;


        knifeManDialogue.gameObject.GetComponent<KnifeManAI>().StartChase();
    }

    public void VremennoQ11() 
    {
        knifeMan.SetActive(false);

        umbrellaMan.SetActive(true);

        player.SetCanMove(false);

        player.isCinematic = true;

        Debug.Log("Конец прототипа");
    }
}
