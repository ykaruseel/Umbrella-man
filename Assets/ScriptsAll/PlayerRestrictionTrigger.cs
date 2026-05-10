using UnityEngine;
using System.Collections;
using TMPro; 

public class PlayerRestrictionTrigger : MonoBehaviour
{
    [Header("Настройки барьера")]
    [Tooltip("Скорость разворота (чем больше, тем быстрее)")]
    public float turnSpeed = 5f; 
    [Tooltip("Время показа текста и защита от повторного срабатывания")]
    public float cooldownTime = 3f; 

    [Header("Текст на экране (UI)")]
    [TextArea(2, 4)]
    public string commentText = "Текст комментария";
    public TMP_Text subtitleUI; 

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player") && !isTriggered)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                StartCoroutine(TurnAndShowText(player));
            }
        }
    }

    private IEnumerator TurnAndShowText(PlayerController player)
    {
        isTriggered = true;

        
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.SetCanMove(false);
        
        
        if (subtitleUI != null)
        {
            subtitleUI.text = commentText;
            subtitleUI.gameObject.SetActive(true);
        }

        
        Quaternion startRot = player.transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0, 180, 0);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * turnSpeed;
            player.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }
        
        
        player.transform.rotation = targetRot;
        player.SetRotation(player.transform.eulerAngles.y, 0f);

        
        if (cc != null) cc.enabled = true;
        player.SetCanMove(true);

        
        yield return new WaitForSeconds(cooldownTime);

        
        if (subtitleUI != null)
        {
            subtitleUI.gameObject.SetActive(false);
        }
        
        isTriggered = false; 
    }
}
