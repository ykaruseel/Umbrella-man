using UnityEngine;
using System.Collections;
using TMPro;
using FMODUnity;

public class SimpleCommentary : MonoBehaviour
{
    [Header("Текст комментария")]
    [TextArea(2, 3)]
    public string commentText = "text";
    public float typingSpeed = 0.03f;

    [Header("Двери соседей")]
    public bool isNeighborDoor = false;
    public EventReference knockSound;

    [Header("UI и Камера")]
    public TMP_Text subtitleText;
    public GameObject skipPromptUI;
    public Camera playerCamera;
    
    [Header("Настройки")]
    public float interactionDistance = 3f;
    [Tooltip("На сколько МЕТРОВ камера подастся вперед (маленькое значение = легкий фокус)")]
    public float zoomDistance = 0.15f; // ВЕРНУЛИ ФИЗИЧЕСКИЙ ЗУМ, НО СДЕЛАЛИ ЕГО КРОШЕЧНЫМ

    private bool isUsed = false;
    private PlayerController player;

    private string[] neighborPhrases = {
        "Get lost, lunatic!",
        "Find something more interesting to do.",
        "Knock again and you're gonna regret it.",
        "Please leave me alone!"
    };

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
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

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.SetCanMove(false);
        player.isCinematic = true;

        // --- ЛЕГКАЯ ФИЗИЧЕСКАЯ ФОКУСИРОВКА (Двигаем на 15 см вперед) ---
        Vector3 startPos = playerCamera.transform.localPosition;
        Vector3 targetPos = startPos + new Vector3(0, 0, zoomDistance);
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f; 
            playerCamera.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        string textToShow = commentText;
        if (isNeighborDoor)
        {
            if (!knockSound.IsNull) RuntimeManager.PlayOneShot(knockSound, transform.position);
            yield return new WaitForSeconds(1f);
            textToShow = neighborPhrases[Random.Range(0, neighborPhrases.Length)];
        }

        // --- ПОКАЗ ТЕКСТА ---
        if (subtitleText != null) 
        { 
            Color c = subtitleText.color;
            c.a = 1f;
            subtitleText.color = c;
            subtitleText.gameObject.SetActive(true); 
            StartCoroutine(TypeText(textToShow));
        }
        if (skipPromptUI != null) skipPromptUI.SetActive(true);

        // Ждем 4 секунды или пропускаем только на "E"
        float timer = 0;
        while (timer < 4f)
        {
            timer += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.E)) break; // Пропуск только на 'E'
            yield return null;
        }

        if (skipPromptUI != null) skipPromptUI.SetActive(false);

        // --- ФЭЙД-АУТ (Плавное затухание текста) ---
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

        // --- ВОЗВРАТ КАМЕРЫ НАЗАД ---
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
