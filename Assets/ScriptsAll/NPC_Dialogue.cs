using UnityEngine;
using UnityEngine;
using System.Collections;
using FMODUnity;

[DisallowMultipleComponent]
public class NPC_Dialogue : MonoBehaviour
{
    public DialogueLine[] dialogueLines;
    public GameObject interactionPrompt;

    [Header("FMOD")]
    [SerializeField] private EventReference knockSound;

    [Header("Настройки Камер Диалога (Cinematic Cameras)")]
    [Tooltip("Основная камера игрока (которую нужно выключить на время разговора)")]
    public GameObject mainPlayerCamera;
    
    [Tooltip("Перетащи сюда от 1 до 3 камер, которые расставил в сцене")]
    public GameObject[] dialogCameras;
    
    [Tooltip("Менять камеру каждые X реплик (1 = каждую фразу, 2 = через одну и т.д.)")]
    public int switchFrequency = 1;

    private int currentCameraIndex = 0;
    private int linesSinceLastSwitch = 0;
    private bool useCinematicCameras = false;

    private DialogueManager dialogueManager;
    private QuestManager questManager;
    private PlayerController playerController;
    private bool dialogueTriggered = false;

    string npcID;

    void Start()
    {
        npcID = gameObject.name;

        dialogueManager = FindFirstObjectByType<DialogueManager>();
        questManager = QuestManager.instance;
        playerController = FindFirstObjectByType<PlayerController>();

        
        if (dialogCameras != null && dialogCameras.Length > 0)
        {
            useCinematicCameras = true;
            
            foreach (var cam in dialogCameras)
            {
                if (cam != null) cam.SetActive(false);
            }
        }
    }

    public void PlayKnock()
    {
        if (!knockSound.IsNull)
            RuntimeManager.PlayOneShotAttached(knockSound, gameObject);
    }

    public void TriggerDialogue()
    {
        //if (questManager == null || questManager.currentQuest == null)
        //    return;

        //QuestObjective objective = questManager.currentQuest.GetCurrentObjective();
        //if (objective == null)
        //    return;

        //if (objective.targetID != "door" || objective.objectiveType != ObjectiveType.Interact)
        //    return;

        if (!QuestManagerV2.Instance.IsGoalRequired(npcID, GoalType.TalkToNPC))
        {
            return;
        }

        if (dialogueTriggered)
            return;

        dialogueTriggered = true;

        questManager.SendMessage("StopMusicForDialogue", SendMessageOptions.DontRequireReceiver);

        if (playerController)
        {
            playerController.SetCanMove(false);
            
            
            if (!useCinematicCameras)
            {
                playerController.SetDialogueZoom(true);
                playerController.ZoomIn();
            }
        }

        if (dialogueManager != null)
        {
            dialogueManager.StartDialogue(dialogueLines);
            StartCoroutine(HandleDialogueSequence());
        }
    }

    private IEnumerator HandleDialogueSequence()
    {
        
        if (useCinematicCameras) StartDialogCameras();

        while (dialogueManager != null && dialogueManager.IsDialogueActive())
        {
            
            if (useCinematicCameras && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E)))
            {
                AdvanceCamera();
            }
            yield return null;
        }

        
        if (useCinematicCameras) EndDialogCameras();

        if (playerController)
        {
            if (!useCinematicCameras)
            {
                playerController.ZoomOut();
                playerController.SetDialogueZoom(false);
            }
            playerController.SetCanMove(true);
        }

        if (QuestManagerV2.Instance.IsGoalRequired(npcID, GoalType.TalkToNPC))
        {
            QuestManagerV2.Instance.ProcessAction(npcID, GoalType.TalkToNPC);
        }

        if (questManager != null)
        {
            questManager.UpdateQuestProgress("door", ObjectiveType.Interact);
            questManager.ForceUpdateQuestText("Follow the light");
        }
    }

    

    private void StartDialogCameras()
    {
        if (mainPlayerCamera != null) mainPlayerCamera.SetActive(false);
        
        currentCameraIndex = 0;
        linesSinceLastSwitch = 0;
        
        
        if (dialogCameras[0] != null) dialogCameras[0].SetActive(true);
    }

    private void AdvanceCamera()
    {
        if (dialogCameras.Length <= 1) return;

        linesSinceLastSwitch++;

        
        if (linesSinceLastSwitch >= switchFrequency)
        {
            
            if (dialogCameras[currentCameraIndex] != null)
                dialogCameras[currentCameraIndex].SetActive(false);

            
            currentCameraIndex++;
            if (currentCameraIndex >= dialogCameras.Length)
                currentCameraIndex = 0; 

            
            if (dialogCameras[currentCameraIndex] != null)
                dialogCameras[currentCameraIndex].SetActive(true);

            
            linesSinceLastSwitch = 0;
        }
    }

    private void EndDialogCameras()
    {
        // Выключаем все режиссерские камеры
        foreach (var cam in dialogCameras) 
        { 
            if (cam != null) cam.SetActive(false); 
        }
        
        // Включаем обратно камеру из глаз игрока
        if (mainPlayerCamera != null) mainPlayerCamera.SetActive(true);
    }
}
