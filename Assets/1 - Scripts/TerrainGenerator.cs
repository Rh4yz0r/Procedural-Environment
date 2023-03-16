using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Depiction of Quad and Tris used for calculation.
 
        C --------- D
        |        /  |
        |     /     |
        |  /        |
        A --------- B
 
    Tri 1 = ABD
    Tri 2 = ADC
 
 */

[Serializable]
public class TerrainMeshSettings
{
    public Vector2Int TerrainSize = Vector2Int.one;
    [Range(1, 1000)] public int TerrainDetail = 1;

    public int WidthTotal => TerrainSize.x;
    public int HeightTotal => TerrainSize.y;

    public float QuadSize => 1f / TerrainDetail;

    public int QuadsX => TerrainSize.x * TerrainDetail;
    public int QuadsY => TerrainSize.y * TerrainDetail;
    public int QuadsTotal => QuadsX * QuadsY;
    

    public int VerticesX => QuadsX + 1;
    public int VerticesY => QuadsY + 1;
    public int VerticesTotal => (QuadsX + 1) * (QuadsY + 1);
}

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    private const bool ENABLE_CONSOLE_LOGS = true;
#endif

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    public TerrainMeshSettings terrainMeshSettings;
    
    private void OnValidate()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    [ContextMenu("Generate new Terrain")]
    public void GenerateNew()
    {
        GenerateTerrainMesh(terrainMeshSettings);
    }
    
    public void GenerateTerrainMesh(TerrainMeshSettings settings)
    {
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();
        List<int> newTriangles = new List<int>();
        
        for (int y = 0; y < settings.VerticesY - 1; y++)
        {
            for (int x = 0; x < settings.VerticesX - 1; x++)
            {
                var A = new Vector3(x * settings.QuadSize, 0, y * settings.QuadSize);
                var B = new Vector3(x * settings.QuadSize, 0, y+1 * settings.QuadSize);
                var C = new Vector3(x+1 * settings.QuadSize, 0, y * settings.QuadSize);
                var D = new Vector3(x+1 * settings.QuadSize, 0, y+1 * settings.QuadSize);

                if (!newVertices.Contains(A)) newVertices.Add(A);
                if (!newVertices.Contains(B)) newVertices.Add(B);
                if (!newVertices.Contains(C)) newVertices.Add(C);
                if (!newVertices.Contains(D)) newVertices.Add(D);

                newTriangles.Add(newVertices.IndexOf(A));
                newTriangles.Add(newVertices.IndexOf(B));
                newTriangles.Add(newVertices.IndexOf(D));
                
                newTriangles.Add(newVertices.IndexOf(A));
                newTriangles.Add(newVertices.IndexOf(D));
                newTriangles.Add(newVertices.IndexOf(C));
            }  
        }
        
#if UNITY_EDITOR
        if (ENABLE_CONSOLE_LOGS) Debug.Log($"Added vertices: {newVertices.Count}");
        if (ENABLE_CONSOLE_LOGS) Debug.Log($"Added triangles: {newTriangles.Count}");
#endif

        Mesh mesh = new Mesh();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUV.ToArray();
        //mesh.normals = newNormals.ToArray();
        mesh.RecalculateNormals();

        if(mesh == null) Debug.Log("mesh == null");
        
        _meshFilter.mesh = mesh;

        /*List<Triangle> triangles = new List<Triangle>();

        for (int x = 0; x < width * detail; x++)
        {
            for (int z = 0; z < height * detail; z++)
            {
                float y = transform.position.z;
                
                Vertex vertA = new Vertex(x * _tileSize, y, z * _tileSize); 
                vertA.UV = new Vector2(x*_tileSize, z * _tileSize);
                
                Vertex vertB = new Vertex(x * _tileSize + _tileSize, y, z * _tileSize); 
                vertB.UV = new Vector2(x * _tileSize + _tileSize, z * _tileSize);
                
                Vertex vertC = new Vertex(x * _tileSize, y, z * _tileSize + _tileSize); 
                vertC.UV = new Vector2(x * _tileSize, z * _tileSize + _tileSize);
                
                Vertex vertD = new Vertex(x * _tileSize + _tileSize, y, z * _tileSize + _tileSize);
                vertD.UV = new Vector2(x * _tileSize + _tileSize, z * _tileSize + _tileSize);

                Triangle triDBA = new Triangle(vertD, vertB, vertA);
                Triangle triACD = new Triangle(vertA, vertC, vertD);
                
                triangles.Add(triDBA);
                triangles.Add(triACD);
            }
        }
        
        if (enableEditorMessages) Debug.Log($"Created triangles: {triangles.Count}");
        
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();
        List<Vector2> newUv = new List<Vector2>();
        List<Vector3> newNormals = new List<Vector3>();
        
        foreach (Triangle triangle in triangles)
        {
            newVertices.Add(triangle.Vertex1.Position);
            newVertices.Add(triangle.Vertex2.Position);
            newVertices.Add(triangle.Vertex3.Position);
            
            newTriangles.Add(newTriangles.Count);
            newTriangles.Add(newTriangles.Count);
            newTriangles.Add(newTriangles.Count);
            
            newUv.Add(triangle.Vertex1.UV);
            newUv.Add(triangle.Vertex2.UV);
            newUv.Add(triangle.Vertex3.UV);
        }
        
        if (enableEditorMessages) Debug.Log($"Added vertices: {newVertices.Count}");
        if (enableEditorMessages) Debug.Log($"Added triangles: {newTriangles.Count}");
        
        _mesh.Clear();
        _mesh.vertices = newVertices.ToArray();
        _mesh.triangles = newTriangles.ToArray();
        _mesh.uv = newUv.ToArray();
        _mesh.normals = newNormals.ToArray();
        _mesh.RecalculateNormals();*/
    }
}
