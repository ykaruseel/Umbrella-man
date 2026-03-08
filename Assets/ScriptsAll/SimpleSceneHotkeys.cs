// Assets/Scripts/SimpleSceneHotkeys.cs
using UnityEngine;
using UnityEngine.SceneManagement;


public sealed class SimpleSceneHotkeys : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("��� ������� ����� (��� � ����� .unity � Build Settings).")]
    public string targetSceneName = "SoundScene";

    [Header("Hotkeys")]
    public KeyCode goKey = KeyCode.F10;
    public KeyCode backKey = KeyCode.F9;

    private string _previousScene;

    private void Update()
    {
        if (!string.IsNullOrWhiteSpace(targetSceneName) && Input.GetKeyDown(goKey))
        {
            
            var current = SceneManager.GetActiveScene().name;
            if (current == targetSceneName) return;

            if (!CanLoadByName(targetSceneName))
            {
                Debug.LogWarning($"[SimpleSceneHotkeys] ����� '{targetSceneName}' �� � Build Settings.");
                return;
            }

            _previousScene = current;
            SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
        }

        if (!string.IsNullOrEmpty(_previousScene) && Input.GetKeyDown(backKey))
        {
            if (!CanLoadByName(_previousScene))
            {
                Debug.LogWarning($"[SimpleSceneHotkeys] ���������� ����� '{_previousScene}' �� � Build Settings.");
                return;
            }

            var backTo = _previousScene;
            _previousScene = null;
            SceneManager.LoadScene(backTo, LoadSceneMode.Single);
        }
    }

    private static bool CanLoadByName(string sceneName)
    {
#if UNITY_2023_1_OR_NEWER
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
#else
        return Application.CanStreamedLevelBeLoaded(sceneName);
#endif
    }
}
