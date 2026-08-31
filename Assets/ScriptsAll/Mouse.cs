using System;
using System.Collections.Generic;
using UnityEngine;

public class Mouse : MonoBehaviour
{
    public List<Transform> points = new List<Transform>();

    [SerializeField] private float speed = 1f;
    [SerializeField] private bool loop = false;

    [SerializeField] private float lookAhead = 0.05f;
    [SerializeField] private float rotationSmooth = 5f;

    float distanceTravelled = 0f;
    float totalLength;
    List<float> segmentLengths = new List<float>();

    public event Action OnPathFinished;
    bool finished;

    void Start()
    {
        CalculateLengths();
        SetInitialRotation();
        distanceTravelled = 0f;
        finished = false;
    }

    void OnValidate()
    {
        CalculateLengths();
    }

    void SetInitialRotation()
    {
        if (points == null || points.Count < 4 || totalLength <= 0f)
            return;

        float startDistance = 0f;

        Vector3 pos = GetPosition(DistanceToT(startDistance));
        Vector3 nextPos = GetPosition(
            DistanceToT(Mathf.Min(startDistance + lookAhead, totalLength))
        );

        Vector3 dir = (nextPos - pos).normalized;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    void Update()
    {
        if (points.Count < 4 || totalLength <= 0f || finished)
            return;

        distanceTravelled += Time.deltaTime * speed;

        if (distanceTravelled >= totalLength)
        {
            distanceTravelled = totalLength;
            finished = true;
            gameObject.SetActive(false);
        }

        float t = DistanceToT(distanceTravelled);

        Vector3 pos = GetPosition(t);

        Vector3 nextPos = GetPosition(
            DistanceToT(Mathf.Min(distanceTravelled + lookAhead, totalLength))
        );

        transform.position = pos;

        Vector3 dir = (nextPos - pos).normalized;
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir) * Quaternion.Euler(90, 0, 0);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * rotationSmooth
            );
        }
    }

    void CalculateLengths()
    {
        segmentLengths.Clear();
        totalLength = 0f;

        int segments = points.Count - 3;

        for (int i = 0; i < segments; i++)
        {
            float length = 0f;
            Vector3 prev = GetPosition(i);

            for (int j = 1; j <= 20; j++)
            {
                float t = i + j / 20f;
                Vector3 p = GetPosition(t);
                length += Vector3.Distance(prev, p);
                prev = p;
            }

            segmentLengths.Add(length);
            totalLength += length;
        }
    }

    float DistanceToT(float distance)
    {
        float accumulated = 0f;

        for (int i = 0; i < segmentLengths.Count; i++)
        {
            if (accumulated + segmentLengths[i] >= distance)
            {
                float local = (distance - accumulated) / segmentLengths[i];
                return i + local;
            }
            accumulated += segmentLengths[i];
        }

        return segmentLengths.Count;
    }

    Vector3 GetPosition(float t)
    {
        int i = Mathf.Clamp(Mathf.FloorToInt(t), 0, points.Count - 4);
        float localT = t - i;

        Vector3 p0 = points[i].position;
        Vector3 p1 = points[i + 1].position;
        Vector3 p2 = points[i + 2].position;
        Vector3 p3 = points[i + 3].position;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * localT +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * localT * localT +
            (-p0 + 3f * p1 - 3f * p2 + p3) * localT * localT * localT
        );
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (points == null || points.Count < 4)
            return;

        Gizmos.color = Color.cyan;
        Vector3 prev = GetPosition(0f);

        for (float d = 0; d < totalLength; d += totalLength / 200f)
        {
            Vector3 pos = GetPosition(DistanceToT(d));
            Gizmos.DrawLine(prev, pos);
            prev = pos;
        }
    }
#endif
}
