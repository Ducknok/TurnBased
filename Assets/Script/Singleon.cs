using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Singleton<T> : DucMonobehaviour where T : Singleton<T>
{
    private static T instance;
    public static T Instance { get { return instance; } }

    protected override void Awake()
    {
        //Debug.Log($"[{typeof(T).Name}] Awake called");

        if (instance != null && this.gameObject != null)
        {
            //Debug.LogWarning($"[{typeof(T).Name}] Duplicate detected, destroying: {gameObject.name}");
            Destroy(this.gameObject);
            return;
        }

        instance = (T)this;
        DontDestroyOnLoad(this.gameObject);
        //Debug.Log($"[{typeof(T).Name}] Singleton initialized: {gameObject.name}");
    }
}
