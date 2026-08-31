using UnityEngine;
using System.Collections;

public class PlaceholderDialogueTrigger : MonoBehaviour
{
    [Header("Ссылка на систему камер (DialogueCameraSystem)")]
    public DialogueCameraSystem cameraSystem;

    private bool hasTriggered = false;

    
    public void StartDialogueSequence()
    {
        if (!hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(RunTestDialogue());
        }
    }

    private IEnumerator RunTestDialogue()
    {
        yield return new WaitForSeconds(7f);

        if (cameraSystem != null) cameraSystem.StartDialogue();

        yield return new WaitForSeconds(3f);

        
        if (cameraSystem != null) cameraSystem.NextLine("Lester");

        yield return new WaitForSeconds(3f);

        if (cameraSystem != null) cameraSystem.EndDialogue();
        
        hasTriggered = false; 
    }
}
