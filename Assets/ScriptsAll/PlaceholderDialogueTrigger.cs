using UnityEngine;
using System.Collections;

public class PlaceholderDialogueTrigger : MonoBehaviour
{
    [Header("Ссылка на систему камер (DialogueCameraSystem)")]
    public DialogueCameraSystem cameraSystem;

    private bool hasTriggered = false;

    // Эту функцию вызывает дверь при нажатии "E"
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

        
        if (cameraSystem != null) cameraSystem.NextLine();

        
        yield return new WaitForSeconds(3f);

        
        if (cameraSystem != null) cameraSystem.EndDialogue();
        
        
        hasTriggered = false; 
    }
}
