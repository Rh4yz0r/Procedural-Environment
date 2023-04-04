using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

//[RequireComponent(typeof(TerrainGenerator), typeof(MeshFilter), typeof(MeshRenderer))]
public class FinalTerrainGenerator : MonoBehaviour
{
    public CustomTerrainData data;
    public List<Transform> treePrefabs;
    public int treeAmount = 100;
    public Transform shrubPrefab;

    public List<Texture2D> chunks;
    public List<Texture2D> chunkSlopes;
    public List<Texture2D> chunkNormals;
    public List<Texture2D> chunkTextures;

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
        this.chunks.Clear();
        this.chunkNormals.Clear();
        this.chunkSlopes.Clear();
        this.chunkTextures.Clear();
        
        
        if (data.heightMap.Asset.width > 256)
        {
            var heightMapChunks = NewChunker.NewChunkTexture(data.heightMap.Asset, out var chunkOffsets);
            this.chunks = heightMapChunks.ToList();

            var slopeMap = FinalTextureGenerator.NewSlopeMap(data.heightMap.Asset);
            var slopeMapChunks = NewChunker.NewChunkTexture(slopeMap, out var redundant);
            this.chunkSlopes = slopeMapChunks.ToList();

            var textureMap = FinalTextureGenerator.GenerateTextureMap(data.heightMap.Asset, slopeMap.Texture2D);
            var textureMapChunks = NewChunker.NewChunkTexture(textureMap, out var redundant2);
            this.chunkTextures = textureMapChunks.ToList();

            for (int i = 0; i < heightMapChunks.Length; i++)
            {
                var chunkHeightMap = heightMapChunks[i];
                var chunkSlopeMap = slopeMapChunks[i];
                var chunkTextureMap = textureMapChunks[i];
                //var chunkSlopeMap = FinalTextureGenerator.GenerateSlopeMap(chunkHeightMap);
                //chunkSlopes.Add(chunkSlopeMap.Texture2D);
                //var chunkTextureMap = FinalTextureGenerator.GenerateTextureMap(chunkHeightMap, chunkSlopeMap);
                //chunkTextures.Add(chunkTextureMap.Texture2D);
                
                var terrainChunkGameObject = CreateTerrainGameObject($"Chunk: {i}", transform.position + chunkOffsets[i]);
                var treeChunkGameObject = CreateLibraryGameObject($"Tree Chunk: {i}", transform.position + chunkOffsets[i]);
                var shrubChunkGameObject = CreateLibraryGameObject($"Shrub Chunk: {i}", transform.position + chunkOffsets[i]);
                LoadTerrainMesh(chunkHeightMap, chunkSlopeMap, chunkTextureMap, terrainChunkGameObject, out var normalMap);
                chunkNormals.Add(normalMap);
                LoadTextures(chunkHeightMap, chunkSlopeMap, chunkTextureMap, terrainChunkGameObject);
                LoadTrees(chunkHeightMap, chunkSlopeMap, chunkTextureMap, terrainChunkGameObject, treeChunkGameObject);
                LoadShrubs(chunkHeightMap, chunkSlopeMap, chunkTextureMap, terrainChunkGameObject, shrubChunkGameObject, normalMap);
            }
        }
        else
        {
            var chunkSlopeMap = FinalTextureGenerator.NewSlopeMap(data.heightMap.Asset);
            var chunkTextureMap = FinalTextureGenerator.GenerateTextureMap(data.heightMap.Asset, chunkSlopeMap.Texture2D);
            chunks.Add(data.heightMap.Asset);
            chunkSlopes.Add(chunkSlopeMap.Texture2D);
            chunkTextures.Add(chunkTextureMap.Texture2D);
            
            var terrainGameObject = CreateTerrainGameObject("New Terrain", transform.position);
            var treeChunkGameObject = CreateLibraryGameObject($"New Trees", transform.position);
            var shrubChunkGameObject = CreateLibraryGameObject($"New Shrubs", transform.position);
            LoadTerrainMesh(data.heightMap.Asset, chunkSlopeMap.Texture2D, chunkTextureMap.Texture2D, terrainGameObject, out var normalMap);
            chunkNormals.Add(normalMap);
            LoadTextures(data.heightMap.Asset, chunkSlopeMap.Texture2D, chunkTextureMap.Texture2D, terrainGameObject);
            LoadTrees(data.heightMap.Asset, chunkSlopeMap.Texture2D, chunkTextureMap.Texture2D, terrainGameObject, treeChunkGameObject);
            LoadShrubs(data.heightMap.Asset, chunkSlopeMap.Texture2D, chunkTextureMap.Texture2D, terrainGameObject, shrubChunkGameObject, normalMap);
        }
    }

    public GameObject CreateTerrainGameObject(string gameObjectName, Vector3 position)
    {
        GameObject newTerrainGameObject = new GameObject(){name = gameObjectName, transform = { parent = transform, position = position}};
        newTerrainGameObject.AddComponent<MeshFilter>();
        newTerrainGameObject.AddComponent<MeshRenderer>();

        return newTerrainGameObject;
    }
    
    public GameObject CreateLibraryGameObject(string gameObjectName, Vector3 position)
    {
        GameObject newTerrainGameObject = new GameObject(){name = gameObjectName, transform = { parent = transform, position = position}};
        return newTerrainGameObject;
    }

    public void LoadTerrainMesh(Texture2D heightMapTexture, Texture2D slopeMapTexture, Texture2D groundTexture, GameObject terrainGameObject, out Texture2D normalTexture)
    {
        Texture2D normalMap = new Texture2D(heightMapTexture.width, heightMapTexture.height, TextureFormat.RGBA32, -1,
            false);
        TextureMapData normalMapData = new TextureMapData(normalMap);
        var normalPixels = normalMapData.Pixels;
        
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
                EditorUtility.DisplayProgressBar("Creating terrain", $"Calculating vertices & uv: {(x + (y * width))}/{(width+1) * (height+1)}",
                        (float)(x + (y * (width+1))) / ((width+1) * (height+1)));
                
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
                EditorUtility.DisplayProgressBar("Creating terrain", $"Calculating triangles: {(x + (y * width))}/{width * height}",
                    (float)(x + (y * width)) / (width * height));
                
                triangles[triIndex] = vertIndex;
                triangles[triIndex + 3] = triangles[triIndex + 2] = vertIndex + 1;
                triangles[triIndex + 4] = triangles[triIndex + 1] = vertIndex + width + 1;
                triangles[triIndex + 5] = vertIndex + width + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        for (int i = 0, y = 0; y < height+1; y++) 
        {
            for (int x = 0; x < width+1; x++, i++) 
            {
                EditorUtility.DisplayProgressBar("Creating terrain", $"Calculating normals: {(x + (y * width))}/{(width+1) * (height+1)}",
                    (float)(x + (y * (width+1))) / ((width+1) * (height+1)));
                
                normalPixels[x, y] = new Color(mesh.normals[i].x, mesh.normals[i].y, mesh.normals[i].z);
            }
        }
        normalMapData.Pixels = normalPixels;

        normalTexture = normalMapData.Texture2D;
        EditorUtility.ClearProgressBar();
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
        var amount = treeAmount;
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
            {
                int rng = Random.Range(0, treePrefabs.Count);

                Instantiate(treePrefabs[rng], new Vector3(x+treeLibraryGameObject.transform.position.x, (heightmap.Pixels[x, y].r * width) + treeLibraryGameObject.transform.position.y, y+treeLibraryGameObject.transform.position.z),
                    treePrefabs[rng].rotation, treeLibraryGameObject.transform);
                
            }
            else continue;
            
            index++;
        }
        
        EditorUtility.ClearProgressBar();
    }

    public void LoadShrubs(Texture2D heightMapTexture, Texture2D slopeMapTexture, Texture2D groundTexture, GameObject terrainGameObject, GameObject shrubLibraryGameObject, Texture2D normalMap)
    {
        float minConstraintShrub = 0.7f;
        var minHeight = 25;
        
        TextureMapData heightmap = new TextureMapData(heightMapTexture);
        TextureMapData groundTextureData = new TextureMapData(groundTexture);
        TextureMapData normalMapData = new TextureMapData(normalMap);
        
        for (int y = 0; y < groundTexture.height; y++)
        {
            for (int x = 0; x < groundTexture.width; x++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Spawning Shrubs", $"Busy Spawning Shrubs: {(x + (y * groundTexture.height))}/{groundTexture.width * groundTexture.height}",
                        (float)(x + (y * groundTexture.height)) / groundTexture.width * groundTexture.height)) break;

                var rng = Random.Range(0, 2);
                
                if (groundTextureData.Pixels[x, y].g > minConstraintShrub && rng == 1)
                {
                    if (heightmap.Pixels[x, y].r >= (float)minHeight / heightmap.Width)
                    {
                        var shrub = Instantiate(shrubPrefab, new Vector3(x+shrubLibraryGameObject.transform.position.x, (heightmap.Pixels[x, y].r * heightMapTexture.width) + shrubLibraryGameObject.transform.position.y, y+shrubLibraryGameObject.transform.position.z),
                            shrubPrefab.rotation, shrubLibraryGameObject.transform);
                        var normalPixel = normalMapData.Pixels[x, y];
                        shrub.up = new Vector3(normalPixel.r, normalPixel.g, normalPixel.b);
                    }
                }
            }
        }
        
        EditorUtility.ClearProgressBar();
    }

}
