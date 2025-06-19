using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXSpawner : Spawner
{
    private static VFXSpawner instance;
    public static VFXSpawner Instance => instance;

    //Normal Attack VFX
    public static string swordSlash = "SwordSlash";
    public static string lanceSlash = "LanceSlash";

    //Ducknok's VFX
    public static string lightningtStrike = "LightningStrike";
    public static string lightningTrail = "LightningTrail";
    public static string ducknokClone = "DucknokClone";

    //May's VFX
    public static string groundSlash = "GroundSlash";
    public static string tonardo = "Tornado";

    //Enemy
    public static string fireBall = "FireBall";
    public static string waterBall = "WaterBall";
    public static string grass = "Grass";

    //Hit particle
    public static string blueHitParticle = "BlueHitParticle";
    public static string yellowHitParticle = "YellowHitParticle";
    public static string greenHitParticle = "GreenHitParticle";
    public static string fireHitParticle = "FireHitParticle";
    public static string windHitParticle = "WindHitParticle";
    public static string slashHitParticle = "SlashHitParticle";

    protected void Awake()
    {
        if (VFXSpawner.instance != null) Debug.LogError("Only 1 FXSpawner allow to exist");
        VFXSpawner.instance = this;
    }
}
