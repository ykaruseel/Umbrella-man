using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollectionController : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField]
    private QuestData questData;

    [SerializeField]
    private string containerPickupObjective = "Find a trash bag";


    [Header("Collection Objects")]
    [SerializeField]
    private CollectionPickup collectionPickup;

    [SerializeField]
    private List<CollectibleItem> collectibleItems =
        new List<CollectibleItem>();


    [Header("Held Container")]
    [SerializeField]
    private GameObject emptyContainerVisual;

    [SerializeField]
    private GameObject fullContainerVisual;

    [SerializeField]
    private Transform collectionTarget;


    [Header("Item Animation")]
    [SerializeField]
    private float flyDuration = 0.55f;

    [Header("Container Animation")]
    [Range(0.1f, 1f)]
    [SerializeField]
    private float squashFactor = 0.85f;

    [SerializeField]
    private float squashDuration = 0.08f;

    [SerializeField]
    private float growDuration = 0.12f;

    [SerializeField]
    private float growthPerItem = 0.04f;


    [Header("Completion")]
    [SerializeField]
    private CameraFade cameraFade;

    [SerializeField]
    private float blackScreenDuration = 0.15f;

    [SerializeField]
    private UnityEvent onCollectionCompleted;


    public bool HasContainer { get; private set; }

    public bool IsCompleted { get; private set; }

    public bool IsBusy => isCollecting;

    public bool HasFullContainer =>
        IsCompleted &&
        fullContainerVisual != null &&
        fullContainerVisual.activeSelf;

    public string QuestID =>
        questData != null ? questData.questID : "";


    public bool CanAcquireContainer =>
        configurationValid &&
        !HasContainer &&
        !IsCompleted &&
        IsCollectionQuestActive();


    private bool isCollecting = false;
    private bool questWasActive = false;
    private bool configurationValid = true;

    private Vector3 baseEmptyContainerScale;
    private Vector3 baseFullContainerScale;

    private readonly HashSet<string> collectedIDs =
        new HashSet<string>();


    private void Awake()
    {
        if (emptyContainerVisual != null)
        {
            baseEmptyContainerScale =
                emptyContainerVisual.transform.localScale;

            emptyContainerVisual.SetActive(false);
        }

        if (fullContainerVisual != null)
        {
            baseFullContainerScale =
                fullContainerVisual.transform.localScale;

            fullContainerVisual.SetActive(false);
        }
    }


    private void Start()
    {
        ValidateConfiguration();

        if (collectionPickup != null)
            collectionPickup.SetAvailable(false);

        SetRemainingCollectiblesAvailable(false);
    }


    private void Update()
    {
        if (IsCompleted)
            return;

        bool questActive = IsCollectionQuestActive();

        if (questActive && !questWasActive)
        {
            questWasActive = true;

            if (collectionPickup != null)
            {
                collectionPickup.SetAvailable(
                    !HasContainer
                );
            }

            SetRemainingCollectiblesAvailable(
                HasContainer
            );


            // Until the bag is picked up —
            // show the first stage of the quest.
            if (!HasContainer)
            {
                QuestManagerV2.Instance.SetCurrentQuestText(
                    containerPickupObjective
                );
            }
            else
            {
                QuestManagerV2.Instance.RefreshCurrentQuestUI();
            }
        }
        else if (!questActive && questWasActive)
        {
            questWasActive = false;

            if (collectionPickup != null)
                collectionPickup.SetAvailable(false);

            SetRemainingCollectiblesAvailable(false);
        }
    }


    private bool IsCollectionQuestActive()
    {
        return
            QuestManagerV2.Instance != null &&
            questData != null &&
            QuestManagerV2.Instance.IsQuestActive(
                questData.questID
            );
    }


    public bool AcquireContainer()
    {
        if (!CanAcquireContainer)
            return false;

        HasContainer = true;

        if (emptyContainerVisual != null)
        {
            emptyContainerVisual.SetActive(true);

            emptyContainerVisual.transform.localScale =
                baseEmptyContainerScale;
        }

        if (fullContainerVisual != null)
            fullContainerVisual.SetActive(false);

        SetRemainingCollectiblesAvailable(true);
        QuestManagerV2.Instance.RefreshCurrentQuestUI();
        return true;
    }


    public bool TryCollect(CollectibleItem item)
    {
        if (item == null)
            return false;

        if (!configurationValid)
            return false;

        if (!IsCollectionQuestActive())
            return false;

        if (!HasContainer)
            return false;

        if (IsCompleted)
            return false;

        if (isCollecting)
            return false;

        if (!item.CanInteract)
            return false;

        if (!collectibleItems.Contains(item))
            return false;

        StartCoroutine(CollectRoutine(item));

        return true;
    }


    private IEnumerator CollectRoutine(
        CollectibleItem item)
    {
        isCollecting = true;

        item.BeginCollection();

        Transform itemTransform = item.transform;

        Vector3 startPosition =
            itemTransform.position;

        Vector3 startScale =
            itemTransform.localScale;

        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / flyDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            Vector3 targetPosition = collectionTarget.position;

            itemTransform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                smoothT
            );

            itemTransform.localScale = Vector3.Lerp(
                startScale,
                Vector3.zero,
                smoothT
            );

            yield return null;
        }

        string collectedID = item.ID;

        collectedIDs.Add(collectedID);

        item.HideAfterCollection();

        int currentAmount =
            collectedIDs.Count;

        int requiredAmount =
            questData.GetRequiredAmount();

        bool finalItem =
            currentAmount >= requiredAmount;


        if (finalItem)
        {
            IsCompleted = true;

            SetRemainingCollectiblesAvailable(false);

            // Записываем последний предмет,
            // но пока НЕ переключаемся
            // на следующий квест.
            QuestManagerV2.Instance.ProcessAction(
                collectedID,
                GoalType.CollectItems,
                true
            );
        }
        else
        {
            QuestManagerV2.Instance.ProcessAction(
                collectedID,
                GoalType.CollectItems
            );
        }


        yield return StartCoroutine(
            AnimateContainer(currentAmount)
        );


        if (finalItem)
        {
            yield return StartCoroutine(
                CompleteCollectionRoutine()
            );
        }

        isCollecting = false;
    }


    private IEnumerator AnimateContainer(
        int collectedAmount)
    {
        if (emptyContainerVisual == null)
            yield break;

        Transform container =
            emptyContainerVisual.transform;

        Vector3 previousScale =
            container.localScale;

        Vector3 squashedScale =
            previousScale * squashFactor;


        yield return StartCoroutine(
            ScaleObject(
                container,
                previousScale,
                squashedScale,
                squashDuration
            )
        );


        yield return StartCoroutine(
            ScaleObject(
                container,
                squashedScale,
                previousScale,
                squashDuration
            )
        );


        Vector3 grownScale =
            baseEmptyContainerScale *
            (1f + growthPerItem * collectedAmount);


        yield return StartCoroutine(
            ScaleObject(
                container,
                previousScale,
                grownScale,
                growDuration
            )
        );
    }


    private IEnumerator ScaleObject(
        Transform target,
        Vector3 from,
        Vector3 to,
        float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t =
                Mathf.Clamp01(time / duration);

            target.localScale =
                Vector3.Lerp(
                    from,
                    to,
                    Mathf.SmoothStep(0f, 1f, t)
                );

            yield return null;
        }

        target.localScale = to;
    }


    private IEnumerator CompleteCollectionRoutine()
    {
        if (cameraFade != null)
        {
            yield return StartCoroutine(
                cameraFade.FadeOut()
            );
        }


        if (emptyContainerVisual != null)
            emptyContainerVisual.SetActive(false);


        if (fullContainerVisual != null)
        {
            fullContainerVisual.SetActive(true);

            fullContainerVisual.transform.localScale =
                baseFullContainerScale;
        }


        if (blackScreenDuration > 0f)
        {
            yield return new WaitForSeconds(
                blackScreenDuration
            );
        }


        if (cameraFade != null)
        {
            yield return StartCoroutine(
                cameraFade.FadeIn()
            );
        }


        onCollectionCompleted?.Invoke();


        QuestManagerV2.Instance.AdvanceCompletedQuest();
    }


    private void SetRemainingCollectiblesAvailable(
        bool available)
    {
        foreach (CollectibleItem item in collectibleItems)
        {
            if (item == null)
                continue;

            if (collectedIDs.Contains(item.ID))
                continue;

            item.SetAvailable(available);
        }
    }


    private void ValidateConfiguration()
    {
        configurationValid = true;

        if (questData == null)
        {
            Debug.LogError(
                "[CollectionController] QuestData is missing."
            );

            configurationValid = false;
            return;
        }

        if (questData.type != GoalType.CollectItems)
        {
            Debug.LogError(
                "[CollectionController] Quest must use CollectItems GoalType."
            );

            configurationValid = false;
        }

        if (collectionTarget == null)
        {
            Debug.LogError(
                "[CollectionController] Collection Target is missing."
            );

            configurationValid = false;
        }

        if (collectibleItems.Count <
            questData.GetRequiredAmount())
        {
            Debug.LogError(
                "[CollectionController] Not enough collectible items. " +
                $"Required: {questData.GetRequiredAmount()}, " +
                $"available: {collectibleItems.Count}."
            );

            configurationValid = false;
        }


        HashSet<string> usedIDs =
            new HashSet<string>();

        foreach (CollectibleItem item in collectibleItems)
        {
            if (item == null)
                continue;

            if (string.IsNullOrWhiteSpace(item.ID))
            {
                Debug.LogError(
                    "[CollectionController] " +
                    $"{item.name} has empty ID."
                );

                configurationValid = false;
                continue;
            }

            if (!usedIDs.Add(item.ID))
            {
                Debug.LogError(
                    "[CollectionController] Duplicate ID: " +
                    item.ID
                );

                configurationValid = false;
            }

            if (!questData.targetID.Contains(item.ID))
            {
                Debug.LogError(
                    "[CollectionController] " +
                    $"{item.ID} is missing from QuestData targetID."
                );

                configurationValid = false;
            }
        }
    }


    public List<string> GetCollectedIDs()
    {
        return new List<string>(collectedIDs);
    }


    public CollectionTaskSaveData CreateSaveData()
    {
        return new CollectionTaskSaveData
        {
            questID = QuestID,
            hasContainer = HasContainer,
            isCompleted = IsCompleted,
            collectedItemIDs = GetCollectedIDs()
        };
    }


    public void RestoreFromSave(
        CollectionTaskSaveData data)
    {
        if (data == null)
            return;

        collectedIDs.Clear();

        if (data.collectedItemIDs != null)
        {
            foreach (string id in data.collectedItemIDs)
                collectedIDs.Add(id);
        }

        HasContainer =
            data.hasContainer || data.isCompleted;

        IsCompleted =
            data.isCompleted;

        isCollecting = false;

        bool questActive =
            IsCollectionQuestActive();

        questWasActive =
            questActive;


        foreach (CollectibleItem item in collectibleItems)
        {
            if (item == null)
                continue;

            if (collectedIDs.Contains(item.ID))
            {
                item.RestoreCollected();
            }
            else
            {
                item.RestoreUncollected(
                    questActive &&
                    HasContainer &&
                    !IsCompleted
                );
            }
        }


        if (collectionPickup != null)
        {
            collectionPickup.RestoreState(
                HasContainer,
                questActive &&
                !HasContainer &&
                !IsCompleted
            );
        }


        if (emptyContainerVisual != null)
        {
            emptyContainerVisual.SetActive(
                HasContainer &&
                !IsCompleted
            );

            emptyContainerVisual.transform.localScale =
                baseEmptyContainerScale *
                (
                    1f +
                    growthPerItem *
                    collectedIDs.Count
                );
        }


        if (fullContainerVisual != null)
        {
            fullContainerVisual.SetActive(
                IsCompleted
            );
        }


        if (questActive && !IsCompleted)
        {
            if (!HasContainer)
            {
                QuestManagerV2.Instance.SetCurrentQuestText(
                    containerPickupObjective
                );
            }
            else
            {
                QuestManagerV2.Instance.RefreshCurrentQuestUI();
            }
        }
    }


    
    // use next quest in sequence, when the trash is actually thrown away.
    public void ConsumeFullContainer()
    {
        if (fullContainerVisual != null)
            fullContainerVisual.SetActive(false);
    }
}