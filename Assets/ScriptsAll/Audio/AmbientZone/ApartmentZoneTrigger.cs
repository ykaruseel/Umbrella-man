using UnityEngine;

public class ApartmentZoneTrigger : MonoBehaviour
{
    [SerializeField] private AmbientZoneController controller;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            controller.EnterApartment();
        }
    }
}