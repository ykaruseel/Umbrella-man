using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEditor.SceneView;

public class TrashCanDoor : MonoBehaviour
{
    private bool isOpen = false;
    private static bool isLocked = true;
    public void OpenDoor()
    {
        if(!isOpen && !isLocked)
        {
            isOpen = true;
            StartCoroutine(Open());
        }
    }

    public IEnumerator Open()
    {
        Quaternion rotateS = Quaternion.Euler(-90f, 0f, 0f);

        Quaternion rotateF = Quaternion.Euler(-90f, 0f, -90f);

        float time = 0f;

        while (time < 2f)
        {
            time += Time.deltaTime;

            float t = Mathf.SmoothStep(0f, 1f, time / 2f);

            transform.localRotation = Quaternion.Slerp(rotateS, rotateF, t);

            yield return null;
        }

        transform.localRotation = rotateF;
    }

    public static void UnlockDoor() => isLocked = false;
}
