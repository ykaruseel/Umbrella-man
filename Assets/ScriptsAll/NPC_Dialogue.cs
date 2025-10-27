// Assets/Scripts/NPC_Dialogue.cs
using UnityEngine;

[DisallowMultipleComponent]
public class NPC_Dialogue : MonoBehaviour
{
    [Header("Dialogue Data")]
    public DialogueLine[] dialogueLines;

    [Header("UI")]
    public GameObject interactionPrompt;
}
