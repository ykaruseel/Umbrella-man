using UnityEngine;

public class SpeedZone : MonoBehaviour
{
    [Header("Settings")]
    public float speedBoost = 2f;          // Насколько увеличивается скорость
    public float restoreDuration = 0.5f;   // Время плавного возврата скорости
    public bool zoneEnabled = true;        // Можно включать/выключать из квестов

    private PlayerController player;
    private float defaultWalkSpeed;
    private float defaultRunSpeed;

    private bool playerInside = false;
    private Coroutine restoreRoutine;

    private void OnTriggerEnter(Collider other)
    {
        if (!zoneEnabled) return;

        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<PlayerController>();
            if (player == null) return;

            if (!playerInside)
            {
                playerInside = true;

                // сохраняем оригинальные скорости
                defaultWalkSpeed = player.walkSpeed;
                defaultRunSpeed = player.runSpeed;

                // увеличиваем скорости
                player.walkSpeed += speedBoost;
                player.runSpeed += speedBoost;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!zoneEnabled) return;

        if (other.CompareTag("Player"))
        {
            if (playerInside)
            {
                playerInside = false;

                if (restoreRoutine != null)
                    StopCoroutine(restoreRoutine);

                restoreRoutine = StartCoroutine(RestoreSpeedSmooth());
            }
        }
    }

    private System.Collections.IEnumerator RestoreSpeedSmooth()
    {
        float elapsed = 0f;

        float startWalk = player.walkSpeed;
        float startRun = player.runSpeed;

        while (elapsed < restoreDuration)
        {
            elapsed += Time.deltaTime;

            player.walkSpeed = Mathf.Lerp(startWalk, defaultWalkSpeed, elapsed / restoreDuration);
            player.runSpeed = Mathf.Lerp(startRun, defaultRunSpeed, elapsed / restoreDuration);

            yield return null;
        }

        player.walkSpeed = defaultWalkSpeed;
        player.runSpeed = defaultRunSpeed;
    }

    public void EnableZone(bool enable)
    {
        zoneEnabled = enable;

        if (!enable && playerInside)
        {
            playerInside = false;

            if (restoreRoutine != null)
                StopCoroutine(restoreRoutine);

            restoreRoutine = StartCoroutine(RestoreSpeedSmooth());
        }
    }
}



