using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class DeformableMesh : MonoBehaviour
{
    [Header("Deformation")]
    [SerializeField] private float deformRadius = 0.5f;
    [SerializeField] private float maxDeformForce = 0.3f;
    [SerializeField] private float breakThreshold = 2f;

    [Header("Fragments")]
    [SerializeField] private GameObject[] fragmentPrefabs;
    [SerializeField] private float fragmentExplosionForce = 5f;

    private Mesh _mesh;
    private Vector3[] _originalVertices;
    private Vector3[] _vertices;
    private MeshCollider _meshCollider;
    private float _totalDeformation;

    private void Awake()
    {
        var meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();

        // Convex = обязательно для Rigidbody + MeshCollider
        _meshCollider.convex = true;

        _mesh = Instantiate(meshFilter.sharedMesh);
        meshFilter.mesh = _mesh;

        _originalVertices = _mesh.vertices;
        _vertices = (Vector3[])_originalVertices.Clone();
    }

    public void Deform(Vector3 worldPoint, float force)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        float clampedForce = Mathf.Clamp(force, 0f, maxDeformForce);
        bool changed = false;

        for (int i = 0; i < _vertices.Length; i++)
        {
            float dist = Vector3.Distance(_vertices[i], localPoint);
            if (dist < deformRadius)
            {
                float influence = 1f - dist / deformRadius;
                Vector3 direction = (_vertices[i] - localPoint).normalized;
                _vertices[i] += direction * clampedForce * influence;
                _totalDeformation += clampedForce * influence * Time.deltaTime;
                changed = true;
            }
        }

        if (!changed) return;

        _mesh.vertices = _vertices;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _meshCollider.sharedMesh = null;
        _meshCollider.sharedMesh = _mesh;

        if (_totalDeformation >= breakThreshold)
            BreakApart();
    }

    private void BreakApart()
    {
        if (fragmentPrefabs != null && fragmentPrefabs.Length > 0)
        {
            foreach (var prefab in fragmentPrefabs)
            {
                var fragment = Instantiate(prefab, transform.position, Random.rotation);
                if (fragment.TryGetComponent<Rigidbody>(out var rb))
                    rb.AddExplosionForce(fragmentExplosionForce, transform.position, 2f);
            }
        }
        Destroy(gameObject);
    }

    public void ResetDeformation()
    {
        System.Array.Copy(_originalVertices, _vertices, _vertices.Length);
        _mesh.vertices = _vertices;
        _mesh.RecalculateNormals();
        _meshCollider.sharedMesh = null;
        _meshCollider.sharedMesh = _mesh;
        _totalDeformation = 0f;
    }
}
