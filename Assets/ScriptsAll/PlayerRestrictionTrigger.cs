using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerRestrictionTrigger : MonoBehaviour
{
    [Header("Настройки камеры")]
    [Tooltip("Скорость разворота игрока")]
    public float turnSpeed = 1.2f; 
    [Tooltip("Скорость затухания текста (чем больше, тем быстрее исчезает)")]
    public float fadeSpeed = 2f;

    [Header("Текст ограничения")]
    [TextArea(2, 4)]
    public string commentText = "I'm sure Lester lives on my floor in apartment 307.";
    [Tooltip("Перетащи сюда твой текстовый объект с Canvas")]
    public TMP_Text subtitleUI;
    [Tooltip("Скорость печатания текста")]
    public float typingSpeed = 0.03f;

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем игрока и что скример-разворот не активен прямо в эту секунду
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

        // 1. Блокируем управление игрока
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.SetCanMove(false);
        
        // 2. Включаем текст и сбрасываем прозрачность на 100% (чтобы он не был невидимым)
        if (subtitleUI != null)
        {
            Color c = subtitleUI.color;
            c.a = 1f;
            subtitleUI.color = c;
            subtitleUI.gameObject.SetActive(true);
            StartCoroutine(TypeText());
        }

        // 3. Плавный поворот камеры на 180 градусов
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

        // 4. СРАЗУ возвращаем управление (чтобы игрок мог уйти в обратную сторону)
        if (cc != null) cc.enabled = true;
        player.SetCanMove(true);

        // 5. Ждем пару секунд, пока текст горит на экране
        yield return new WaitForSeconds(2.5f);
        
        // 6. ИСПРАВЛЕНИЕ: Плавный Fade Out (затухание) текста
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
        
        // --- ИСПРАВЛЕНИЕ 2: НЕ выключаем скрипт! Переменная возвращается в false, ---
        // поэтому при следующем наступлении в куб комментарий ПОВТОРИТСЯ снова.
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
