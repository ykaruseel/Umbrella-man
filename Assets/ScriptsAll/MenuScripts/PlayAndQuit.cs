using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayAndQuit : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        Debug.Log("Trying to load scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void QuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
