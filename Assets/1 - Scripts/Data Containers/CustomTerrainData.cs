using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Terrain Data", menuName = "ScriptableObjects/New Terrain Data", order = 1)]
public class CustomTerrainData : ParentScriptableObjectAsset
{
    private readonly Vector2 _textureSize = new Vector2(512, 512); //Should become 513x513      EDIT: Don't...
    
    private const string HeightMapName = "Height Map";
    private const TextureFormat HeightMapFormat = TextureFormat.R16;
    public TextureChildAsset heightMap;
    [HideInInspector] public int heightSmoothIterations = 1;

    private const string SlopeMapName = "Slope Map";
    private const TextureFormat SlopeMapFormat = TextureFormat.R16;
    public TextureChildAsset slopeMap;
    
    private const string TexMapName = "Texture Map";
    private const TextureFormat TextureMapFormat = TextureFormat.RGBA32;
    public TextureChildAsset textureMap;

    protected override void Awake()
    {
        base.Awake();

        heightMap = new TextureChildAsset(HeightMapFormat, HeightMapName, this);
        slopeMap = new TextureChildAsset(SlopeMapFormat, SlopeMapName, this);
        textureMap = new TextureChildAsset(TextureMapFormat, TexMapName, this);

        ResetEvents();
    }

    protected override void ResetEvents()
    {
        base.ResetEvents();
        heightMap.Subscribe(this);
        slopeMap.Subscribe(this);
        textureMap.Subscribe(this);
    }

    public void GenerateNewHeightMap()
    {
        Texture2D newHeightMap = new Texture2D((int)_textureSize.x, (int)_textureSize.y, HeightMapFormat, -1, false) { name = HeightMapName };
        TextureMapGenerator.SetTextureToColor(newHeightMap, Color.black);

        heightMap.Asset = newHeightMap;
        heightMap.Refresh();
    }
    
    public void SmoothHeightMap(int iterations)
    {
        TextureMapData heightMapData = new TextureMapData(heightMap.Asset);
        heightMapData = TextureMapGenerator.SmoothMap(heightMapData.Texture2D, iterations);

        heightMap.Asset = heightMapData.Texture2D;
        heightMap.Refresh();
    }

    public void GenerateTextureMap()
    {
        TextureMapData newTextureMap = TextureMapGenerator.GenerateTextureMap(heightMap.Asset, slopeMap.Asset);
        
        textureMap.Asset = newTextureMap.Texture2D;
        textureMap.Refresh();
    }
    
    public void GenerateSlopeMap()
    {
        TextureMapData newSlopeMap = TextureMapGenerator.GenerateSlopeMap(heightMap.Asset);
        newSlopeMap = TextureMapGenerator.SmoothMap(newSlopeMap.Texture2D);
        
        slopeMap.Asset = newSlopeMap.Texture2D;
        slopeMap.Refresh();
    }
}

#region Editor
#if UNITY_EDITOR
[CustomEditor(typeof(CustomTerrainData))]
public class TerrainDataScriptableObjectEditor : Editor
{
    private CustomTerrainData _context;
    //private SerializedProperty _heightMap, _slopeMap;
    //private SerializedProperty _heightSmoothIterations;
    
    private void Awake()
    {
        _context = (CustomTerrainData)target;
        SerializeProperties();
    }

    private void SerializeProperties()
    {
        //_heightSmoothIterations = serializedObject.FindProperty("heightSmoothIterations");
        //_heightMap = serializedObject.FindProperty("HeightMap");
        //_slopeMap = serializedObject.FindProperty("SlopeMap");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    
        //EditorGUILayout.ObjectField(_heightMap, typeof(Texture2D));
        if (GUILayout.Button("Generate Height Map"))
        {
            _context.GenerateNewHeightMap();
        }
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Smooth Height Map"))
        {
            _context.SmoothHeightMap(_context.heightSmoothIterations);
        }
        _context.heightSmoothIterations = EditorGUILayout.IntField("Iterations", _context.heightSmoothIterations);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Generate Slope Map"))
        {
            _context.GenerateSlopeMap();
        }
        if (GUILayout.Button("Generate Texture Map"))
        {
            _context.GenerateTextureMap();
        }
        

        //if (GUI.changed) { EditorUtility.SetDirty(_context); }
    }
}
#endif
#endregion