using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkingTester : MonoBehaviour
{
    /*public List<GameObject> chunkPlanes;
    public Texture2D Texture512;
    public List<Texture2D> newTexes;

    [ContextMenu("Chunk texture")]
    public void ChunkingTest()
    {
        var chunks = TextureMapGenerator.ChunkTexture(Texture512);
        newTexes = chunks.ToList();

        for (int i = 0; i < chunks.Length; i++)
        {
            Debug.Log($"Setting chunk {i}");
            var meshRenderer = chunkPlanes[i].GetComponent<MeshRenderer>();
            Material chunkMat = new Material(Shader.Find("Standard"));
            chunkMat.mainTexture = chunks[i];
            chunkMat.name = $"Chunk{i}";
            meshRenderer.material = chunkMat;
        }
    }*/
}
