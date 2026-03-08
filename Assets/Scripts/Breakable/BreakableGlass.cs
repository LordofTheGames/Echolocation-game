using UnityEngine;

/// <summary>
/// Museum-style glass display box that can be broken by mouse click.
/// In the final game, this will be triggered by high-pitched sound and will contain a key.
/// </summary>
public class BreakableGlass : MonoBehaviour
{
    [Header("Glass Components")]
    [Tooltip("The mesh renderers for the glass panels (will be hidden when broken)")]
    public Renderer[] glassRenderers;

    [Header("Break Effect")]
    [Tooltip("Optional: Prefab to spawn as broken glass debris (if null, procedural fragments are created)")]
    public GameObject brokenGlassPrefab;
    [Tooltip("Number of glass fragments to spawn when using procedural generation")]
    public int fragmentCount = 12;
    [Tooltip("Optional: Audio clip to play when glass breaks")]
    public AudioClip breakSound;
    [Tooltip("Optional: Particle effect for glass shards")]
    public ParticleSystem breakParticles;

    [Header("State")]
    [SerializeField] private bool isBroken;

    private AudioSource _audioSource;
    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        if (breakSound != null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        if (glassRenderers == null || glassRenderers.Length == 0)
        {
            glassRenderers = GetComponentsInChildren<Renderer>();
        }
    }

    private void OnMouseDown()
    {
        if (!isBroken)
        {
            Break();
        }
    }

    public void Break()
    {
        if (isBroken) return;
        isBroken = true;

        if (glassRenderers != null)
        {
            foreach (var renderer in glassRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
        }

        if (_collider != null)
        {
            _collider.enabled = false;
        }

        if (breakSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(breakSound);
        }

        if (brokenGlassPrefab != null)
        {
            var pos = transform.position + Vector3.down * 0.5f;
            Instantiate(brokenGlassPrefab, pos, transform.rotation);
        }
        else
        {
            SpawnProceduralFragments();
        }

        if (breakParticles != null)
        {
            breakParticles.transform.SetParent(null);
            breakParticles.Play();
            Destroy(breakParticles.gameObject, breakParticles.main.duration + 2f);
        }
    }

    private void SpawnProceduralFragments()
    {
        Material mat = null;
        if (glassRenderers != null && glassRenderers.Length > 0 && glassRenderers[0] != null)
        {
            mat = glassRenderers[0].sharedMaterial;
        }
        if (mat == null) return;

        var fragmentsRoot = new GameObject("GlassFragments");
        fragmentsRoot.transform.position = transform.position + Vector3.down * 0.6f;
        fragmentsRoot.transform.rotation = transform.rotation;

        var scatterRadius = 0.6f;
        for (int i = 0; i < fragmentCount; i++)
        {
            var shard = CreateIrregularShard();
            shard.name = "Shard" + i;
            shard.transform.SetParent(fragmentsRoot.transform);

            shard.transform.localPosition = new Vector3(
                Random.Range(-scatterRadius, scatterRadius),
                0.01f,
                Random.Range(-scatterRadius, scatterRadius)
            );
            shard.transform.localRotation = Quaternion.Euler(
                Random.Range(-15f, 15f),
                Random.Range(0, 360),
                Random.Range(-15f, 15f)
            );

            shard.GetComponent<Renderer>().sharedMaterial = mat;
            Object.Destroy(shard.GetComponent<Collider>());
        }
    }

    private static GameObject CreateIrregularShard()
    {
        var go = new GameObject();
        var mf = go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();

        var count = 5 + Random.Range(0, 3);
        var bottom = new Vector3[count];
        var radius = Random.Range(0.12f, 0.25f);
        for (int i = 0; i < count; i++)
        {
            var angle = (float)i / count * Mathf.PI * 2f + Random.Range(-0.2f, 0.2f);
            var r = radius * (0.7f + Random.Range(0f, 0.6f));
            bottom[i] = new Vector3(Mathf.Cos(angle) * r, -0.02f, Mathf.Sin(angle) * r);
        }

        var thick = Random.Range(0.02f, 0.06f);
        var verts = new Vector3[count * 2];
        var tris = new int[count * 2 * 3 + count * 6];
        for (int i = 0; i < count; i++)
        {
            verts[i] = bottom[i];
            verts[i + count] = bottom[i] + Vector3.up * thick;
        }

        var ti = 0;
        for (int i = 0; i < count; i++)
        {
            var a = i;
            var b = (i + 1) % count;
            tris[ti++] = a;
            tris[ti++] = b;
            tris[ti++] = a + count;
            tris[ti++] = b;
            tris[ti++] = b + count;
            tris[ti++] = a + count;
        }
        for (int i = 2; i < count; i++)
        {
            tris[ti++] = 0;
            tris[ti++] = i;
            tris[ti++] = i - 1;
        }
        for (int i = 2; i < count; i++)
        {
            tris[ti++] = count;
            tris[ti++] = count + i - 1;
            tris[ti++] = count + i;
        }

        mf.mesh = new Mesh
        {
            vertices = verts,
            triangles = tris
        };
        mf.mesh.RecalculateNormals();
        mf.mesh.RecalculateBounds();

        return go;
    }
}
