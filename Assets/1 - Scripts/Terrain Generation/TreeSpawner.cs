using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    public CustomTerrainData terrainData;
    public Transform treePrefab;

    public int treeAmount = 100;
    
    [Range(0, 255)] public int maxSlopeValue;

    public int minHeight = 0;
    private float _maxSlope => (float)maxSlopeValue / 255;

    [ContextMenu("Spawn Trees")]
    public void Spawn()
    {
        SpawnTrees(Vector3.zero, terrainData.heightMap.Asset.width, terrainData.heightMap.Asset.height, terrainData.heightMap.Asset, terrainData.slopeMap.Asset, treeAmount);
    }

    public void SpawnTrees(Vector3 startPos, int width, int height, Texture2D heightMap, Texture2D slopeMap, int amount)
    {
        TextureMapData heightmap = new TextureMapData(heightMap);
        TextureMapData slopemap = new TextureMapData(slopeMap);

        GameObject lib = new GameObject(){name = "Trees"};

        int index = 0;
        while (index < amount)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Spawning Trees", $"Busy Spawning Trees: {index}/{amount}",
                    (float)index / amount)) break;
            
            var x = Random.Range((int)startPos.x, width);
            var y = Random.Range((int)startPos.z, height);

            //Debug.Log($"Slope: {slopemap.Pixels[x    , y    ].r}");
            
            try{ if(slopemap.Pixels[x    , y    ].r > _maxSlope){continue;}}catch{/*ignored*/}
            try{ if(slopemap.Pixels[x + 1, y    ].r > _maxSlope){continue;}}catch{/*ignored*/}
            try{ if(slopemap.Pixels[x - 1, y    ].r > _maxSlope){continue;}}catch{/*ignored*/}
            try{ if(slopemap.Pixels[x    , y + 1].r > _maxSlope){continue;}}catch{/*ignored*/}
            try{ if(slopemap.Pixels[x + 1, y + 1].r > _maxSlope){continue;}}catch{/*ignored*/}
            try{ if(slopemap.Pixels[x - 1, y + 1].r > _maxSlope){continue;}}catch{/*ignored*/}
            try{ if(slopemap.Pixels[x    , y - 1].r > _maxSlope){continue;}}catch{/*ignored*/}
            try{ if(slopemap.Pixels[x + 1, y - 1].r > _maxSlope){continue;}}catch{/*ignored*/}
            try{ if(slopemap.Pixels[x - 1, y - 1].r > _maxSlope){continue;}}catch{/*ignored*/}

            if (heightmap.Pixels[x, y].r >= (float)minHeight / heightmap.Width)
                Instantiate(treePrefab, new Vector3(x, (heightmap.Pixels[x, y].r * width) + transform.position.y, y),
                    treePrefab.rotation, lib.transform);
            else continue;
            
            index++;
        }
        
        EditorUtility.ClearProgressBar();
    }
}
