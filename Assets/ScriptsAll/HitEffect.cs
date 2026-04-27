using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class HitEffect : MonoBehaviour
{
    public Volume hitVolume;
    public float duration = 0.5f;
    private float timer = 0f;

    public Image bloodImage;
    public List<Sprite> bloodSprites;

    void Start()
    {
        if (hitVolume != null) hitVolume.weight = 0;
    }

    public void TakeDamageEffect()
    {
        timer = duration;
        if (bloodImage != null && bloodSprites != null && bloodSprites.Count > 0)
        {
            int randomIndex = Random.Range(0, bloodSprites.Count);
            bloodImage.sprite = bloodSprites[randomIndex];
        }
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;

            float progress = timer / duration;

            if (hitVolume != null) hitVolume.weight = progress;

            if (bloodImage != null) SetImageAlpha(progress);
        }
        else if (hitVolume != null && hitVolume.weight > 0)
        {
            hitVolume.weight = 0;
            SetImageAlpha(0);
        }

        //dla testa
        if (Input.GetKeyDown(KeyCode.L))
        {
            TakeDamageEffect();
        }
    }

    private void SetImageAlpha(float alpha)
    {
        Color c = bloodImage.color;
        c.a = alpha;
        bloodImage.color = c;
    }
}
