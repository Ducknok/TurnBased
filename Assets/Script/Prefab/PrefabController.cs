using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabController : MonoBehaviour
{


    [SerializeField] protected PrefabDespawn bulletDespawn;
    public PrefabDespawn BulletDespawn { get => bulletDespawn; }

    protected void LoadComponents()
    {
        this.LoadPrefabDespawn();
    }
    protected virtual void LoadPrefabDespawn()
    {
        if (this.bulletDespawn != null) return;
        this.bulletDespawn = transform.GetComponentInChildren<PrefabDespawn>();
    }

}