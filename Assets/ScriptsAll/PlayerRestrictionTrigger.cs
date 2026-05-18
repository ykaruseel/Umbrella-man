using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerRestrictionTrigger : MonoBehaviour
{
    [Header("camera")]
    [Tooltip("spedd return")]
    public float turnSpeed = 1.2f; 
    [Tooltip("fade text")]
    public float fadeSpeed = 2f;

    [Header("text")]
    [TextArea(2, 4)]
    public string commentText = "I'm sure Lester lives on my floor in apartment 307.";
    [Tooltip("Canvas")]
    public TMP_Text subtitleUI;
    [Tooltip("speed text")]
    public float typingSpeed = 0.03f;

    [Header("Настройки блокировки")]
    [Tooltip("На сколько секунд стена становится твердой (каменной) после разворота")]
    public float solidTime = 3.0f;

    private bool isProcessing = false;
    private BoxCollider myCollider;

    void Start()
    {
        
        myCollider = GetComponent<BoxCollider>();
        if (myCollider != null)
        {
            myCollider.isTrigger = true;
        }
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isProcessing)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                StartCoroutine(TurnAndBlockProcess(player));
            }
        }
    }

    private IEnumerator TurnAndBlockProcess(PlayerController player)
    {
        isProcessing = true;

        
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

        
        player.transform.position += player.transform.forward * 0.4f;

        
        if (myCollider != null)
        {
            myCollider.isTrigger = false;
        }

        
        if (cc != null) cc.enabled = true;
        player.SetCanMove(true);

        
        yield return new WaitForSeconds(2.0f);
        
        
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

        
        yield return new WaitForSeconds(solidTime - 2.0f);

        
        if (myCollider != null)
        {
            myCollider.isTrigger = true;
        }

        isProcessing = false; 
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
