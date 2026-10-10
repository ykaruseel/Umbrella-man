using System.Collections;
using UnityEngine;

public class TakeOutTrash2Controller : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField]
    private string requiredQuestID =
        "Q_TAKE_OUT_TRASH_2";


    [Header("References")]
    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private PlayerComments introComment;


    private bool introPlayed = false;


    public IEnumerator PlayIntroRoutine()
    {
        if (introPlayed)
            yield break;


        // QuestManager может переключить quest
        // только на следующем кадре.
        float timeout = 1f;


        while (
            QuestManagerV2.Instance != null &&
            !QuestManagerV2.Instance.IsQuestActive(
                requiredQuestID
            ) &&
            timeout > 0f
        )
        {
            timeout -= Time.deltaTime;
            yield return null;
        }


        if (QuestManagerV2.Instance == null ||
            !QuestManagerV2.Instance.IsQuestActive(
                requiredQuestID
            ))
        {
            Debug.LogWarning(
                "[TakeOutTrash2] " +
                "Q_TAKE_OUT_TRASH_2 is not active."
            );

            RestoreControl();
            yield break;
        }


        introPlayed = true;


        Pause.canPause = false;


        if (playerController != null)
        {
            playerController.SetCanMove(false);
            playerController.isCinematic = true;
        }


        if (introComment != null)
        {
            introComment.StartDialogue();


            while (introComment.IsDialogueActive())
            {
                yield return null;
            }
        }


        RestoreControl();
    }


    public void StartIntro()
    {
        if (introPlayed)
            return;


        StartCoroutine(
            PlayIntroRoutine()
        );
    }


    private void RestoreControl()
    {
        Debug.Log(
            "[TakeOutTrash2] Player control restored."
        );

        if (playerController != null)
        {
            playerController.isCinematic = false;
            playerController.SetCanMove(true);
        }

        Pause.canPause = true;
    }
}