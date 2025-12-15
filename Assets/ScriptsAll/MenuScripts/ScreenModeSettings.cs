using UnityEngine;

public class ScreenModeSettings : MonoBehaviour
{
    private void Start()
    {
        int mode = PlayerPrefs.GetInt("ScreenMode", 0);
        SetScreenMode(mode);
    }

    public void SetScreenMode(int mode)
    {
        switch (mode)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
        }

        PlayerPrefs.SetInt("ScreenMode", mode);
    }
}
