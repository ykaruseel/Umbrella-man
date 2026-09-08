using UnityEngine;
using UnityEngine.Video;

public class TVLight : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Light tvLight;
    [SerializeField] private Light tvLight2;
    [SerializeField] private Renderer screenRenderer;

    [Header("Materials")]
    [SerializeField] private Material screenOnMaterial;
    [SerializeField] private Material screenOffMaterial;

    [Header("Light Settings")]
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 2.5f;
    [SerializeField] private float smoothSpeed = 5f;

    private RenderTexture renderTexture;
    private Texture2D texture;
    private bool isTVOn = true;

    void Start()
    {
        if (videoPlayer != null)
            renderTexture = videoPlayer.targetTexture;

        if (renderTexture != null)
            texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
    }

    void Update()
    {
        if (!isTVOn)
        {
            if (tvLight != null)
            {
                tvLight.intensity = Mathf.Lerp(
                    tvLight.intensity,
                    0f,
                    Time.deltaTime * smoothSpeed
                );
            }

            return;
        }

        if (renderTexture == null || tvLight == null || texture == null)
            return;

        RenderTexture.active = renderTexture;

        texture.ReadPixels(
            new Rect(
                renderTexture.width / 2,
                renderTexture.height / 2,
                1,
                1
            ),
            0,
            0
        );

        texture.Apply();

        RenderTexture.active = null;

        Color pixel = texture.GetPixel(0, 0);

        float brightness =
            pixel.r * 0.299f +
            pixel.g * 0.587f +
            pixel.b * 0.114f;

        float targetIntensity = Mathf.Lerp(
            minIntensity,
            maxIntensity,
            brightness
        );

        tvLight.intensity = Mathf.Lerp(
            tvLight.intensity,
            targetIntensity,
            Time.deltaTime * smoothSpeed
        );
    }

    public void Interact()
    {
        isTVOn = !isTVOn;

        if (isTVOn)
        {
            if (videoPlayer != null)
                videoPlayer.Play();

            if (screenRenderer != null && screenOnMaterial != null)
                screenRenderer.material = screenOnMaterial;

            if (tvLight2 != null)
                tvLight2.enabled = true;
        }
        else
        {
            if (videoPlayer != null)
                videoPlayer.Pause();

            if (screenRenderer != null && screenOffMaterial != null)
                screenRenderer.material = screenOffMaterial;

            if (tvLight2 != null)
                tvLight2.enabled = false;
        }
    }
}