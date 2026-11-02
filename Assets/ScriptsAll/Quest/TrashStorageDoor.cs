
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class TrashStorageDoor : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField]
    private string partOneQuestID = "Q_TAKE_OUT_TRASH_1";

    [SerializeField]
    private string partTwoQuestID = "Q_TAKE_OUT_TRASH_2";

    [Header("Door")]
    [SerializeField]
    private Transform doorPivot;

    [SerializeField]
    private Vector3 openEuler = new Vector3(0f, 95f, 0f);

    [SerializeField]
    private float openDuration = 0.7f;

    [Header("UI")]
    [SerializeField]
    private InteractionMessageUI messageUI;

    [Header("Highlight")]
    [SerializeField]
    private OutlineInteractable outline;

    [Header("FMOD")]
    [SerializeField]
    private EventReference metalDoorAudio;

    [SerializeField]
    private string doorParameterName = "Door";

    [SerializeField]
    private float squeakValue = 1f;

    private Quaternion closedRotation;

    private bool isUnlocked;
    private bool isOpening;
    private bool isOpened;
    private bool keyUsedMessageShown;

    public bool IsUnlocked => isUnlocked;

    public bool IsOpened => isOpened;

    public bool CanInteract
    {
        get
        {
            if (isOpening || isOpened)
                return false;

            if (QuestManagerV2.Instance == null)
                return false;

            return
                QuestManagerV2.Instance.IsQuestActive(partOneQuestID)
                ||
                QuestManagerV2.Instance.IsQuestActive(partTwoQuestID);
        }
    }

    private void Awake()
    {
        if (outline == null)
            outline = GetComponent<OutlineInteractable>();

        if (doorPivot == null)
            doorPivot = transform;

        closedRotation = doorPivot.localRotation;
    }

    public bool TryInteract()
    {
        Debug.Log(
            $"[TrashStorageDoor] TryInteract. CanInteract = {CanInteract}, Unlocked = {isUnlocked}"
        );

        if (!CanInteract)
            return false;

        if (!isUnlocked)
        {
            if (messageUI != null)
                messageUI.ShowMessage("Closed");

            return true;
        }

        StartCoroutine(OpenRoutine());

        return true;
    }

    public void Unlock()
    {
        isUnlocked = true;

        Debug.Log("[TrashStorageDoor] Unlocked.");
    }

    private IEnumerator OpenRoutine()
    {
        if (isOpening || isOpened)
            yield break;

        isOpening = true;

        if (outline != null)
        {
            outline.Hide();
            outline.isBlocked = true;
        }

        if (!keyUsedMessageShown)
        {
            keyUsedMessageShown = true;

            if (messageUI != null)
                messageUI.ShowMessage("Trash storage key used");
        }

        PlayMetalDoorSqueak();

        Quaternion startRotation = doorPivot.localRotation;

        Quaternion targetRotation =
            closedRotation * Quaternion.Euler(openEuler);

        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / openDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            doorPivot.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                smoothT
            );

            yield return null;
        }

        doorPivot.localRotation = targetRotation;

        isOpened = true;
        isOpening = false;
    }

    private void PlayMetalDoorSqueak()
    {
        if (metalDoorAudio.IsNull)
            return;

        EventInstance instance =
            RuntimeManager.CreateInstance(metalDoorAudio);

        RuntimeManager.AttachInstanceToGameObject(
            instance,
            doorPivot
        );

        instance.setParameterByName(
            doorParameterName,
            squeakValue
        );

        instance.start();
        instance.release();
    }
}