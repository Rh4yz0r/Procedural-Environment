using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentScriptableObjectAsset : ScriptableObject
{
    public event Action OnAwakeEvent;
    public event Action OnValidateEvent;
    public event Action OnResetEvent;
    public event Action OnDestroyEvent;
    
    protected virtual void Awake()
    {
        OnAwakeEvent?.Invoke();
        Debug.Log($"Awake '{this.name}'");
    }

    protected virtual void OnValidate()
    {
        OnValidateEvent?.Invoke();
        Debug.Log($"OnValidate '{this.name}'");
    }

    protected virtual void Reset()
    {
        OnResetEvent?.Invoke();
        Debug.Log($"Reset '{this.name}'");
    }
    
    protected virtual void OnDestroy()
    {
        OnDestroyEvent?.Invoke();
        Debug.Log($"OnDestroy '{this.name}'");
    }

}
