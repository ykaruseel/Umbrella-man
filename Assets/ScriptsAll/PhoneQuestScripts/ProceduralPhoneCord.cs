using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralPhoneCord : MonoBehaviour
{
    [Header("Attachment points")]
    [SerializeField] private Transform baseConnection;
    [SerializeField] private Transform handsetConnection;

    [Header("Cable appearance")]
    [SerializeField, Range(4, 64)] private int lengthSegments = 28;
    [SerializeField, Range(3, 16)] private int radialSegments = 8;
    [SerializeField, Min(0.001f)] private float cableRadius = 0.008f;
    [SerializeField, Min(0f)] private float sag = 0.08f;
    [SerializeField, Min(0f)] private float additionalSagPerMeter = 0.09f;

    private Mesh cableMesh;
    private Vector3[] vertices;
    private int[] triangles;

    private void Awake()
    {
        cableMesh = new Mesh { name = "Generated Phone Cord" };
        cableMesh.MarkDynamic();
        GetComponent<MeshFilter>().sharedMesh = cableMesh;
        BuildIndices();
    }

    private void LateUpdate()
    {
        if (baseConnection == null || handsetConnection == null) return;
        UpdateMesh();
    }

    private Vector3 CurvePoint(float t, Vector3 a, Vector3 b)
    {
        Vector3 p = Vector3.Lerp(a, b, t);
        float cableSag = sag + Vector3.Distance(a, b) * additionalSagPerMeter;
        return p + Vector3.down * (Mathf.Sin(t * Mathf.PI) * cableSag);
    }

    private void BuildIndices()
    {
        int rings = lengthSegments + 1;
        int vertsPerRing = radialSegments + 1;
        vertices = new Vector3[rings * vertsPerRing];
        triangles = new int[lengthSegments * radialSegments * 6];

        int n = 0;
        for (int i = 0; i < lengthSegments; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int a = i * vertsPerRing + j;
                int b = (i + 1) * vertsPerRing + j;
                triangles[n++] = a;
                triangles[n++] = b;
                triangles[n++] = a + 1;
                triangles[n++] = b;
                triangles[n++] = b + 1;
                triangles[n++] = a + 1;
            }
        }
    }

    private void UpdateMesh()
    {
        Vector3 a = baseConnection.position;
        Vector3 b = handsetConnection.position;
        int vertsPerRing = radialSegments + 1;

        for (int i = 0; i <= lengthSegments; i++)
        {
            float t = (float)i / lengthSegments;
            Vector3 center = CurvePoint(t, a, b);
            Vector3 tangent = CurvePoint(Mathf.Min(1f, t + 0.01f), a, b) -
                              CurvePoint(Mathf.Max(0f, t - 0.01f), a, b);
            if (tangent.sqrMagnitude < 0.000001f) tangent = Vector3.forward;
            tangent.Normalize();

            Vector3 side = Vector3.Cross(tangent, Vector3.up);
            if (side.sqrMagnitude < 0.000001f) side = Vector3.Cross(tangent, Vector3.right);
            side.Normalize();
            Vector3 normal = Vector3.Cross(tangent, side).normalized;

            for (int j = 0; j <= radialSegments; j++)
            {
                float angle = (float)j / radialSegments * Mathf.PI * 2f;
                Vector3 worldPoint = center + cableRadius *
                    (Mathf.Cos(angle) * side + Mathf.Sin(angle) * normal);
                vertices[i * vertsPerRing + j] = transform.InverseTransformPoint(worldPoint);
            }
        }

        cableMesh.Clear();
        cableMesh.vertices = vertices;
        cableMesh.triangles = triangles;
        cableMesh.RecalculateNormals();
        cableMesh.RecalculateBounds();
    }

    private void OnDestroy()
    {
        if (cableMesh != null) Destroy(cableMesh);
    }
}
