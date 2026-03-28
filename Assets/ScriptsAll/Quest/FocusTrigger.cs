using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FocusTrigger : MonoBehaviour
{
    public string requiredQuestID;

    [SerializeField] private bool hasTriggered = false;

    public GameObject focusObject;

    public IntroText text;

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
        playerController.ZoomIn();

        if (text != null)
            StartCoroutine(text.SequenceRoutine(0f));

        yield return new WaitForSeconds(6.5f);
        //Change this time and introtext

        playerController.ZoomOut();
        playerController.isCinematic = false;
        playerController.SetCanMove(true);

        enabled = false;
    }

    private bool IsQuestActive(string id)
    {
        return QuestManagerV2.Instance.IsQuestActive(id);
    }
}
