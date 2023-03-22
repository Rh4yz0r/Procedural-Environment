using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(TerrainGenerator))]
public class TerrainLoader : MonoBehaviour
{
    private TerrainGenerator _generator;

    public CustomTerrainData dataToLoad;

    private void OnValidate()
    {
        _generator = GetComponent<TerrainGenerator>();
    }

    /*[ContextMenu("Load Terrain")]
    public void LoadTerrainData()
    {
        TerrainMeshSettings settings = new TerrainMeshSettings();
        settings.TerrainSize = new Vector2Int(dataToLoad.heightMap.Asset.width-1, dataToLoad.heightMap.Asset.height-1);
        settings.QuadSize = 1;
        
        _generator.GenerateTerrainMeshWithHeight(settings, new TextureMapData(dataToLoad.heightMap.Asset));
    }*/

    Vector3[] vertices;
    
    /*private void OnDrawGizmos () 
    {
        if (vertices == null) 
        {
            return;
        }
        
        Gizmos.color = Color.black;
        for (int i = 0; i < vertices.Length; i++) 
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }*/
    
    [ContextMenu("Load New Terrain")]
    public void LoadNewTerrain()
    {
        
        Vector2[] uv;
        int[] triangles;
        
        var width = dataToLoad.heightMap.Asset.width - 1;
        var height = dataToLoad.heightMap.Asset.height - 1;
        
        var mesh = new Mesh();
        mesh.name = "New Terrain";
        GetComponent<MeshFilter>().mesh = mesh;

        TextureMapData heightMap = new TextureMapData(dataToLoad.heightMap.Asset);
        
        vertices = new Vector3[(width + 1) * (height + 1)];
        uv = new Vector2[vertices.Length];
        for (int i = 0, y = 0; y < height+1; y++) 
        {
            for (int x = 0; x < width+1; x++, i++) 
            {
                vertices[i] = new Vector3(x, heightMap.Pixels[x, y].r*heightMap.Width, y);
                uv[i] = new Vector2((float)x / width, (float)y / height);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        
        triangles = new int[width * height * 6];
        for (int triIndex = 0, vertIndex = 0, y = 0; y < height; y++, vertIndex++) 
        {
            for (int x = 0; x < width; x++, triIndex += 6, vertIndex++) 
            {
                triangles[triIndex] = vertIndex;
                triangles[triIndex + 3] = triangles[triIndex + 2] = vertIndex + 1;
                triangles[triIndex + 4] = triangles[triIndex + 1] = vertIndex + width + 1;
                triangles[triIndex + 5] = vertIndex + width + 2;
            }
        }

        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }


    //public Terrain terrain;
    //public CustomTerrainData dataToLoad;

    /*[ContextMenu("Load")]
    public void LoadDataToTerrain()
    {
        float timerStart = Time.realtimeSinceStartup;
        Debug.Log("Started Loading Terrain");
        //TextureMapData textureMapData = new TextureMapData(dataToLoad.HeightMap);
        //terrain.terrainData.SetHeights(0, 0, textureMapData.RedValues);

        try
        {
            float wantedMaxHeight = 600;
            float TerrainHeight = wantedMaxHeight;
            
            float HeightmapRemapMax = 600;
            float HeightmapRemapMin = 0;
            
            // heightmap remap
            var remap = (HeightmapRemapMax - HeightmapRemapMin) / TerrainHeight;
            var baseLevel = HeightmapRemapMin / TerrainHeight;
            
            CopyTextureToTerrainHeight(terrain.terrainData, dataToLoad.heightMap.Asset, new Vector2Int(0, 0), terrain.terrainData.heightmapResolution, 1, baseLevel, remap);
        
            if (terrain.terrainData.heightmapResolution != dataToLoad.heightMap.Asset.width)
            {
                ResizeHeightmap(terrain.terrainData, dataToLoad.heightMap.Asset.width);
            }
        }
        finally
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        Debug.Log($"Finished Loading Terrain in: {Time.realtimeSinceStartup - timerStart} seconds");
    }

    [ContextMenu("TEST")]
    public void Test()
    {
        float timerStart = Time.realtimeSinceStartup;
        Debug.Log("Started Test");
        TextureMapData.TextureToPixelGrid(dataToLoad.heightMap.Asset);
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
        }#1#
        
        //terrain.terrainData.splatPrototypes
        terrain.terrainData.alphamapTextures[1] = dataToLoad.slopeMap.Asset;
        terrain.terrainData.SetBaseMapDirty();
        Debug.Log("Set Textures");
    }
    
    public static float kNormalizedHeightScale => 32766.0f / 65535.0f;
    public static void CopyTextureToTerrainHeight(TerrainData terrainData, Texture2D heightmap, Vector2Int indexOffset, int resolution, int numTiles, float baseLevel, float remap)
    {
        terrainData.heightmapResolution = resolution + 1;

        float hWidth = heightmap.height;
        float div = hWidth / numTiles;

        float scale = ((resolution / (resolution + 1.0f)) * (div + 1)) / hWidth;
        float offset = ((resolution / (resolution + 1.0f)) * div) / hWidth;

        Vector2 scaleV = new Vector2(scale, scale);
        Vector2 offsetV = new Vector2(offset * indexOffset.x, offset * indexOffset.y);

        Material blitMaterial = GetHeightBlitMaterial();
        blitMaterial.SetFloat("_Height_Offset", baseLevel * kNormalizedHeightScale);
        blitMaterial.SetFloat("_Height_Scale", remap * kNormalizedHeightScale);
        RenderTexture heightmapRT = RenderTexture.GetTemporary(terrainData.heightmapTexture.descriptor);
        Graphics.Blit(heightmap, heightmapRT/*, blitMaterial#1#);

        Graphics.Blit(heightmapRT, terrainData.heightmapTexture, scaleV, offsetV);

        terrainData.DirtyHeightmapRegion(new RectInt(0, 0, terrainData.heightmapTexture.width, terrainData.heightmapTexture.height), TerrainHeightmapSyncControl.HeightAndLod);
        RenderTexture.ReleaseTemporary(heightmapRT);
    }
    
    public static Material GetHeightBlitMaterial()
    {
        return new Material(Shader.Find("Hidden/TerrainTools/HeightBlit"));
    }
    
    public static void ResizeHeightmap(TerrainData terrainData, int resolution)
        {
            RenderTexture oldRT = RenderTexture.active;

            RenderTexture oldHeightmap = RenderTexture.GetTemporary(terrainData.heightmapTexture.descriptor);
            Graphics.Blit(terrainData.heightmapTexture, oldHeightmap);

#if UNITY_2019_3_OR_NEWER
            // terrain holes
            RenderTexture oldHoles = RenderTexture.GetTemporary(terrainData.holesTexture.width, terrainData.holesTexture.height);
            Graphics.Blit(terrainData.holesTexture, oldHoles);
#endif

            Undo.RegisterCompleteObjectUndo(terrainData, "Resize heightmap");

            float sUV = 1.0f;
            int dWidth = terrainData.heightmapResolution;
            int sWidth = resolution;

            Vector3 oldSize = terrainData.size;
            terrainData.heightmapResolution = resolution;
            terrainData.size = oldSize;

            oldHeightmap.filterMode = FilterMode.Bilinear;

            // Make sure textures are offset correctly when resampling
            // tsuv = (suv * swidth - 0.5) / (swidth - 1)
            // duv = (tsuv(dwidth - 1) + 0.5) / dwidth
            // duv = (((suv * swidth - 0.5) / (swidth - 1)) * (dwidth - 1) + 0.5) / dwidth
            // k = (dwidth - 1) / (swidth - 1) / dwidth
            // duv = suv * (swidth * k)		+ 0.5 / dwidth - 0.5 * k

            float k = (dWidth - 1.0f) / (sWidth - 1.0f) / dWidth;
            float scaleX = sUV * (sWidth * k);
            float offsetX = (float)(0.5 / dWidth - 0.5 * k);
            Vector2 scale = new Vector2(scaleX, scaleX);
            Vector2 offset = new Vector2(offsetX, offsetX);

            Graphics.Blit(oldHeightmap, terrainData.heightmapTexture, scale, offset);
            RenderTexture.ReleaseTemporary(oldHeightmap);

#if UNITY_2019_3_OR_NEWER
            oldHoles.filterMode = FilterMode.Point;
            Graphics.Blit(oldHoles, (RenderTexture)terrainData.holesTexture);
            RenderTexture.ReleaseTemporary(oldHoles);
#endif

            RenderTexture.active = oldRT;

            terrainData.DirtyHeightmapRegion(new RectInt(0, 0, terrainData.heightmapTexture.width, terrainData.heightmapTexture.height), TerrainHeightmapSyncControl.HeightAndLod);
#if UNITY_2019_3_OR_NEWER
            terrainData.DirtyTextureRegion(TerrainData.HolesTextureName, new RectInt(0, 0, terrainData.holesTexture.width, terrainData.holesTexture.height), false);
#endif
        }*/
}
