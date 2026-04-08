using UnityEngine;

public class GramophoneRotation : MonoBehaviour
{
    [SerializeField] private Transform plate;
    [SerializeField] private Transform handle;

    [SerializeField] private float plateSpeed = 180f;
    [SerializeField] private float handleSpeed = 120f;

    private void Update()
    {
        if (plate != null)
            plate.Rotate(Vector3.up * plateSpeed * Time.deltaTime);

        if (handle != null)
            handle.Rotate(Vector3.right * handleSpeed * Time.deltaTime);
    }
}