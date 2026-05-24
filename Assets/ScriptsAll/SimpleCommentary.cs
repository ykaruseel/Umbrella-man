using UnityEngine;
using System.Collections;
using TMPro;
using FMODUnity;

public class SimpleCommentary : MonoBehaviour
{
    [Header("Comment text")]
    [TextArea(2, 3)]
    public string commentText = "text";
    public float typingSpeed = 0.03f;

    [Header("Audio Settings (FMOD)")]
    [Tooltip("Если сюда закинут звук стука, он автоматически проиграется одновременно с текстом")]
    public EventReference knockSound;

    [Header("UI and Camera")]
    public TMP_Text subtitleText;
    public GameObject skipPromptUI;
    public Camera playerCamera;
    
    [Header("Settings")]
    public float interactionDistance = 3f;
    [Tooltip("How many METERS will the camera move forward (small value = easy focus)")]
    public float zoomDistance = 0.15f;

    private bool isUsed = false;
    private PlayerController player;
    
    
    private OutlineInteractable myOutline; 

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        
        
        myOutline = GetComponentInChildren<OutlineInteractable>();
    }

    void Update()
    {
        
        if (isUsed || playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    StartCoroutine(PlayCommentary());
                }
            }
        }
    }

    private IEnumerator PlayCommentary()
    {
        isUsed = true; 

        
        if (myOutline != null)
        {
            myOutline.Hide();  
            myOutline.isBlocked = true; 
        }

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.SetCanMove(false);
        player.isCinematic = true;

        
        Vector3 startPos = playerCamera.transform.localPosition;
        Vector3 targetPos = startPos + new Vector3(0, 0, zoomDistance);
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f; 
            playerCamera.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        
        if (!knockSound.IsNull)
        {
            RuntimeManager.PlayOneShot(knockSound, transform.position);
        }

        
        string textToShow = commentText;

        if (subtitleText != null) 
        { 
            Color c = subtitleText.color;
            c.a = 1f;
            subtitleText.color = c;
            subtitleText.gameObject.SetActive(true); 
            StartCoroutine(TypeText(textToShow)); 
        }
        if (skipPromptUI != null) skipPromptUI.SetActive(true);

        
        float timer = 0;
        while (timer < 4f)
        {
            timer += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.E)) break;
            yield return null;
        }

        if (skipPromptUI != null) skipPromptUI.SetActive(false);

        
        if (subtitleText != null)
        {
            float alpha = 1f;
            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * 2f; 
                Color c = subtitleText.color;
                c.a = alpha;
                subtitleText.color = c;
                yield return null;
            }
            subtitleText.gameObject.SetActive(false);
        }

        
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            playerCamera.transform.localPosition = Vector3.Lerp(targetPos, startPos, t);
            yield return null;
        }

        if (cc != null) cc.enabled = true;
        player.SetCanMove(true);
        player.isCinematic = false;
    }

    private IEnumerator TypeText(string text)
    {
        subtitleText.text = ""; 
        foreach (char c in text.ToCharArray())
        {
            subtitleText.text += c;
            yield return new WaitForSeconds(typingSpeed); 
        }
    }
}
