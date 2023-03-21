using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Test Data", menuName = "ScriptableObjects/New Test Data", order = 1)]
public class TestParentSO : ParentScriptableObjectAsset
{
    public ChildAsset<Texture2D> heightMap;
    public ChildAsset<ScriptableObject> test;

    protected override void Awake()
    {
        base.Awake();
        
        heightMap = new ChildAsset<Texture2D>("Height Map", this);
        test = new ChildAsset<ScriptableObject>("Test", this);
    }
}
