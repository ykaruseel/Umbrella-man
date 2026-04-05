using FMODUnity;
using System.Collections;
using UnityEngine;

public class UmbrellaMan : MonoBehaviour
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


    [Header("Player Camera")]
    public Transform player;

    private ParticleSystem.MainModule particleMain;

    public EventReference smokeSound;
    public EventReference walkSound;

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

        StartCoroutine(SequenceRoutine());
    }

    private IEnumerator SequenceRoutine()
    {
        RuntimeManager.PlayOneShotAttached(smokeSound, smokeParticles.gameObject);

        smokeParticles.Play();

        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(ScaleObjectRoutine(2f));

        yield return StartCoroutine(ReduceParticleLifetimeRoutine(5f));

        smokeParticles.Stop();
        particleMain.startLifetime = particleInitialLifetime;

        enemy.SetActive(true);

        yield return StartCoroutine(MoveEnemyRoutine());

        FacePlayer();

        yield return StartCoroutine(IdleAndShrinkRoutine());

        StartCoroutine(PlaySparkParticles());
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


    private IEnumerator FootstepLoop()
    {
        while (Vector3.Distance(enemy.transform.position, enemyTargetPoint.position) > 0.05f && isEnemyMoving)
        {
            RuntimeManager.PlayOneShotAttached(walkSound, enemy);

            float delay = Random.Range(0.6f, 0.8f);
            yield return new WaitForSeconds(delay);
        }
    }
}
