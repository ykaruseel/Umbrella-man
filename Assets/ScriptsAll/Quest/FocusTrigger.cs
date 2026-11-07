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

    private PlayerController _playerController;

    
private void OnTriggerEnter(Collider other)
{
    TryActivateFromTrigger(other);
}

private void OnTriggerStay(Collider other)
{
    TryActivateFromTrigger(other);
}

private void TryActivateFromTrigger(Collider other)
{
    if (hasTriggered)
        return;

    // When a QuestTrigger exists, it controls
    // the order of focus and quest completion.
    QuestTrigger questTrigger =
        GetComponent<QuestTrigger>();

    if (questTrigger != null &&
        questTrigger.enabled)
    {
        return;
    }

    PlayerController player =
        other.GetComponentInParent<PlayerController>();

    if (player == null)
        return;

    if (!IsQuestActive(requiredQuestID))
        return;

    TriggerFocus(player);
}


    public void TriggerFocus(PlayerController playerController)
    {
        if (hasTriggered) return;
        _playerController = playerController;
        hasTriggered = true;
        if (TryGetComponent<OutlineInteractable>(out var outline))
        {
            outline.isBlocked = true;
        }
        StartCoroutine(OnTriggerEvent());
    }

    private IEnumerator OnTriggerEvent()
    {
        Pause.canPause = false;
        gameObject.GetComponent<Collider>().enabled = false;

        _playerController.isCinematic = true;
        _playerController.SetCanMove(false);
        _playerController.StartCinematicPan(focusObject.transform, 2f);
        _playerController.ZoomIn(focusPower);

        if (comments != null)
        {
            comments.StartDialogue();

            while (comments != null && comments.IsDialogueActive())
            {
                yield return null;
            }
        }

        _playerController.ZoomOut(focusPower);
        yield return new WaitForSeconds(1f);
        _playerController.isCinematic = false;
        _playerController.SetCanMove(true);

        enabled = false;
        Pause.canPause = true;
    }

    private bool IsQuestActive(string id)
{
    QuestManagerV2 manager =
        QuestManagerV2.Instance;

    return manager != null &&
           manager.IsQuestActive(id);
}
}
