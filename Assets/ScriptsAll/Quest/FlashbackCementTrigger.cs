using UnityEngine;

public class FlashbackCementTrigger : MonoBehaviour
{
    [SerializeField]
    private ReturnHomeFlashbackController
        flashbackController;


    private bool used = false;


    private void OnTriggerEnter(
        Collider other)
    {
        if (used)
            return;


        PlayerController player =
            other.GetComponentInParent<PlayerController>();


        if (player == null)
            return;


        if (flashbackController == null)
            return;


        if (
            flashbackController
            .TryStartCementSequence()
        )
        {
            used = true;
        }
    }
}