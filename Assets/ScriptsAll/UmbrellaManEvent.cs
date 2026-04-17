using UnityEngine;
using System.Collections;

public class UmbrellaManEvent : MonoBehaviour
{
    [Header("Что и где спавним")]
    [Tooltip("Перетащи сюда выключенную модель тени")]
    public GameObject shadowPrefab; 
    
    [Tooltip("Перетащи сюда самого Игрока")]
    public Transform player; 
    
    [Tooltip("Дистанция появления за спиной")]
    public float spawnDistance = 3.0f; 
    
    [Tooltip("Шанс срабатывания в процентах")]
    [Range(0, 100)] public int spawnChance = 50; 

    [Header("Исчезновение")]
    [Tooltip("Камера игрока (от первого лица)")]
    public Camera mainCamera;
    
    [Tooltip("Сколько секунд висит перед исчезновением")]
    public float disappearDelay = 0.5f; 

    private bool eventTriggered = false;
    private bool isLooking = false;

    private void Start()
    {
        if (shadowPrefab != null) shadowPrefab.SetActive(false);
        if (mainCamera == null) mainCamera = Camera.main;
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (eventTriggered) return; 

        if (other.CompareTag("Player"))
        {
            

            
            int roll = Random.Range(1, 101);
            if (roll <= spawnChance)
            {
                eventTriggered = true;
                SpawnShadow();
            }
        }
    }

    private void SpawnShadow()
    {
        
        Vector3 spawnPos = player.position - (player.forward * spawnDistance);
        
        
        spawnPos.y = player.position.y;

        shadowPrefab.transform.position = spawnPos;

        
        Vector3 lookDir = player.position - shadowPrefab.transform.position;
        lookDir.y = 0;
        shadowPrefab.transform.rotation = Quaternion.LookRotation(lookDir);

        
        shadowPrefab.SetActive(true);
        
        
        StartCoroutine(CheckVisibilityRoutine());
    }

    private IEnumerator CheckVisibilityRoutine()
    {
        while (!isLooking)
        {
            
            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(shadowPrefab.transform.position);
            
            
            if (viewportPoint.z > 0 && viewportPoint.x > 0.1f && viewportPoint.x < 0.9f && viewportPoint.y > 0.1f && viewportPoint.y < 0.9f)
            {
                isLooking = true;
                
                
                yield return new WaitForSeconds(disappearDelay);
                
                
                shadowPrefab.SetActive(false);
            }
            yield return null;
        }
    }
}
