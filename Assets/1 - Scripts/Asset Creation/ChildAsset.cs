using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class ChildAsset<T> where T : Object
{
    [HideInInspector] public bool EnableConsoleLogs = false;

    [HideInInspector] public ParentScriptableObjectAsset Parent;
    
    [HideInInspector] public string AssetName;
    
    public T Asset{
        get
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(Parent));
            return assets.FirstOrDefault(a => a.name == AssetName) as T;
        }
        set
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(Parent));
            var asset = assets.FirstOrDefault(a => a.name == AssetName);

            if (!asset)
            {
                Debug.LogError($"Cannot find '{AssetName}' asset!");
                return;
            }
            
            if (value != null)
            {
                value.name = AssetName;
                EditorUtility.CopySerialized(value, asset);
#if UNITY_EDITOR
                if(EnableConsoleLogs) Debug.Log("Updated");
#endif
            }
            else
            {
                SetAssetIfValueIsNull(asset);
#if UNITY_EDITOR
                if(EnableConsoleLogs) Debug.Log("Updated to null");
#endif
            }
        }
    }

    [SerializeField] private T _asset;

    public ChildAsset(string assetName, ParentScriptableObjectAsset parent)
    {
        this.AssetName = assetName;
        this.Parent = parent;

        Init();
        Subscribe();
    }
    
    public ChildAsset(string assetName, ParentScriptableObjectAsset parent, bool enableConsoleLogs)
    {
        this.AssetName = assetName;
        this.Parent = parent;

        Init();
        Subscribe();
        

        EnableConsoleLogs = enableConsoleLogs;
    }

    protected void SetAssetIfValueIsNull(Object asset)
    {
        EditorUtility.CopySerialized(new Texture2D(0, 0){name = AssetName}, asset);
    }

    private void Validate()
    {
        Init(); 
        Asset = _asset;
        Debug.Log($"Validating {AssetName}");
    }

    private void Subscribe()
    {
        this.Parent.OnValidateEvent += Init;
        this.Parent.OnValidateEvent += Validate;
        this.Parent.OnResetEvent += Init;
        this.Parent.OnDestroyEvent += Destroy;
    }
    
    private void Destroy()
    {
        EditorApplication.update -= DelayedInit;
        //Parent.OnAwakeEvent -= Init;
        //Parent.OnValidateEvent -= Validate;
        //Parent.OnResetEvent -= Init;
        //Parent.OnDestroyEvent -= Destroy;
    }

    protected void Create()
    {
        var asset = ObjectFactory.CreateInstance(typeof(T));
        asset.name = AssetName;
        AssetDatabase.AddObjectToAsset(asset, Parent);
    }

    public void Refresh()
    {
        _asset = Asset;
    }

    private void Init()
    {
        if (Asset) { return; }
        
        if (AssetDatabase.Contains(Parent))
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
        if (!AssetDatabase.Contains(Parent)) { return; }
        
        EditorApplication.update -= DelayedInit;

        if (!Asset)
        {
            Create();
            _asset = Asset;
        }
        
        EditorUtility.SetDirty(Parent);
        AssetDatabase.SaveAssets();
    }
}
