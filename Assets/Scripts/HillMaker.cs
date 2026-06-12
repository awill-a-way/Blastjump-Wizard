using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class HillMaker : MonoBehaviour
{
    [Header("Static Settings")]
    [SerializeField] public float noiseScale = 0.01f;
    [SerializeField] public float heightAmplitude = 10f;
    [SerializeField] public float seedOffset = 0f;

    Mesh mesh;
    Vector3[] vertices;
    bool applied = false;

    void Awake()
    {
        // Important: Avoid modifying the shared mesh asset
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void Start()
    {
        ApplyStaticDeformation();
    }

    void ApplyStaticDeformation()
    {
        if (mesh == null || applied)
            return;

        vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            // Convert vertex to world position
            Vector3 worldPos = transform.TransformPoint(vertices[i]);

            float stHeight = GetStaticHeight(worldPos.x, worldPos.z);

            // IMPORTANT: Assign, don't accumulate
            vertices[i].y = stHeight;
        }

        mesh.vertices = vertices;
        //mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        applied = true;
    }

    public float GetStaticHeight(float x, float z)
    {
        float noise = Mathf.PerlinNoise(
            x * noiseScale + seedOffset,
            z * noiseScale + seedOffset
        );

        return noise * heightAmplitude;
    }

    void OnDestroy()
    {
    if (mesh != null)
    {
        Destroy(mesh);
    }
    }
}