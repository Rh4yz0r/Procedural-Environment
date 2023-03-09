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
                SetTextureToColor(blankTexture, Color.black);
                
                EditorUtility.CopySerialized(blankTexture, heightMapAsset);
#if UNITY_EDITOR
                if(ENABLE_CONSOLE_LOGS) Debug.Log("Updated to null (blank)");
#endif
            }
        }
    }

    public void GenerateNewHeightMap()
    {
        Texture2D newHeightMap = new Texture2D((int)_textureSize.x, (int)_textureSize.y, heightMapFormat, -1, false) { name = heightMapName };
        SetTextureToColor(newHeightMap, Color.black);

        HeightMap = newHeightMap;
        heightMap = HeightMap;
    }
    
    private void SetTextureToColor(Texture2D texture2D, Color color)
    {
        Color fillColor = color;
        var fillColorArray = new Color[texture2D.width * texture2D.height];

        for (var i = 0; i < fillColorArray.Length; ++i)
        {
            fillColorArray[i] = fillColor;
        }

        texture2D.SetPixels(fillColorArray);

        texture2D.Apply();
    }
    
#if UNITY_EDITOR
    private void Awake()
    {
        Init();
    }

    private void OnValidate()
    {
        Init();
        HeightMap = heightMap;
    }

    private void Reset()
    {
        Init();
    }

    private void OnDestroy()
    {
        EditorApplication.update -= DelayedInit;
    }

    #region Creating Child Assets
    private void Init()
    {
        if (HeightMap) { return; }
        
        if (AssetDatabase.Contains(this))
        {
            DelayedInit();
        }
        else
        {
            EditorApplication.update -= DelayedInit;
            EditorApplication.update += DelayedInit;
        }
    }

    private void DelayedInit()
    {
        if (!AssetDatabase.Contains(this)) { return; }
        
        EditorApplication.update -= DelayedInit;

        if (!HeightMap)
        {
            var heightMapAsset = new Texture2D((int)_textureSize.x, (int)_textureSize.y, heightMapFormat, -1, false){ name = heightMapName };
            AssetDatabase.AddObjectToAsset(heightMapAsset, this);
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

        //if (GUI.changed) { EditorUtility.SetDirty(_context); }
    }
}
#endif
#endregion