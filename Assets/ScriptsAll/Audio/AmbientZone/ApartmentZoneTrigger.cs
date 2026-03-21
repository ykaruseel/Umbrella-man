using UnityEngine;

public class ApartmentTrigger : MonoBehaviour
{
    [SerializeField] private AmbientController controller;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            controller.SetApartment();
        }
    }
}