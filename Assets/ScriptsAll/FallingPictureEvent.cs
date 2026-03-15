using UnityEngine;
using FMODUnity;

public class FallingPicture : MonoBehaviour
{
    [Header("Звук падения FMOD")]
    public EventReference impactSound;

    private Vector3 startPos;
    private Quaternion startRot;
    private Rigidbody rb;
    private bool isFalling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        
        startPos = transform.position;
        startRot = transform.rotation;
        
        
        rb.isKinematic = true; 
    }

    
    public void Drop()
    {
        rb.isKinematic = false;
        
        
        rb.AddForce(transform.forward * 0.1f, ForceMode.Impulse); 

        
        rb.AddTorque(transform.right * 0.1f, ForceMode.Impulse); 
        
        isFalling = true;
    }

    
    void OnCollisionEnter(Collision col)
    {
        
        if (isFalling && col.relativeVelocity.magnitude > 0.5f)
        {
            if (!impactSound.IsNull)
            {
                RuntimeManager.PlayOneShot(impactSound, transform.position);
            }
            isFalling = false;
        }
    }

    
    public void PutBack()
    {
        rb.isKinematic = true; 
        rb.linearVelocity = Vector3.zero; 
        
        
        transform.position = startPos;
        transform.rotation = startRot;
    }
}
