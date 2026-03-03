using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DucMonobehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    protected virtual void Start(){}
    // Update is called once per frame
    protected virtual void Update(){}
    protected virtual void Awake(){}
    protected virtual void FixedUpdate(){}
    protected virtual void OnEnable() 
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    protected virtual void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }
    public virtual void CheckState() { }

}
