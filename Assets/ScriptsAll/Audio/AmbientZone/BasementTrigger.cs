using UnityEngine;

public class BasementTrigger : MonoBehaviour
{
    [SerializeField] private AmbientController controller;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            controller.SetBasement();
        }
    }
}