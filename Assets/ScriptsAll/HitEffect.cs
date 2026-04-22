using UnityEngine;
using UnityEngine.Rendering;

public class HitEffect : MonoBehaviour
{
    public Volume hitVolume;
    public float duration = 0.5f;
    private float timer = 0f;

    void Start()
    {
        if (hitVolume != null) hitVolume.weight = 0;
    }

    public void TakeDamageEffect()
    {
        timer = duration;
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            hitVolume.weight = timer / duration;
        }
        //dla testa
        if (Input.GetKeyDown(KeyCode.L))
        {
            TakeDamageEffect();
        }
    }
}
