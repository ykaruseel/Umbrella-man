using UnityEngine;

public class StairwellZoneTrigger : MonoBehaviour
{
    [SerializeField] private AmbientZoneController controller;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            controller.EnterStairwell();
        }
    }
}