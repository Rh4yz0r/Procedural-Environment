using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class TextureChildAsset : ChildAsset<Texture2D>
{
    private TextureFormat _format;
    
    public TextureChildAsset(TextureFormat format, string assetName, ParentScriptableObjectAsset parent) : base(assetName, parent)
    {
        _format = format;
    }

    public TextureChildAsset(TextureFormat format, string assetName, ParentScriptableObjectAsset parent, bool enableConsoleLogs) : base(assetName, parent, enableConsoleLogs)
    {
        _format = format;
    }

    protected override void Create()
    {
        var asset = new Texture2D(0, 0, _format, -1, false);
        asset.name = AssetName;
        AssetDatabase.AddObjectToAsset(asset, Parent);
    }

    protected override void SetAsset(Texture2D value, Object asset)
    {
        value.name = AssetName;

        var oldTexture = asset as Texture2D;
        var newTexture = value;

        if (!oldTexture) throw new Exception("Asset is Null!");

        if (newTexture.format != oldTexture.format)
        {
            var convertedNew = new Texture2D(newTexture.width, newTexture.height, oldTexture.format, -1, false);
            convertedNew.name = AssetName;
            
            convertedNew.SetPixels(newTexture.GetPixels());
            convertedNew.Apply();
            EditorUtility.CopySerialized(convertedNew, asset);
        }
        else
        {
            EditorUtility.CopySerialized(value, asset);
        }
    }
    
    protected override void SetAssetIfValueIsNull(Object asset)
    {
        EditorUtility.CopySerialized(new Texture2D(0, 0){name = AssetName}, asset);
    }
}
