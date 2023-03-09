using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[CreateAssetMenu(fileName = "New Terrain Data", menuName = "ScriptableObjects/New Terrain Data", order = 1)]
public class TerrainData : ScriptableObject
{
    private readonly Vector2 _textureSize = new Vector2(512, 512);
    private readonly string heightMapName = "Height Map";
    private readonly TextureFormat heightMapFormat = TextureFormat.R16;

    public Texture2D HeightMap
    {
        get => heightMapAsset;
        set
        {
            if (value != null)
            {
                EditorUtility.CopySerialized(value, heightMapAsset);
                Debug.Log("Updated");
            }
            else
            {
                Texture2D blankTexture = new Texture2D((int)_textureSize.x, (int)_textureSize.y, heightMapFormat, -1, false) { name = heightMapName };
                SetTextureToColor(blankTexture, Color.black);
                
                EditorUtility.CopySerialized(blankTexture, heightMapAsset);
                Debug.Log("Updated to null (blank)");
            }
            
            AssetDatabase.Refresh();
        }
    }

    [SerializeField] private Texture2D heightMapAsset;
    

    public void GenerateNewHeightMap()
    {
        Texture2D newHeightMap = new Texture2D((int)_textureSize.x, (int)_textureSize.y, heightMapFormat, -1, false) { name = heightMapName };

        SetTextureToColor(newHeightMap, Color.black);

        HeightMap = newHeightMap;
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

    #region Creating Sub Assets (Uses Awake(), OnValidate(), Reset() and Destroy())
#if UNITY_EDITOR
    private void Awake()
    {
        Init();
    }

    private void OnValidate()
    {
        Init();
        
        Debug.Log("VALIDATING");
        HeightMap = heightMapAsset;
    }

    private void Reset()
    {
        Init();
    }

    private void OnDestroy()
    {
        EditorApplication.update -= DelayedInit;
    }

    private void Init()
    {
        if (heightMapAsset) { return; }
        
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
        
        var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));
        
        heightMapAsset = assets.FirstOrDefault(a => a.name == heightMapName) as Texture2D;
        if (!heightMapAsset)
        {
            heightMapAsset = new Texture2D((int)_textureSize.x, (int)_textureSize.y, heightMapFormat, -1, false){ name = heightMapName };
            AssetDatabase.AddObjectToAsset(heightMapAsset, this);
        }
        
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
#endif
    #endregion
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

        //if (GUI.changed) { EditorUtility.SetDirty(_context); }
    }
}
#endif
#endregion










/*private readonly Vector2 _textureSize = new Vector2(512, 512);

[SerializeField] private Texture2D _heightMap;



/*public Texture2D HeightMap
{
get => _heightMap;
set
{
    if (value == null)
    {
        Texture2D blankTexture = new Texture2D((int)_textureSize.x, (int)_textureSize.y, TextureFormat.R16, -1, false) 
            { name = "Height Map" };
        SetTextureToColor(blankTexture, Color.black);
        
        EditorUtility.CopySerialized(blankTexture, _heightMap);
        Debug.Log("Updated to null (blank)");
    }
    else 
    {
        EditorUtility.CopySerialized(value, _heightMap);
        Debug.Log("Updated");
    }
    
    AssetDatabase.SaveAssets();
}
}#1#

/*private void OnValidate()
{
HeightMap = _heightMap;
}#1#

private Texture2D CreateHeightMapTexture() 
{
Texture2D newHeightMap = new Texture2D((int)_textureSize.x, (int)_textureSize.y, TextureFormat.R16, -1, false) 
    { name = "Height Map" };

SetTextureToColor(newHeightMap, Color.black);

return newHeightMap;
}

/*public void GenerateRandomHeightMap()
{
HeightMap = CreateHeightMapTexture();

EditorUtility.SetDirty(this);
}#1#

private void SetTextureToColor(Texture2D texture2D, Color color)
{
Color fillColor = color;
var fillColorArray = new Color[texture2D.width * texture2D.height];

for(var i = 0; i < fillColorArray.Length; ++i)
{
    fillColorArray[i] = fillColor;
}

texture2D.SetPixels(fillColorArray);

texture2D.Apply();
}
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
    //_context.GenerateRandomHeightMap();
    //_context.GenerateSlopeMapFromHeightMap();
}

//EditorGUILayout.Space();

//EditorGUILayout.ObjectField(_slopeMap, typeof(Texture2D));
/*if (GUILayout.Button("Generate Slope Map"))
{
    _context.GenerateSlopeMapFromHeightMap();
}#1#

/*if (GUI.changed)
{
    EditorUtility.SetDirty(_context);
}#1#
}
}
#endif
#endregion
*/
