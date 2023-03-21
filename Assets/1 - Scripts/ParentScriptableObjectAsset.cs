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
    }

    protected virtual void OnValidate()
    {
        OnValidateEvent?.Invoke();
    }

    protected virtual void Reset()
    {
        OnResetEvent?.Invoke();
    }
    
    protected virtual void OnDestroy()
    {
        OnDestroyEvent?.Invoke();
    }

}
