using FMODUnity;
using System.Collections;
using UnityEngine;

public class UmbrellaManSequenceController : MonoBehaviour
{
    [Header("Particles")]
    public ParticleSystem smokeParticles;
    public ParticleSystem sparkParticles;
    public float particleInitialLifetime = 7f;

    [Header("Growing Object (Wall Spot)")]
    public Transform growingObject;
    public Vector3 targetScale = new Vector3(1f, 2f, 1f);

    [Header("Enemy")]
    public GameObject enemy;
    public Transform enemyTargetPoint;
    public float enemyMoveSpeed = 1.5f;
    public Animator enemyAnimator;
    public Collider enemyCollider;
    public Transform player;

    [Header("Light Flicker")]
    public Light flickerLight;

    [Header("Player Camera")]
    public PlayerController playerController;
    public Transform cinematicTarget;
    public float panDuration = 2f;

    private ParticleSystem.MainModule particleMain;

    public EnemyLightDistortion lightDistortion;

    public EventReference smokeSound;
    public EventReference walkSound;
    public EventReference explosionSound;
    public EventReference pulseSound;

    private FMOD.Studio.EventInstance pulseInstance;

    private bool isEnemyMoving = false;

    private Vector3 enemyStartPosition;
    private Quaternion enemyStartRotation;
    private Vector3 growingObjectStartScale;

    private void Awake()
    {
        if (enemy != null)
        {
            enemyStartPosition = enemy.transform.position;
            enemyStartRotation = enemy.transform.rotation;
        }

        if (growingObject != null)
        {
            growingObjectStartScale = growingObject.localScale;
        }

        particleMain = smokeParticles.main;
    }

    public void StartSequence()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.EnsureMusicPlaying();
            MusicManager.Instance.SetSection("Value E");
        }

        lightDistortion.enabled = false;
        StartCoroutine(SequenceRoutine());
    }

    private IEnumerator SequenceRoutine()
    {
        playerController.isCinematic = true;
        playerController.SetCanMove(false);
        particleMain = smokeParticles.main;

        playerController.StartCinematicPan(cinematicTarget, panDuration);
        playerController.ZoomIn();

        RuntimeManager.PlayOneShotAttached(smokeSound, smokeParticles.gameObject);

        smokeParticles.Play();
        StartCoroutine(LightFlickerRoutine());

        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(ScaleObjectRoutine(2f));

        yield return StartCoroutine(ReduceParticleLifetimeRoutine(5f));

        smokeParticles.Stop();
        particleMain.startLifetime = particleInitialLifetime;

        enemy.SetActive(true);
        enemyCollider.enabled = false;

        yield return StartCoroutine(MoveEnemyRoutine());

        FacePlayer();

        yield return StartCoroutine(IdleAndShrinkRoutine());

        StartCoroutine(PlaySparkParticles());
        RuntimeManager.PlayOneShotAttached(explosionSound, flickerLight.gameObject);

        flickerLight.enabled = false;

        playerController.ZoomOut();
        
        playerController.isCinematic = false;
        playerController.SetCanMove(true);
        
        lightDistortion.enabled = true;

        enemyCollider.enabled = true;

        QuestManager.instance.TriggerChaseScene();
    }

    private IEnumerator PlaySparkParticles()
    {
        sparkParticles.Play();

        yield return new WaitForSeconds(0.1f);

        sparkParticles.Stop();
    }

    private IEnumerator ScaleObjectRoutine(float duration)
    {
        Vector3 startScale = growingObject.localScale;
        float t = 0f;

        while (t < duration)
        {
            growingObject.localScale = Vector3.Lerp(startScale, targetScale, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        growingObject.localScale = targetScale;
    }

    private IEnumerator ReduceParticleLifetimeRoutine(float duration)
    {
        float startLifetime = particleInitialLifetime;
        float t = 0f;

        while (t < duration)
        {
            float value = Mathf.Lerp(startLifetime, 0f, t / duration);
            particleMain.startLifetime = value;

            t += Time.deltaTime;
            yield return null;
        }

        particleMain.startLifetime = 0f;
    }

    private IEnumerator MoveEnemyRoutine()
    {
        isEnemyMoving = true;
        enemyAnimator.SetBool("isMoving", true);

        StartCoroutine(FootstepLoop());

        Transform tr = enemy.transform;

        while (Vector3.Distance(tr.position, enemyTargetPoint.position) > 0.05f)
        {
            Vector3 dir = (enemyTargetPoint.position - tr.position).normalized;

            tr.position += dir * enemyMoveSpeed * Time.deltaTime;

            if (dir != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(dir);
                tr.rotation = Quaternion.Slerp(tr.rotation, rot, Time.deltaTime * 5f);
            }

            yield return null;
        }

        isEnemyMoving = false;
        enemyAnimator.SetBool("isMoving", false);
    }

    private void FacePlayer()
    {
        Vector3 dir = (player.position - enemy.transform.position);
        dir.y = 0f;

        if (dir != Vector3.zero)
        {
            enemy.transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    private IEnumerator IdleAndShrinkRoutine()
    {
        float duration = 2f;
        float t = 0f;

        Vector3 startScale = growingObject.localScale;

        while (t < duration)
        {
            growingObject.localScale = Vector3.Lerp(startScale, Vector3.zero, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        growingObject.localScale = Vector3.zero;
        growingObject.gameObject.SetActive(false);
    }

    private IEnumerator LightFlickerRoutine()
    {
        if (flickerLight == null) yield break;

        flickerLight.enabled = true;

        pulseInstance = RuntimeManager.CreateInstance(pulseSound);

        RuntimeManager.AttachInstanceToGameObject(pulseInstance, flickerLight.transform, flickerLight.transform);

        pulseInstance.start();

        float minDuration = 0.5f;
        float maxDuration = 1f;
        float decreaseFactor = 0.6f;

        float targetMin = 1.5f;
        float targetMax = 4f;

        while (enemyCollider != null && !enemyCollider.enabled)
        {
            float startIntensity = flickerLight.intensity;
            float targetIntensity = Random.Range(targetMin, targetMax);

            float duration = Random.Range(minDuration, maxDuration);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                flickerLight.intensity = Mathf.Lerp(
                    startIntensity,
                    targetIntensity,
                    Mathf.SmoothStep(0f, 1f, t)
                );

                yield return null;
            }

            flickerLight.intensity = targetIntensity;

            minDuration *= decreaseFactor;
            maxDuration *= decreaseFactor;

            minDuration = Mathf.Max(minDuration, 0.05f);
            maxDuration = Mathf.Max(maxDuration, 0.1f);
        }

        pulseInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        pulseInstance.release();
    }

    private IEnumerator FootstepLoop()
    {
        while (Vector3.Distance(enemy.transform.position, enemyTargetPoint.position) > 0.05f && isEnemyMoving)
        {
            RuntimeManager.PlayOneShotAttached(walkSound, enemy);

            float delay = Random.Range(0.6f, 0.8f);
            yield return new WaitForSeconds(delay);
        }
    }

    public void ResetSequence()
    {
        StopAllCoroutines();

        if (enemy != null)
        {
            enemy.SetActive(false);
            enemyCollider.enabled = false;
            enemy.transform.position = enemyStartPosition;
            enemy.transform.rotation = enemyStartRotation;
            enemyAnimator.SetBool("isMoving", false);
        }

        if (growingObject != null)
        {
            growingObject.localScale = growingObjectStartScale;
            growingObject.gameObject.SetActive(true);
        }

        if (smokeParticles != null)
        {
            smokeParticles.Stop();
            particleMain.startLifetime = particleInitialLifetime;
        }

        if (flickerLight != null)
        {
            flickerLight.enabled = false;
            flickerLight.intensity = 6f;
        }

        if (pulseInstance.isValid())
        {
            pulseInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            pulseInstance.release();
        }

        if(sparkParticles != null)
        {
            sparkParticles.Stop();
        }

        isEnemyMoving = false;

        if (lightDistortion != null)
            lightDistortion.enabled = true;
    }

}
