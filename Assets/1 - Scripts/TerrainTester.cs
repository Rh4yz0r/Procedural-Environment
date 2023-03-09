using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTester : MonoBehaviour
{
    public Terrain terrain;
    private UnityEngine.TerrainData _terrainData;

    public MeshRenderer heightMap;
    public MeshRenderer alphaMap2;
    public MeshRenderer alphaMap1;
    
    // Start is called before the first frame update
    void Start()
    {
        _terrainData = terrain.terrainData;
        
        heightMap.material.mainTexture = _terrainData.heightmapTexture;
        alphaMap1.material.mainTexture = _terrainData.alphamapTextures[0];
        alphaMap2.material.mainTexture = _terrainData.alphamapTextures[1];
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
