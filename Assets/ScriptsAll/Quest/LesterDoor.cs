using FMODUnity;
using System.Collections;
using UnityEngine;

public class LesterDoor : MonoBehaviour
{
    public PlayerController player;
    public Transform retreatPoint;

    [SerializeField] private Transform door;

    [SerializeField] private EventReference knockSound;
    [SerializeField] private StudioEventEmitter emitter;

    [SerializeField] private NPC_Dialogue lester;

    
    [Header("Система Камер")]
    public DialogueCameraSystem cameraSystem;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    public void Interact()
    {
        gameObject.tag = "Untagged";
        closedRotation = door.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0, -90, 0);
        
        
        StartCoroutine(OpenDoor());
    }

    private IEnumerator OpenDoor()
    {
        player.SetCanMove(false);
        player.isCinematic = true;

        CharacterController cc = player.transform.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        player.SetRotation(0f, 0f);

        float elapsed = 0;
        Vector3 startPos = player.transform.position;
        while (elapsed < 1.5f)
        {
            player.transform.position = Vector3.Lerp(startPos, retreatPoint.position, elapsed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!knockSound.IsNull) 
        {
            RuntimeManager.PlayOneShotAttached(knockSound, gameObject);
        }

        yield return new WaitForSeconds(2f);

        var instance = emitter.EventInstance;

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
            emitter.Stop();
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 1f;
            door.localRotation = Quaternion.Slerp(closedRotation, openRotation, t);
            yield return null;
        }
        door.localRotation = openRotation;

        
        if (cameraSystem != null)
        {
            cameraSystem.StartDialogue();
            QuestEvents.Instance.DanielsModel.SetActive(true);
        }

        if (lester != null)
        {
            lester.TriggerDialogue();
        }
    }
}
