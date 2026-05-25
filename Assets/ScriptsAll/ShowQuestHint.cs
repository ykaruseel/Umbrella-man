using UnityEngine;

public class ShowQuestHint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TutorialManager.Instance.ShowHint(HintType.ViewQuest);
        }
    }
}