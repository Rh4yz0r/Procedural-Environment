using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
    public int QuadSize = 1;
    //[Range(1, 10)] public int TerrainDetail = 1;

    public int WidthTotal => TerrainSize.x;
    public int HeightTotal => TerrainSize.y;

    public int QuadsX => TerrainSize.x / QuadSize;
    public int QuadsY => TerrainSize.y / QuadSize;
    
    /*public int QuadsX => TerrainSize.x * TerrainDetail;
    public int QuadsY => TerrainSize.y * TerrainDetail;*/
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
    
    public void GenerateTerrainMeshWithHeight(TerrainMeshSettings settings, TextureMapData textureMapData)
    {
        //Undo.RegisterCompleteObjectUndo(this.gameObject, "Load new terrain mesh");

        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();
        List<int> newTriangles = new List<int>();
        
        int total = settings.VerticesX * settings.VerticesY;
        float progress = 0;
        
        for (int y = 0; y < settings.VerticesY - 1; y++)
        {
            for (int x = 0; x < settings.VerticesX - 1; x++)
            {
                EditorUtility.DisplayProgressBar("Creating new terrain mesh", $"Creating tile x:{x},y:{y}", progress);

                var A = new Vector3(x * settings.QuadSize, textureMapData.Pixels[x, y].r*textureMapData.Height, y * settings.QuadSize);
                var B = new Vector3(x * settings.QuadSize, textureMapData.Pixels[x, y+1].r*textureMapData.Height, (y+1) * settings.QuadSize);
                var C = new Vector3((x+1) * settings.QuadSize, textureMapData.Pixels[x+1, y].r*textureMapData.Height, y * settings.QuadSize);
                var D = new Vector3((x+1) * settings.QuadSize, textureMapData.Pixels[x+1, y+1].r*textureMapData.Height, (y+1) * settings.QuadSize);

                if (!newVertices.Contains(A)) {newVertices.Add(A); newUV.Add(new Vector2(A.x/textureMapData.Width, A.z/textureMapData.Height));}
                if (!newVertices.Contains(B)) {newVertices.Add(B); newUV.Add(new Vector2(B.x/textureMapData.Width, B.z/textureMapData.Height));}
                if (!newVertices.Contains(C)) {newVertices.Add(C); newUV.Add(new Vector2(C.x/textureMapData.Width, C.z/textureMapData.Height));}
                if (!newVertices.Contains(D)) {newVertices.Add(D); newUV.Add(new Vector2(D.x/textureMapData.Width, D.z/textureMapData.Height));}

                newTriangles.Add(newVertices.IndexOf(A));
                newTriangles.Add(newVertices.IndexOf(B));
                newTriangles.Add(newVertices.IndexOf(D));
                
                newTriangles.Add(newVertices.IndexOf(A));
                newTriangles.Add(newVertices.IndexOf(D));
                newTriangles.Add(newVertices.IndexOf(C));

                progress += 1f / total;
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

        _meshFilter.mesh = mesh;

        EditorUtility.ClearProgressBar();
    }
    
    public void GenerateTerrainMesh(TerrainMeshSettings settings)
    {
        //Undo.RegisterCompleteObjectUndo(this.gameObject, "Load new terrain mesh");
        
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();
        List<int> newTriangles = new List<int>();
        
        int total = settings.VerticesX * settings.VerticesY;
        float progress = 0;
        
        for (int y = 0; y < settings.VerticesY - 1; y++)
        {
            for (int x = 0; x < settings.VerticesX - 1; x++)
            {
                EditorUtility.DisplayProgressBar("Creating new terrain mesh", $"Creating tile x:{x},y:{y}", progress);
                
                var A = new Vector3(x * settings.QuadSize, 0, y * settings.QuadSize);
                var B = new Vector3(x * settings.QuadSize, 0, (y+1) * settings.QuadSize);
                var C = new Vector3((x+1) * settings.QuadSize, 0, y * settings.QuadSize);
                var D = new Vector3((x+1) * settings.QuadSize, 0, (y+1) * settings.QuadSize);

                if (!newVertices.Contains(A)) {newVertices.Add(A); newUV.Add(new Vector2(A.x/settings.TerrainSize.x, A.z/settings.TerrainSize.y));}
                if (!newVertices.Contains(B)) {newVertices.Add(B); newUV.Add(new Vector2(B.x/settings.TerrainSize.x, B.z/settings.TerrainSize.y));}
                if (!newVertices.Contains(C)) {newVertices.Add(C); newUV.Add(new Vector2(C.x/settings.TerrainSize.x, C.z/settings.TerrainSize.y));}
                if (!newVertices.Contains(D)) {newVertices.Add(D); newUV.Add(new Vector2(D.x/settings.TerrainSize.x, D.z/settings.TerrainSize.y));}

                newTriangles.Add(newVertices.IndexOf(A));
                newTriangles.Add(newVertices.IndexOf(B));
                newTriangles.Add(newVertices.IndexOf(D));
                
                newTriangles.Add(newVertices.IndexOf(A));
                newTriangles.Add(newVertices.IndexOf(D));
                newTriangles.Add(newVertices.IndexOf(C));

                progress += 1f / total;
            }  
        }
        
#if UNITY_EDITOR
        if (ENABLE_CONSOLE_LOGS) Debug.Log($"Added vertices: {newVertices.Count}");
        if (ENABLE_CONSOLE_LOGS) Debug.Log($"Added triangles: {newTriangles.Count}");
#endif

        Mesh mesh = new Mesh();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        //mesh.uv = newUV.ToArray();
        //mesh.normals = newNormals.ToArray();
        mesh.RecalculateNormals();

        _meshFilter.mesh = mesh;

        EditorUtility.ClearProgressBar();
    }
}
