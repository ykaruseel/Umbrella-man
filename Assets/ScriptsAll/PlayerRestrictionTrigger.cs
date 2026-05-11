using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerRestrictionTrigger : MonoBehaviour
{
    [Header("Setting camera")]
    [Tooltip("return speed")]
    public float turnSpeed = 1.2f; 
    public float cooldownTime = 3f; 

    [Header("text")]
    [TextArea(2, 4)]
    public string commentText = "I'm sure Lester lives on my floor in apartment 307.";
    [Tooltip("Canvas")]
    public TMP_Text subtitleUI;
    
    [Tooltip("speed")]
    public float typingSpeed = 0.03f;

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
            subtitleUI.gameObject.SetActive(true);
            StartCoroutine(TypeText());
        }

        
        Quaternion startRot = player.transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0, 180, 0);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * turnSpeed;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            player.transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
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

    
    private IEnumerator TypeText()
    {
        subtitleUI.text = "";
        
        
        foreach (char c in commentText.ToCharArray())
        {
            subtitleUI.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
