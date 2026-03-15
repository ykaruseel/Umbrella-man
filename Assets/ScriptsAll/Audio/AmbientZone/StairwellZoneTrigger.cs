using UnityEngine;

public class StairwellTrigger : MonoBehaviour
{
    [SerializeField] private AmbientController controller;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            controller.SetStairwell();
        }
    }
}