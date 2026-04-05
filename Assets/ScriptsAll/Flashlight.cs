using UnityEngine;

public class Flashlight : MonoBehaviour
{
    public string ID;
    
    public Light flashlightLight;

    private void OnEnable()
    {
        Debug.Log($"<color=yellow>[Flashlight]</color> Инициализация фонарика с ID: {ID}");
        QuestManagerV2.Instance.ProcessAction(ID, GoalType.ReturnItem);
    }

    //torze nado by perenesti
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            flashlightLight.enabled = !flashlightLight.enabled;
        }
    }
}
