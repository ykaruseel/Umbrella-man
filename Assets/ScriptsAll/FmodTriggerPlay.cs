// Assets/Scripts/FmodTriggerPlay.cs
using UnityEngine;
using FMODUnity;

public sealed class FmodTriggerPlay : MonoBehaviour
{
    [Header("FMOD")]
    [Tooltip("FMOD Event (drag from FMOD Events browser).")]
    public EventReference eventRef;

    [Header("Filter")]
    [Tooltip("������ ������, ����� ����������� �� ����� ���������.")]
    public string requiredTag = "Player";

    [Header("Behavior")]
    [Tooltip("����������� ������ ���� ��� �� ����� �������.")]
    public bool playOnce = false;

    [Tooltip("�������� ��������������, ���� ���-�� �� ���.")]
    public bool logWarnings = true;

    private bool _played;

    
    private void Reset()
    {
        var col3D = GetComponent<Collider>();
        var col2D = GetComponent<Collider2D>();

        if (col3D == null && col2D == null)
        {
            var box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
        }
        else
        {
            if (col3D != null) col3D.isTrigger = true;
            if (col2D != null) col2D.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsAllowed(other.tag)) return;
        TryPlay();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsAllowed(other.tag)) return;
        TryPlay();
    }

    private bool IsAllowed(string tag)
    {
        if (playOnce && _played) return false;
        if (string.IsNullOrEmpty(requiredTag)) return true;
        return tag == requiredTag;
    }

    private void TryPlay()
    {
        if (eventRef.IsNull)
        {
            if (logWarnings) Debug.LogWarning("[FmodTriggerPlay] EventReference ����. ����� ����� � ����������.");
            return;
        }

        RuntimeManager.PlayOneShot(eventRef, transform.position);
        _played = true;
    }
}
