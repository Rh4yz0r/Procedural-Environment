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
    private readonly Vector2 _textureSize = new Vector2(512, 512); //Should become 513x513
    
    private readonly string heightMapName = "Slope Map";
    private readonly TextureFormat heightMapFormat = TextureFormat.R16;
    public ChildAsset<Texture2D> heightMap;
    
    private readonly string slopeMapName = "Height Map";
    private readonly TextureFormat slopeMapFormat = TextureFormat.R16;
    public ChildAsset<Texture2D> slopeMap;
    
    private readonly string texMapName = "Texture Map";
    private readonly TextureFormat textureMapFormat = TextureFormat.R16;
    public ChildAsset<Texture2D> textureMap;

    protected override void Awake()
    {
        base.Awake();

        heightMap = new ChildAsset<Texture2D>("Height Map", this);
        slopeMap = new ChildAsset<Texture2D>("Slope Map", this);
        textureMap = new ChildAsset<Texture2D>("Texture Map", this);
    }

    public void GenerateNewHeightMap()
    {
        Texture2D newHeightMap = new Texture2D((int)_textureSize.x, (int)_textureSize.y, heightMapFormat, -1, false) { name = heightMapName };
        TextureMapGenerator.SetTextureToColor(newHeightMap, Color.black);

        heightMap.Asset = newHeightMap;
        heightMap.Refresh();
    }

    public void GenerateSlopeMap()
    {
        TextureMapData newSlopeMap = TextureMapGenerator.GenerateSlopeMap(heightMap.Asset);

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
    
    private void Awake()
    {
        _context = (CustomTerrainData)target;
        SerializeProperties();
    }

    private void SerializeProperties()
    {
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
        if (GUILayout.Button("Generate Slope Map"))
        {
            _context.GenerateSlopeMap();
        }

        //if (GUI.changed) { EditorUtility.SetDirty(_context); }
    }
}
#endif
#endregion