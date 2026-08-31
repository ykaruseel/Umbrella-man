using FMODUnity;
using FMOD.Studio;
using System.Collections;
using UnityEngine;

public class LesterDoor : MonoBehaviour
{
    public PlayerController player;
    public Transform retreatPoint;

    [SerializeField] private Transform door;

    [Header("FMOD")]
    [SerializeField] private EventReference knockSound;
    [SerializeField] private EventReference doorSoundEvent;

    private EventInstance doorSoundInstance;

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

        if (cc)
            cc.enabled = false;

        player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        player.SetRotation(0f, 0f);

        float elapsed = 0f;

        Vector3 startPos = player.transform.position;

        while (elapsed < 1.5f)
        {
            player.transform.position = Vector3.Lerp(
                startPos,
                retreatPoint.position,
                elapsed / 1.5f
            );

            elapsed += Time.deltaTime;

            yield return null;
        }

        player.transform.position = retreatPoint.position;

        if (!knockSound.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(knockSound, gameObject);
        }

        yield return new WaitForSeconds(2f);

        if (!doorSoundEvent.IsNull)
        {
            doorSoundInstance = RuntimeManager.CreateInstance(doorSoundEvent);

            RuntimeManager.AttachInstanceToGameObject(
                doorSoundInstance,
                door
            );

            doorSoundInstance.start();

            doorSoundInstance.setParameterByName("Door", 0f);

            yield return new WaitForSeconds(0.15f);

            doorSoundInstance.setParameterByName("Door", 1f);
        }

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / 1f;

            door.localRotation = Quaternion.Slerp(
                closedRotation,
                openRotation,
                t
            );

            yield return null;
        }

        door.localRotation = openRotation;

        if (doorSoundInstance.isValid())
        {
            doorSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            doorSoundInstance.release();
        }

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