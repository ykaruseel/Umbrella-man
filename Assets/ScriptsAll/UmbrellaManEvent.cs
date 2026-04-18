using UnityEngine;
using System.Collections;

public class UmbrellaManEvent : MonoBehaviour
{
    [Header("Co i gdzie się rozmnażamy?")]
    [Tooltip("Przeciągnij tutaj wyłączony model cienia")]
    public GameObject shadowPrefab; 
    
    [Tooltip("Przeciągnij gracza tutaj")]
    public Transform player; 
    
    [Tooltip("Odległość wyglądu za plecami")]
    public float spawnDistance = 3.0f; 
    
    
    [Tooltip("Regulacja wysokości. Jeśli obiekt wisi w powietrzu, ustaw wartość na minus (na przykład -1).")]
    public float yOffset = -1.0f; 
    
    [Tooltip("Szansa na sukces w procentach")]
    [Range(0, 100)] public int spawnChance = 50; 

    [Header("Disapear")]
    [Tooltip("Kamera gracza (pierwsza osoba)")]
    public Camera mainCamera;
    
    [Tooltip("Ile sekund wisi zanim zniknie?")]
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
        
        
        spawnPos.y = player.position.y + yOffset;

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
