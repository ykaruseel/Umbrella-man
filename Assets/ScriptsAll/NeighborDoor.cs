using FMOD.Studio;
using FMODUnity;
using System.Collections;
using UnityEngine;

public class NeighborDoor : MonoBehaviour
{
    public bool hasTriggered = false;

    public GameObject focusObject;

    [SerializeField] private EventReference knockSound;

    private PlayerController player;

    [Range(0.1f, 1f)]
    [SerializeField] private float focusPower = 0.9f;

    private PlayerComments comments;
    public void Interact(PlayerController playerController)
    {
        if (hasTriggered) return;

        player = playerController;

        comments = GetComponent<PlayerComments>();

        player.SetCanMove(false);
        player.isCinematic = true;
      
        hasTriggered = true;

        if (TryGetComponent<OutlineInteractable>(out var outline))
        {
            outline.isBlocked = true;
        }

        StartCoroutine(OnTriggerEvent());
    }

    private IEnumerator OnTriggerEvent()
    {
        gameObject.GetComponent<Collider>().enabled = false;

        player.isCinematic = true;
        player.SetCanMove(false);
        player.StartCinematicPan(focusObject.transform, 2f);
        player.ZoomIn(focusPower);

        if (!knockSound.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(knockSound, gameObject);
        }

        yield return new WaitForSeconds(2f);    

        if (comments != null)
        {
            comments.StartDialogue();

            while (comments != null && comments.IsDialogueActive())
            {
                yield return null;
            }
        }

        player.ZoomOut(focusPower);
        yield return new WaitForSeconds(1f);
        player.isCinematic = false;
        player.SetCanMove(true);

        enabled = false;
    }
}
