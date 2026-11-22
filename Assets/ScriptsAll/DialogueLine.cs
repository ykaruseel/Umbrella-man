using UnityEngine;


[System.Serializable] 

public class DialogueLine
{
    public string speakerName; 
    public FMODUnity.EventReference voiceEvent;

    [TextArea(3, 10)] 
    public string sentence;
}
