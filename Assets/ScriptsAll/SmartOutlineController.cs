using UnityEngine;

public class SmartOutlineController : MonoBehaviour
{
    [Header("Камера и Дистанция")]
    public Camera playerCamera;
    public float interactionDistance = 3f;

    [Header("Состояние интеракции")]
    [Tooltip("Если галочка снята, аутлайн не включится (нужно для торшера)")]
    public bool isInteractionAvailable = true;

    private OutlineInteractable myOutline;
    private bool isHovered = false;

    void Start()
    {
        
        myOutline = GetComponent<OutlineInteractable>();

        
        if (playerCamera == null)
        {
            
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        
        if (myOutline == null || playerCamera == null || !isInteractionAvailable)
        {
            if (isHovered) ResetOutline();
            return;
        }

        
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        bool lookingAtMe = false;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                lookingAtMe = true;
            }
        }

        
        if (lookingAtMe && !isHovered)
        {
            isHovered = true;
            myOutline.Show();
        }
        else if (!lookingAtMe && isHovered)
        {
            ResetOutline();
        }
    }

    private void ResetOutline()
    {
        isHovered = false;
        if (myOutline != null)
        {
            myOutline.Hide();
        }
    }

    
    public void SetInteractionActive(bool state)
    {
        isInteractionAvailable = state;
        if (!state) ResetOutline();
    }
}
