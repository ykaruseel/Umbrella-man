public static class SceneTransitionState
{
    public static bool HasPendingTransition { get; private set; }

    public static string SpawnPointID { get; private set; }

    public static string ActiveQuestID { get; private set; }

    public static bool HasFullTrashBag { get; private set; }


    public static void Prepare(
        string spawnPointID,
        string activeQuestID,
        bool hasFullTrashBag)
    {
        HasPendingTransition = true;

        SpawnPointID = spawnPointID;
        ActiveQuestID = activeQuestID;
        HasFullTrashBag = hasFullTrashBag;
    }


    public static void Clear()
    {
        HasPendingTransition = false;

        SpawnPointID = "";
        ActiveQuestID = "";
        HasFullTrashBag = false;
    }
}