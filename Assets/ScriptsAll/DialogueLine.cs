using UnityEngine;


[System.Serializable] // Эта строчка важна, чтобы мы могли редактировать диалоги в инспекторе

public class DialogueLine
{
    public string speakerName; // Имя того, кто говорит

    [TextArea(3, 10)] // Делает текстовое поле в инспекторе больше и удобнее
    public string sentence; // Сама реплика
}
