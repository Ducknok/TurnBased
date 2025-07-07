using System.Collections;
using UnityEngine;

public class CoroutineHelper : DucMonobehaviour
{
    private static CoroutineHelper instance;

    public static Coroutine Start(IEnumerator coroutine)
    {
        if (instance == null)
        {
            GameObject go = new GameObject("CoroutineHelper");
            instance = go.AddComponent<CoroutineHelper>();
            GameObject.DontDestroyOnLoad(go);
        }

        return instance.StartCoroutine(coroutine);
    }
}
