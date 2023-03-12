using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Terrain Data", menuName = "ScriptableObjects/New Terrain Data", order = 1)]
public class TerrainData : ScriptableObject
{
#if UNITY_EDITOR
    private const bool ENABLE_CONSOLE_LOGS = false;
#endif
    
    private readonly Vector2 _textureSize = new Vector2(512, 512);
    private readonly string heightMapName = "Height Map";
    private readonly TextureFormat heightMapFormat = TextureFormat.R16;

    [SerializeField] private Texture2D heightMap;
    public Texture2D HeightMap
    {
        get
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));
            return assets.FirstOrDefault(a => a.name == heightMapName) as Texture2D;
        }
        set
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));
            var heightMapAsset = assets.FirstOrDefault(a => a.name == heightMapName);

            if (!heightMapAsset)
            {
                Debug.LogError("Cannot find height map asset!");
                return;
            }
            
            if (value != null)
            {
                value.name = heightMapName;
                EditorUtility.CopySerialized(value, heightMapAsset);
#if UNITY_EDITOR
                if(ENABLE_CONSOLE_LOGS) Debug.Log("Updated");
#endif
            }
            else
            {
                Texture2D blankTexture = new Texture2D((int)_textureSize.x, (int)_textureSize.y, heightMapFormat, -1, false) { name = heightMapName };
                TextureMapGenerator.SetTextureToColor(blankTexture, Color.black);
                
                EditorUtility.CopySerialized(blankTexture, heightMapAsset);
#if UNITY_EDITOR
                if(ENABLE_CONSOLE_LOGS) Debug.Log("Updated to null (blank)");
#endif
            }
        }
    }
    
    private readonly string slopeMapName = "Slope Map";
    private readonly TextureFormat slopeMapFormat = TextureFormat.R16;

    [SerializeField] private Texture2D slopeMap;
    public Texture2D SlopeMap
    {
        get
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));
            return assets.FirstOrDefault(a => a.name == slopeMapName) as Texture2D;
        }
        set
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));
            var slopeMapAsset = assets.FirstOrDefault(a => a.name == slopeMapName);

            if (!slopeMapAsset)
            {
                Debug.LogError("Cannot find slope map asset!");
                return;
            }
            
            if (value != null)
            {
                value.name = slopeMapName;
                EditorUtility.CopySerialized(value, slopeMapAsset);
#if UNITY_EDITOR
                if(ENABLE_CONSOLE_LOGS) Debug.Log("Updated");
#endif
            }
            else
            {
                Texture2D blankTexture = new Texture2D((int)_textureSize.x, (int)_textureSize.y, slopeMapFormat, -1, false) { name = slopeMapName };
                TextureMapGenerator.SetTextureToColor(blankTexture, Color.black);
                
                EditorUtility.CopySerialized(blankTexture, slopeMapAsset);
#if UNITY_EDITOR
                if(ENABLE_CONSOLE_LOGS) Debug.Log("Updated to null (blank)");
#endif
            }
        }
    }

    public void GenerateNewHeightMap()
    {
        Texture2D newHeightMap = new Texture2D((int)_textureSize.x, (int)_textureSize.y, heightMapFormat, -1, false) { name = heightMapName };
        TextureMapGenerator.SetTextureToColor(newHeightMap, Color.black);

        HeightMap = newHeightMap;
        heightMap = HeightMap;
    }

    public void GenerateSlopeMap()
    {
        TextureMapData newSlopeMap = TextureMapGenerator.GenerateSlopeMap(heightMap);

        foreach (var pixel in newSlopeMap.Pixels)
        {
            //Debug.Log(pixel);
        }
        
        SlopeMap = newSlopeMap.Texture2D;
        slopeMap = SlopeMap;
    }

#if UNITY_EDITOR
    private void Awake()
    {
        InitHeightMap();
        InitSlopeMap();
    }

    private void OnValidate()
    {
        InitHeightMap();
        InitSlopeMap();
        HeightMap = heightMap;
        SlopeMap = slopeMap;
    }

    private void Reset()
    {
        InitHeightMap();
        InitSlopeMap();
    }

    private void OnDestroy()
    {
        EditorApplication.update -= DelayedInitHeightMap;
        EditorApplication.update -= DelayedInitSlopeMap;
    }

    #region Creating Child Assets
    private void InitHeightMap()
    {
        if (HeightMap) { return; }
        
        if (AssetDatabase.Contains(this))
        {
            DelayedInitHeightMap();
        }
        else
        {
            EditorApplication.update -= DelayedInitHeightMap;
            EditorApplication.update += DelayedInitHeightMap;
        }
    }

    private void DelayedInitHeightMap()
    {
        if (!AssetDatabase.Contains(this)) { return; }
        
        EditorApplication.update -= DelayedInitHeightMap;

        if (!HeightMap)
        {
            var heightMapAsset = new Texture2D((int)_textureSize.x, (int)_textureSize.y, heightMapFormat, -1, false){ name = heightMapName };
            AssetDatabase.AddObjectToAsset(heightMapAsset, this);
            heightMap = HeightMap;
        }
        
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
    
    private void InitSlopeMap()
    {
        if (SlopeMap) { return; }
        
        if (AssetDatabase.Contains(this))
        {
            DelayedInitSlopeMap();
        }
        else
        {
            EditorApplication.update -= DelayedInitSlopeMap;
            EditorApplication.update += DelayedInitSlopeMap;
        }
    }

    private void DelayedInitSlopeMap()
    {
        if (!AssetDatabase.Contains(this)) { return; }
        
        EditorApplication.update -= DelayedInitSlopeMap;

        if (!SlopeMap)
        {
            var slopeMapAsset = new Texture2D((int)_textureSize.x, (int)_textureSize.y, slopeMapFormat, -1, false){ name = slopeMapName };
            AssetDatabase.AddObjectToAsset(slopeMapAsset, this);
            slopeMap = SlopeMap;
        }
        
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
    #endregion
#endif
}

#region Editor
#if UNITY_EDITOR
[CustomEditor(typeof(TerrainData))]
public class TerrainDataScriptableObjectEditor : Editor
{
    private TerrainData _context;
    private SerializedProperty _heightMap, _slopeMap;
    
    private void Awake()
    {
        _context = (TerrainData)target;
        SerializeProperties();
    }

    private void SerializeProperties()
    {
        _heightMap = serializedObject.FindProperty("HeightMap");
        _slopeMap = serializedObject.FindProperty("SlopeMap");
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