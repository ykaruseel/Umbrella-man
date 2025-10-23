using UnityEngine;

public class PlacementSpot : MonoBehaviour
{
    // ID предмета, который подходит к этому месту (должен совпадать с ID предмета)
    public string requiredItemID;

    // Ссылка на объект-подсветку (например, свет или полупрозрачный материал)
    public GameObject highlightEffect;

    // Точное место и поворот, где будет стоять предмет после установки
    public Transform placementTransform;

    void Start()
    {
        // В начале игры подсветка всегда выключена
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(false);
        }
    }

    // Этот метод будет включать или выключать подсветку
    public void SetHighlight(bool isActive)
    {
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(isActive);
        }
    }
}
