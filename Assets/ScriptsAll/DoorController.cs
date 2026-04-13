using FMOD.Studio;
using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DoorController : MonoBehaviour
{
    // ==========================================
    // СТАРЫЕ НАСТРОЙКИ (ОБЫЧНАЯ ДВЕРЬ)
    // ==========================================
    [Header("Normal Door Settings")]
    [SerializeField] private Transform door;
    [SerializeField] private Transform handle;

    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openDuration = 1f;
    [SerializeField] private float handleRotationAngle = 30f;
    [SerializeField] private Vector3 handleRotationAxis = Vector3.right;

    [SerializeField] private float autoCloseMin = 10f;
    [SerializeField] private float autoCloseMax = 15f;

    [SerializeField] private EventReference DoorAudio;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion handleClosedRotation;
    private Quaternion handleOpenRotation;

    private bool isOpen = false;
    private bool isAnimating = false;
    private Coroutine autoCloseCoroutine;

    // ==========================================
    // НОВЫЕ НАСТРОЙКИ (QTE ДВЕРЬ)
    // ==========================================
    [Header("QTE Settings")]
    [SerializeField] public bool isLockedWithQTE = false; 

    [Header("QTE Events (Блокировка Игрока)")]
    public UnityEvent onQteStart; 
    public UnityEvent onQteEnd;   

    [Header("QTE UI")]
    [SerializeField] private GameObject qtePanel;        
    [SerializeField] private RectTransform movingLine;   
    [SerializeField] private RectTransform barArea;      

    [Header("QTE Zones (Только верхняя половина)")]
    [SerializeField] private Vector2 weakZone = new Vector2(0.45f, 0.65f);   
    [SerializeField] private Vector2 mediumZone = new Vector2(0.65f, 0.85f);
    [SerializeField] private Vector2 strongZone = new Vector2(0.85f, 1.0f);

    [Header("QTE Difficulty (ХАРДКОР)")]
    [SerializeField] private float lineSpeed = 2.0f;  // Ускорили бегунок, чтобы было сложнее!
    [SerializeField] private float qteCooldown = 0.4f; 

    [Header("QTE Damage (100 HP = 3 сильных, 5 средних, 10 слабых)")]
    [SerializeField] private float weakDamage = 10f;   // Нужно 10 ударов
    [SerializeField] private float mediumDamage = 20f; // Нужно 5 ударов
    [SerializeField] private float strongDamage = 34f; // Нужно 3 удара
    
    [Header("QTE Visuals & Audio")]
    [SerializeField] private ParticleSystem hitDust;        
    [SerializeField] private ParticleSystem fallDust;       
    [SerializeField] private ParticleSystem breakParticles; 
    [SerializeField] private EventReference qteHitSound;

    // Внутренние переменные QTE
    private float currentHealth = 100f;
    private bool isQteActive = false;
    private bool isQteCooldown = false;
    private float linePos = 0f; 
    private int lineDir = 1;
    private Vector3 originalLocalPos;

    private void Start()
    {
        closedRotation = door.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);

        handleClosedRotation = handle.localRotation;
        handleOpenRotation = handleClosedRotation * Quaternion.AngleAxis(handleRotationAngle, handleRotationAxis);

        originalLocalPos = door.localPosition;
        if (qtePanel != null) qtePanel.SetActive(false);
    }

    private void Update()
    {
        if (isQteActive && !isQteCooldown)
        {
            // Линия бегает от 0 до 1 туда-сюда
            linePos += lineSpeed * lineDir * Time.deltaTime;
            if (linePos >= 1f || linePos <= 0f)
            {
                lineDir *= -1;
                linePos = Mathf.Clamp(linePos, 0f, 1f);
            }

            if (movingLine != null && barArea != null)
            {
                float barHeight = barArea.rect.height;
                float lineHeight = movingLine.rect.height;
                float startY = lineHeight / 2f;
                float endY = barHeight - (lineHeight / 2f);

                if (barArea.pivot.y == 0.5f)
                {
                    startY -= barHeight / 2f;
                    endY -= barHeight / 2f;
                }

                float newY = Mathf.Lerp(startY, endY, linePos);
                movingLine.localPosition = new Vector3(movingLine.localPosition.x, newY, 0);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                CheckQTEHit();
            }
        }
    }

    private void PlayQTEHit(int hitType)
    {
        if (qteHitSound.IsNull) return;

        var instance = RuntimeManager.CreateInstance(qteHitSound);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        instance.setParameterByName("HitType", hitType);
        instance.start();
        instance.release();
    }

    public void TryOpenDoor()
    {
        if (isAnimating) return;

        if (isLockedWithQTE)
        {
            if (!isQteActive && currentHealth > 0)
            {
                StartQTE(); 
            }
            return; 
        }

        if (!isOpen)
        {
            StartCoroutine(OpenDoor());
            PlayDoorSound(0);
        }
        else
        {
            PlayDoorSound(1);
            StartCoroutine(CloseDoor());
        }
    }

    // ==========================================
    // ЛОГИКА МИНИ-ИГРЫ QTE
    // ==========================================
    private void StartQTE()
    {
        currentHealth = 100f;
        linePos = 0f; 
        lineDir = 1;
        isQteActive = true;
        isQteCooldown = false;
        if (qtePanel != null) qtePanel.SetActive(true);
        
        onQteStart?.Invoke();
    }

    private void CheckQTEHit()
    {
        float damageDone = 0f;
        int hitType = -1;

        // Проверяем попадание по зонам
        if (linePos >= strongZone.x && linePos <= strongZone.y)
        {
            damageDone = strongDamage;
            hitType = 2; // Передаем параметр FMOD для сильного удара
        }
        else if (linePos >= mediumZone.x && linePos <= mediumZone.y)
        {
            damageDone = mediumDamage;
            hitType = 1; // Средний удар
        }
        else if (linePos >= weakZone.x && linePos <= weakZone.y)
        {
            damageDone = weakDamage;
            hitType = 0; // Слабый удар
        }

        if (damageDone > 0)
        {
            PlayQTEHit(hitType); 
            StartCoroutine(RegisterQTEHit(damageDone));
        }
        else
        {
            // Промах - просто даем кулдаун
            StartCoroutine(QTECooldown(0.5f));
        }
    }

    private IEnumerator RegisterQTEHit(float damage)
    {
        isQteCooldown = true;
        currentHealth -= damage;

        if (hitDust != null) hitDust.Play();
        StartCoroutine(ShakeDoor());

        yield return new WaitForSeconds(qteCooldown); 

        if (currentHealth <= 0)
        {
            BreakDownDoor(); 
        }
        else
        {
            isQteCooldown = false; 
        }
    }

    private IEnumerator QTECooldown(float time)
    {
        isQteCooldown = true;
        yield return new WaitForSeconds(time);
        isQteCooldown = false;
    }

    private IEnumerator ShakeDoor()
    {
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            door.localPosition = originalLocalPos + Random.insideUnitSphere * 0.05f;
            elapsed += Time.deltaTime;
            yield return null;
        }
        door.localPosition = originalLocalPos;
    }

    private void BreakDownDoor()
    {
        StopAllCoroutines(); 

        isQteActive = false;
        if (qtePanel != null) qtePanel.SetActive(false); 
        
        if (breakParticles != null) breakParticles.Play();
        if (fallDust != null) fallDust.Play(); 

        onQteEnd?.Invoke();

        door.SetParent(null);

        Rigidbody rb = door.gameObject.GetComponent<Rigidbody>();
        if (rb == null) rb = door.gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.mass = 15f; 
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; 

        BoxCollider col = door.gameObject.GetComponent<BoxCollider>();
        if (col != null) col.size = col.size * 0.95f;

        Vector3 pushDir = transform.forward;
        if (Camera.main != null)
        {
            pushDir = door.position - Camera.main.transform.position;
            pushDir.y = 0; 
            pushDir.Normalize();
        }

        rb.AddForce((pushDir + Vector3.up * 0.4f) * 6f, ForceMode.VelocityChange);
        rb.AddTorque((transform.right * 3f + transform.up * Random.Range(-1f, 1f)), ForceMode.VelocityChange);

        if (QuestManagerV2.Instance.IsGoalRequired(transform.name, GoalType.Door))
        {
            QuestManagerV2.Instance.ProcessAction(transform.name, GoalType.Door);
        }
        PlayQTEHit(3); // Звук падения/выбивания
    }

    // ==========================================
    // ЛОГИКА ОБЫЧНЫХ ДВЕРЕЙ
    // ==========================================
    private IEnumerator OpenDoor()
    {
        isAnimating = true;
        float handleTime = openDuration * 0.3f;
        float doorTime = openDuration;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / handleTime;
            handle.localRotation = Quaternion.Slerp(handleClosedRotation, handleOpenRotation, t);
            yield return null;
        }
        handle.localRotation = handleOpenRotation;

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / doorTime;
            door.localRotation = Quaternion.Slerp(closedRotation, openRotation, t);
            handle.localRotation = Quaternion.Slerp(handleOpenRotation, handleClosedRotation, t);
            yield return null;
        }
        door.localRotation = openRotation;
        handle.localRotation = handleClosedRotation;

        isOpen = true;
        isAnimating = false;

        if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);
        autoCloseCoroutine = StartCoroutine(AutoCloseDoor());
    }

    private IEnumerator CloseDoor()
    {
        isAnimating = true;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / openDuration;
            door.localRotation = Quaternion.Slerp(openRotation, closedRotation, t);
            yield return null;
        }
        door.localRotation = closedRotation;
        PlayDoorSound(2);
        isOpen = false;
        isAnimating = false;

        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }

    private IEnumerator AutoCloseDoor()
    {
        float waitTime = Random.Range(autoCloseMin, autoCloseMax);
        yield return new WaitForSeconds(waitTime);
        if (!isAnimating && isOpen)
        {
            PlayDoorSound(1);
            StartCoroutine(CloseDoor());
        }
    }

    private void PlayDoorSound(int state)
    {
        EventInstance inst = RuntimeManager.CreateInstance(DoorAudio);
        RuntimeManager.AttachInstanceToGameObject(inst, transform);
        inst.setParameterByName("Door", state);
        inst.start();
        inst.release();
    }
}


