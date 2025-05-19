using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXSpawner : Spawner
{
    private static VFXSpawner instance;
    public static VFXSpawner Instance => instance;

    public static string swordSlash = "SwordSlash";

    public static string lanceSlash = "LanceSlash";
    public static string lightningtStrike = "LightningStrike";
    public static string ducknokClone = "DucknokClone";

    protected void Awake()
    {
        if (VFXSpawner.instance != null) Debug.LogError("Only 1 FXSpawner allow to exist");
        VFXSpawner.instance = this;
    }
}
