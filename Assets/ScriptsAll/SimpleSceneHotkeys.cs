// Assets/Scripts/SimpleSceneHotkeys.cs
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Простой переход между двумя сценами по клавишам.
/// Минимум логики: A -> B (goKey), B -> A (backKey).
/// </summary>
public sealed class SimpleSceneHotkeys : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Имя целевой сцены (как в файле .unity и Build Settings).")]
    public string targetSceneName = "SoundScene";

    [Header("Hotkeys")]
    public KeyCode goKey = KeyCode.F10;
    public KeyCode backKey = KeyCode.F9;

    private string _previousScene; // почему: нужно помнить, куда возвращаться

    private void Update()
    {
        if (!string.IsNullOrWhiteSpace(targetSceneName) && Input.GetKeyDown(goKey))
        {
            // Уже в целевой сцене — не прыгаем повторно
            var current = SceneManager.GetActiveScene().name;
            if (current == targetSceneName) return;

            if (!CanLoadByName(targetSceneName))
            {
                Debug.LogWarning($"[SimpleSceneHotkeys] Сцена '{targetSceneName}' не в Build Settings.");
                return;
            }

            _previousScene = current;
            SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
        }

        if (!string.IsNullOrEmpty(_previousScene) && Input.GetKeyDown(backKey))
        {
            if (!CanLoadByName(_previousScene))
            {
                Debug.LogWarning($"[SimpleSceneHotkeys] Предыдущая сцена '{_previousScene}' не в Build Settings.");
                return;
            }

            var backTo = _previousScene;
            _previousScene = null; // чтобы избежать циклов при спаме клавиши
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
