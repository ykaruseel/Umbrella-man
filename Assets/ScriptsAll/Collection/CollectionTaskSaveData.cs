using System;
using System.Collections.Generic;

[Serializable]
public class CollectionTaskSaveData
{
    public string questID;

    public bool hasContainer;

    public bool isCompleted;

    public List<string> collectedItemIDs =
        new List<string>();
}