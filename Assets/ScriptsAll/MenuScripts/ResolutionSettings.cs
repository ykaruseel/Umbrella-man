using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ResolutionSettings : MonoBehaviour
{
    [SerializeField] private TMP_Text resolutionText;
    private List<Resolution> resolutions;
    private int currentIndex;

    private void Start()
    {
        resolutions = new List<Resolution>();
        foreach (var res in Screen.resolutions)
        {
            bool exists = resolutions.Exists(r => r.width == res.width && r.height == res.height);
            if (!exists)
                resolutions.Add(res);
        }

        if (!PlayerPrefs.HasKey("ResolutionIndex"))
        {
            Resolution current = Screen.currentResolution;

            currentIndex = resolutions.FindIndex(r =>
                r.width == current.width &&
                r.height == current.height);

            if (currentIndex < 0)
                currentIndex = resolutions.Count - 1;
        }
        else
        {
            currentIndex = Mathf.Clamp(
                PlayerPrefs.GetInt("ResolutionIndex"),
                0,
                resolutions.Count - 1);
        }

        ApplyResolution();
    }


    public void NextResolution()
    {
        currentIndex = (currentIndex + 1) % resolutions.Count;
        ApplyResolution();
    }

    public void PreviousResolution()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = resolutions.Count - 1;
        ApplyResolution();
    }

    private void ApplyResolution()
    {
        Resolution res = resolutions[currentIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
        resolutionText.text = $"{res.width} x {res.height}";
        PlayerPrefs.SetInt("ResolutionIndex", currentIndex);
    }
}
