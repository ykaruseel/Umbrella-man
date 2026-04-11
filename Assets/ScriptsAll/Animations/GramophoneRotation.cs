using UnityEngine;

public class GramophoneRotation : MonoBehaviour
{
    [SerializeField] private Transform plate;
    [SerializeField] private Transform handlePivot;

    [SerializeField] private float plateSpeed = 180f;
    [SerializeField] private float handleSpeed = 120f;

    [SerializeField] private Vector3 plateAxis = Vector3.up;
    [SerializeField] private Vector3 handleAxis = Vector3.right;

    private void Update()
    {
        if (plate != null)
            plate.Rotate(plateAxis * plateSpeed * Time.deltaTime, Space.Self);

        if (handlePivot != null)
            handlePivot.Rotate(handleAxis * handleSpeed * Time.deltaTime, Space.Self);
    }
}