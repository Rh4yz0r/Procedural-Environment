using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainLoader : MonoBehaviour
{
    public Terrain terrain;
    public TerrainData dataToLoad;

    [ContextMenu("Load")]
    public void LoadDataToTerrain()
    {
        float timerStart = Time.realtimeSinceStartup;
        Debug.Log("Started Loading Terrain");
        TextureMapData textureMapData = new TextureMapData(dataToLoad.HeightMap);
        terrain.terrainData.SetHeights(0, 0, textureMapData.RedValues);
        Debug.Log($"Finished Loading Terrain in: {Time.realtimeSinceStartup - timerStart} seconds");
    }

    [ContextMenu("TEST")]
    public void Test()
    {
        float timerStart = Time.realtimeSinceStartup;
        Debug.Log("Started Test");
        TextureMapData.TextureToPixelGrid(dataToLoad.HeightMap);
        Debug.Log($"Finished Test in: {Time.realtimeSinceStartup - timerStart} seconds");
    }
    
    [ContextMenu("Set textures")]
    public void SetTextures()
    {
        /*float[,,] map = new float[terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight, 2];

        for (int y = 0; y < terrain.terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrain.terrainData.alphamapWidth; x++)
            {
                map[x, y, 0] = Color.red;
            }
        }*/
        

        terrain.terrainData.alphamapTextures[1] = dataToLoad.HeightMap;
        terrain.terrainData.SetBaseMapDirty();
        Debug.Log("Set Textures");
    }
}
