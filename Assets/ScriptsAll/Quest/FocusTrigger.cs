using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FocusTrigger : MonoBehaviour
{
    public string requiredQuestID;

    public bool hasTriggered = false;

    public GameObject focusObject;

    [Range(0.1f,1f)]
    [SerializeField] private float focusPower = 0.8f;

    public PlayerComments comments;

    private PlayerController playerController;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player") && IsQuestActive(requiredQuestID))
        {
            other.TryGetComponent(out playerController);
            hasTriggered = true;
            StartCoroutine(OnTriggerEvent());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player") && IsQuestActive(requiredQuestID))
        {
            other.TryGetComponent(out playerController);
            hasTriggered = true;
            StartCoroutine(OnTriggerEvent());
        }
    }

    private IEnumerator OnTriggerEvent()
    {
        gameObject.GetComponent<Collider>().enabled = false;

        playerController.isCinematic = true;
        playerController.SetCanMove(false);
        playerController.StartCinematicPan(focusObject.transform, 2f);
        playerController.ZoomIn(focusPower);

        if (comments != null)
        {
            comments.StartDialogue();

            while (comments != null && comments.IsDialogueActive())
            {
                yield return null;
            }
        }

        playerController.ZoomOut(focusPower);
        yield return new WaitForSeconds(1f);
        playerController.isCinematic = false;
        playerController.SetCanMove(true);

        enabled = false;
    }

    private bool IsQuestActive(string id)
    {
        return QuestManagerV2.Instance.IsQuestActive(id);
    }
}
