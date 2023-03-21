using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    public CustomTerrainData terrainData;
    public Transform treePrefab;

    [Range(0, 255)] public int maxSlopeValue;
    private int _maxSlope => maxSlopeValue / 255;

    [ContextMenu("Spawn Trees")]
    public void Spawn()
    {
        SpawnTrees(Vector3.zero, terrainData.heightMap.Asset.width, terrainData.heightMap.Asset.height, terrainData.heightMap.Asset, terrainData.slopeMap.Asset, 1000);
    }

    public void SpawnTrees(Vector3 startPos, int width, int height, Texture2D heightMap, Texture2D slopeMap, int amount)
    {
        TextureMapData heightmap = new TextureMapData(heightMap);
        TextureMapData slopemap = new TextureMapData(slopeMap);

        GameObject lib = new GameObject();

        int index = 0;
        while (index < amount)
        {
            var x = Random.Range((int)startPos.x, width);
            var y = Random.Range((int)startPos.z, height);

            try{ if(slopemap.Pixels[x, y].r > _maxSlope){Debug.Log("Skipping"); continue;}}catch{/*ignored*/}
            try{ if(slopemap.Pixels[x+1, y].r > _maxSlope){Debug.Log("Skipping"); continue;}}catch{/*ignored*/}
            try{ if(slopemap.Pixels[x-1, y].r > _maxSlope){Debug.Log("Skipping"); continue;}}catch{/*ignored*/}
            try{ if(slopemap.Pixels[x, y+1].r > _maxSlope){Debug.Log("Skipping"); continue;}}catch{/*ignored*/}
            try{ if(slopemap.Pixels[x+1, y+1].r > _maxSlope){Debug.Log("Skipping"); continue;}}catch{/*ignored*/}
            try{ if(slopemap.Pixels[x-1, y+1].r > _maxSlope){Debug.Log("Skipping"); continue;}}catch{/*ignored*/}
            try{ if(slopemap.Pixels[x, y-1].r > _maxSlope){Debug.Log("Skipping"); continue;}}catch{/*ignored*/}
            try{ if(slopemap.Pixels[x+1, y-1].r > _maxSlope){Debug.Log("Skipping"); continue;}}catch{/*ignored*/}
            try{ if(slopemap.Pixels[x-1, y-1].r > _maxSlope){Debug.Log("Skipping"); continue;}}catch{/*ignored*/}

            Instantiate(treePrefab, new Vector3(x, heightmap.Pixels[x, y].r * width, y),
                treePrefab.rotation, lib.transform);
            
            index++;
        }
        
        
    }
}
