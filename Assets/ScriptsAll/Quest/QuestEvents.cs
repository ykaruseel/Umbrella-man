using System.Collections.Generic;
using UnityEngine;

public class QuestEvents : MonoBehaviour
{
    // Versia prototip, perenesti vse eventy siuda potom i dielat vizov iz QMV2 czerez switch i ID kvesta
    public List<GameObject> objectsToEnable;

    public List<GameObject> objectsToDisable;

    public static QuestEvents Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void VremennoQ3()
    {
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}
