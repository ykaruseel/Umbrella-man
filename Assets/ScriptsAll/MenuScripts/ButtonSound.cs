using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private EventInstance hoverInstance;
    private EventInstance clickInstance;

    private string hoverPath = "event:/UI/UI_point_button";
    private string clickPath = "event:/UI/UI_click_button";

    private void OnDestroy()
    {
        hoverInstance.release();
        clickInstance.release();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverInstance = RuntimeManager.CreateInstance(hoverPath);
        hoverInstance.start();
        hoverInstance.release();
    }

    public void OnPointerClick(PointerEventData eventData)
    {

        clickInstance = RuntimeManager.CreateInstance(clickPath);
        clickInstance.start();
        clickInstance.release();
    }
}
