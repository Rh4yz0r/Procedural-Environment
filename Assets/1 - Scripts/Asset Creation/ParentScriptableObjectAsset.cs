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
        //ResetEvents();
        OnAwakeEvent?.Invoke();
        Debug.Log($"Awake '{this.name}'");
    }

    protected virtual void OnValidate()
    {
        ResetEvents();
        OnValidateEvent?.Invoke();
        Debug.Log($"OnValidate '{this.name}'");
    }

    protected virtual void Reset()
    {
        //ResetEvents();
        OnResetEvent?.Invoke();
        Debug.Log($"Reset '{this.name}'");
    }
    
    protected virtual void OnDestroy()
    {
        ResetEvents();
        OnDestroyEvent?.Invoke();
        Debug.Log($"OnDestroy '{this.name}'");
    }

    protected virtual void ResetEvents()
    {
        OnAwakeEvent = null;
        OnValidateEvent = null;
        OnResetEvent = null;
        OnDestroyEvent = null;
    }
}
