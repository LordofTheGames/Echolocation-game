using UnityEngine;

public class BakeMeshCollider : MonoBehaviour
{
    private SkinnedMeshRenderer skinnedMesh;
    private MeshCollider meshCollider;
    private Mesh bakedMesh;
    private int frameCount = 0;

    void Start()
    {
        skinnedMesh = GetComponent<SkinnedMeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        bakedMesh = new Mesh();
    }

    void Update()
    {
        frameCount++;
        if (frameCount == 10)
        {
            // Bakes the current animation frame's mesh into the empty 'bakedMesh'
            skinnedMesh.BakeMesh(bakedMesh);
            
            // Assigns that newly deformed mesh to the collider
            meshCollider.sharedMesh = bakedMesh;

            frameCount = 0;
        }

    }
}