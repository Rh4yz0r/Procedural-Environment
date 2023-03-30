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
    private bool _initiated = false;
    
    [HideInInspector] public bool EnableConsoleLogs = false;

    [HideInInspector] public ParentScriptableObjectAsset Parent;
    
    [HideInInspector] public string AssetName;
    
    public T Asset{
        get
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(Parent));
            var foundAsset = assets.FirstOrDefault(a => a.name == AssetName) as T;
            if (!foundAsset && _initiated) throw new Exception($"{AssetName}: Asset not found!");
            return foundAsset;
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
                SetAsset(value, asset);
#if UNITY_EDITOR
                if(EnableConsoleLogs) Debug.Log($"Updated: {AssetName}");
#endif
            }
            else
            {
                SetAssetIfValueIsNull(asset);
#if UNITY_EDITOR
                if(EnableConsoleLogs) Debug.Log($"Updated to null: {AssetName}");
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
        Subscribe(parent);
        Refresh();
    }
    
    public ChildAsset(string assetName, ParentScriptableObjectAsset parent, bool enableConsoleLogs)
    {
        this.AssetName = assetName;
        this.Parent = parent;

        Init();
        Subscribe(parent);
        Refresh();
        
        EnableConsoleLogs = enableConsoleLogs;
    }

    protected virtual void SetAsset(T value, Object asset)
    {
        value.name = AssetName;
        EditorUtility.CopySerialized(value, asset);
    }
    
    protected virtual void SetAssetIfValueIsNull(Object asset)
    {
        EditorUtility.CopySerialized(new Texture2D(0, 0){name = AssetName}, asset);
    }

    private void Validate()
    {
        Init(); 
        Asset = _asset;
        Debug.Log($"Validating {AssetName}");
    }

    public void Subscribe(ParentScriptableObjectAsset parent)
    {
        parent.OnValidateEvent += Init;
        parent.OnValidateEvent += Validate;
        parent.OnResetEvent += Init;
        parent.OnDestroyEvent += Destroy;
    }
    
    private void Destroy()
    {
        EditorApplication.update -= DelayedInit;
        //Parent.OnAwakeEvent -= Init;
        //Parent.OnValidateEvent -= Validate;
        //Parent.OnResetEvent -= Init;
        //Parent.OnDestroyEvent -= Destroy;
    }

    protected virtual void Create()
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
            _initiated = true;
        }
        
        EditorUtility.SetDirty(Parent);
        AssetDatabase.SaveAssets();
    }
}
