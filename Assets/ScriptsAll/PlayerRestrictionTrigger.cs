using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerRestrictionTrigger : MonoBehaviour
{
    [Header("camera")]
    [Tooltip("speed return")]
    public float turnSpeed = 1.2f; 
    [Tooltip("speed text")]
    public float fadeSpeed = 2f;

    [Header("text")]
    [TextArea(2, 4)]
    public string commentText = "I'm sure Lester lives on my floor in apartment 307.";
    [Tooltip("obiekt z Canvas")]
    public TMP_Text subtitleUI;
    [Tooltip("typing speed text")]
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
            Color c = subtitleUI.color;
            c.a = 1f;
            subtitleUI.color = c;
            subtitleUI.gameObject.SetActive(true);
            StartCoroutine(TypeText());
        }

        
        float startPitch = 0f;
        if (player.playerCamera != null)
        {
            
            startPitch = player.playerCamera.transform.localEulerAngles.x;
            
            if (startPitch > 180) startPitch -= 360;
        }

        
        Quaternion startRot = player.transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0, 180, 0);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * turnSpeed;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            player.transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
            
            
            player.SetRotation(player.transform.eulerAngles.y, startPitch);
            
            yield return null;
        }
        
        
        player.transform.rotation = targetRot;
        player.SetRotation(targetRot.eulerAngles.y, startPitch);

        
        if (cc != null) cc.enabled = true;
        player.SetCanMove(true);

        
        yield return new WaitForSeconds(2.5f);
        
        
        if (subtitleUI != null)
        {
            float alpha = 1f;
            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * fadeSpeed;
                Color c = subtitleUI.color;
                c.a = alpha;
                subtitleUI.color = c;
                yield return null;
            }
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
