using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManagement : Singleton<SceneManagement>
{

    public string sceneTransitionName { get; private set; }
    public void SetTransitionName(string SceneTransitionName)
    {
        this.sceneTransitionName = SceneTransitionName;
        //Debug.LogError(sceneTransitionName);
    }
}
