using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

//[RequireComponent(typeof(TerrainGenerator), typeof(MeshFilter), typeof(MeshRenderer))]
public class FinalTerrainGenerator : MonoBehaviour
{
    public CustomTerrainData data;
    public Transform treePrefab;

    public List<Texture2D> chunks;

    //private TerrainGenerator _terrainGenerator;
    //private MeshFilter _meshFilter;
    //private MeshRenderer _meshRenderer;
    
    private Vector3[] _terrainMeshVertices;
    
    private void OnValidate()
    {
        //_terrainGenerator = GetComponent<TerrainGenerator>();
        //_meshFilter = GetComponent<MeshFilter>();
        //_meshRenderer = GetComponent<MeshRenderer>();
    }

    [ContextMenu("Load Terrain")]
    public void Load()
    {
        if (data.heightMap.Asset.width > 256)
        {
            var chunks = NewChunker.NewChunkTexture(data.heightMap.Asset, out var chunkOffsets);
            this.chunks = chunks.ToList();

            for (int i = 0; i < chunks.Length; i++)
            {
                var chunkHeightMap = chunks[i];
                var chunkSlopeMap = FinalTextureGenerator.GenerateSlopeMap(chunkHeightMap);
                var chunkTextureMap = FinalTextureGenerator.GenerateTextureMap(chunkHeightMap, chunkSlopeMap.Texture2D);
                
                var terrainChunkGameObject = CreateTerrainGameObject($"Chunk: {i}", transform.position+chunkOffsets[i]);
                var treeChunkGameObject = CreateTreeLibraryGameObject($"Tree Chunk: {i}", transform.position + chunkOffsets[i]);
                LoadTerrainMesh(chunkHeightMap, chunkSlopeMap.Texture2D, chunkTextureMap.Texture2D, terrainChunkGameObject);
                LoadTextures(chunkHeightMap, chunkSlopeMap.Texture2D, chunkTextureMap.Texture2D, terrainChunkGameObject);
                LoadTrees(chunkHeightMap, chunkSlopeMap.Texture2D, chunkTextureMap.Texture2D, terrainChunkGameObject, treeChunkGameObject);
            }
        }
        else
        {
            var terrainGameObject = CreateTerrainGameObject("New Terrain", transform.position);
            var treeChunkGameObject = CreateTreeLibraryGameObject($"New Trees", transform.position);
            LoadTerrainMesh(data.heightMap.Asset, data.slopeMap.Asset, data.textureMap.Asset, terrainGameObject);
            LoadTextures(data.heightMap.Asset, data.slopeMap.Asset, data.textureMap.Asset, terrainGameObject);
            LoadTrees(data.heightMap.Asset, data.slopeMap.Asset, data.textureMap.Asset, terrainGameObject, treeChunkGameObject);
        }
    }

    public GameObject CreateTerrainGameObject(string gameObjectName, Vector3 position)
    {
        GameObject newTerrainGameObject = new GameObject(){name = gameObjectName, transform = { parent = transform, position = position}};
        newTerrainGameObject.AddComponent<MeshFilter>();
        newTerrainGameObject.AddComponent<MeshRenderer>();

        return newTerrainGameObject;
    }
    
    public GameObject CreateTreeLibraryGameObject(string gameObjectName, Vector3 position)
    {
        GameObject newTerrainGameObject = new GameObject(){name = gameObjectName, transform = { parent = transform, position = position}};
        return newTerrainGameObject;
    }
    
    public void LoadTerrainMesh(Texture2D heightMapTexture, Texture2D slopeMapTexture, Texture2D groundTexture, GameObject terrainGameObject)
    {
        //Determine size
        var width = heightMapTexture.width - 1;
        var height = heightMapTexture.height - 1;
        
        //Create new Mesh
        var mesh = new Mesh(){ name = "New Terrain" };
        terrainGameObject.GetComponent<MeshFilter>().mesh = mesh;

        //Create dataset
        TextureMapData heightMap = new TextureMapData(heightMapTexture);
        
        //Set vertices and uv
        _terrainMeshVertices = new Vector3[(width + 1) * (height + 1)];
        Vector2[] uv = new Vector2[_terrainMeshVertices.Length];
        for (int i = 0, y = 0; y < height+1; y++) 
        {
            for (int x = 0; x < width+1; x++, i++) 
            {
                _terrainMeshVertices[i] = new Vector3(x, heightMap.Pixels[x, y].r*heightMap.Width, y);
                uv[i] = new Vector2((float)x / width, (float)y / height);
            }
        }
        mesh.vertices = _terrainMeshVertices;
        mesh.uv = uv;
        
        //Set triangles and normals
        int[] triangles = new int[width * height * 6];
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

    public void LoadTextures(Texture2D heightMapTexture, Texture2D slopeMapTexture, Texture2D groundTexture, GameObject terrainGameObject)
    {
        Material chunkMat = new Material(Shader.Find("Standard"));
        chunkMat.mainTexture = groundTexture;
        chunkMat.name = $"{groundTexture}";
        terrainGameObject.GetComponent<MeshRenderer>().material = chunkMat;
    }

    public void LoadTrees(Texture2D heightMapTexture, Texture2D slopeMapTexture, Texture2D groundTexture, GameObject terrainGameObject, GameObject treeLibraryGameObject)
    {
        var width = heightMapTexture.width;
        var height = heightMapTexture.height;
        var amount = 100;
        var startPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        var minHeight = 25;
        var _maxSlope = (float)16 / 255;
        
        TextureMapData heightmap = new TextureMapData(heightMapTexture);
        TextureMapData slopemap = new TextureMapData(slopeMapTexture);

        int index = 0;
        while (index < amount)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Spawning Trees", $"Busy Spawning Trees: {index}/{amount}",
                    (float)index / amount)) break;
            
            var x = Random.Range(0, width);
            var y = Random.Range(0, height);

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
                Instantiate(treePrefab, new Vector3(x+treeLibraryGameObject.transform.position.x, (heightmap.Pixels[x, y].r * width) + treeLibraryGameObject.transform.position.y, y+treeLibraryGameObject.transform.position.z),
                    treePrefab.rotation, treeLibraryGameObject.transform);
            else continue;
            
            index++;
        }
        
        EditorUtility.ClearProgressBar();
    }

}
