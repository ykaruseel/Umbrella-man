using UnityEngine;
using Unity.Cinemachine;

public class DialogueCameraSystem : MonoBehaviour
{
    [Header("Player Camera (Standard Perspective)")]
    public CinemachineCamera playerCamera;

    [Header("Character Cameras (0 = Daniel, 1 = Lester)")]
    public CinemachineCamera[] characterCameras = new CinemachineCamera[2];

    [Header("B-Roll Cameras (Random Shots)")]
    public CinemachineCamera[] bRollCameras;

    [Header("Randomness Settings")]
    [Range(0, 100)] public int bRollChancePercent = 30;
    public int minLinesBetweenBRoll = 2;

    private int linesSinceLastBRoll = 0;
    private bool isDialogueActive = false;

    private void Start()
    {
        TurnOffAllCameras();
    }

    public void StartDialogue()
    {
        isDialogueActive = true;
        linesSinceLastBRoll = minLinesBetweenBRoll; 
        if (playerCamera != null) playerCamera.Priority = 0;
    }

    public void NextLine(string speakerName)
    {
        if (!isDialogueActive) return;

        TurnOffAllCameras();
        linesSinceLastBRoll++;

        // Логика перебивки (Третья камера)
        if (bRollCameras.Length > 0 && linesSinceLastBRoll >= minLinesBetweenBRoll)
        {
            int roll = Random.Range(0, 100);
            if (roll < bRollChancePercent)
            {
                int randomBRoll = Random.Range(0, bRollCameras.Length);
                if (bRollCameras[randomBRoll] != null)
                {
                    bRollCameras[randomBRoll].Priority = 100;
                    linesSinceLastBRoll = 0; 
                    return; 
                }
            }
        }

        // Логика основных камер
        if (speakerName == "Daniel" && characterCameras[0] != null)
        {
            characterCameras[0].Priority = 100;
        }
        else if (speakerName == "Lester" && characterCameras[1] != null)
        {
            characterCameras[1].Priority = 100;
        }
        else 
        {
            if (characterCameras[0] != null) characterCameras[0].Priority = 100;
        }
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        TurnOffAllCameras();
        if (playerCamera != null) playerCamera.Priority = 100;
    }

    private void TurnOffAllCameras()
    {
        if (characterCameras != null)
        {
            foreach (var cam in characterCameras)
            {
                if (cam != null) cam.Priority = 0;
            }
        }
        
        if (bRollCameras != null)
        {
            foreach (var cam in bRollCameras)
            {
                if (cam != null) cam.Priority = 0;
            }
        }
    }
}
