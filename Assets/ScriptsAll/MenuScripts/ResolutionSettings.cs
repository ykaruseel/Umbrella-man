using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ResolutionSettings : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    Resolution[] resolutions;

    private void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int defaultIndex = 0;

        Resolution currentRes = Screen.currentResolution;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            if (!options.Contains(option))
            {
                options.Add(option);
            }

            if (resolutions[i].width == currentRes.width && resolutions[i].height == currentRes.height)
            {
                defaultIndex = options.IndexOf(option);
            }
        }

        resolutionDropdown.AddOptions(options);

        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", defaultIndex);

        resolutionDropdown.value = savedIndex;
        resolutionDropdown.RefreshShownValue();

        SetResolution(savedIndex);
    }

    public void SetResolution(int index)
    {
        string[] resParts = resolutionDropdown.options[index].text.Split('x');
        int width = int.Parse(resParts[0].Trim());
        int height = int.Parse(resParts[1].Trim());

        Screen.SetResolution(width, height, Screen.fullScreenMode);

        PlayerPrefs.SetInt("ResolutionIndex", index);
    }
}
