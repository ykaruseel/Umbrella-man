using System.Collections;
using TMPro;
using UnityEngine;

public class OptionalBookQuestState : MonoBehaviour
{
    [SerializeField] private TMP_Text notificationText;
    [SerializeField, Min(0.5f)] private float notificationSeconds = 5f;

    public bool IsUnlocked { get; private set; }

    private void Awake()
    {
        if (notificationText != null)
            notificationText.gameObject.SetActive(false);
    }

    public void UnlockQuest()
    {
        if (IsUnlocked) return;
        IsUnlocked = true;
        Debug.Log("[OptionalBook] Tommy's book side quest unlocked.");
        if (notificationText != null)
        {
            notificationText.text = "Optional quest unlocked: Retrieve Tommy's book";
            notificationText.gameObject.SetActive(true);
            StartCoroutine(HideNotification());
        }
    }

    private IEnumerator HideNotification()
    {
        yield return new WaitForSeconds(notificationSeconds);
        if (notificationText != null)
            notificationText.gameObject.SetActive(false);
    }
}
