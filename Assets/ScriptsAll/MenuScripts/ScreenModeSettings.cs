using TMPro;
using UnityEngine;

public class ScreenModeSettings : MonoBehaviour
{
    [SerializeField] private TMP_Text modeText;
    private FullScreenMode[] modes = new FullScreenMode[]
    {
        FullScreenMode.ExclusiveFullScreen,
        FullScreenMode.Windowed,
        FullScreenMode.FullScreenWindow
    };
    private string[] modeNames = new string[]
    {
        "Fullscreen",
        "Windowed",
        "Borderless"
    };

    private int currentIndex;

    private void Start()
    {
        if (!PlayerPrefs.HasKey("ScreenMode"))
        {
            currentIndex = 2; 
        }
        else
        {
            currentIndex = PlayerPrefs.GetInt("ScreenMode");
        }

        ApplyMode();
    }

    public void NextMode()
    {
        currentIndex = (currentIndex + 1) % modes.Length;
        ApplyMode();
    }

    public void PreviousMode()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = modes.Length - 1;
        ApplyMode();
    }

    private void ApplyMode()
    {
        Screen.fullScreenMode = modes[currentIndex];
        modeText.text = modeNames[currentIndex];
        PlayerPrefs.SetInt("ScreenMode", currentIndex);
    }
}
