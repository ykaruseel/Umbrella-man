using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    public string targetID;
    public GoalType type;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && QuestManagerV2.Instance.IsGoalRequired(targetID, type))
        {
            QuestManagerV2.Instance.ProcessAction(targetID, type);
            gameObject.GetComponent<Collider>().enabled = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && QuestManagerV2.Instance.IsGoalRequired(targetID, type))
        {
            QuestManagerV2.Instance.ProcessAction(targetID, type);
            gameObject.GetComponent<Collider>().enabled = false;
        }
    }
}
