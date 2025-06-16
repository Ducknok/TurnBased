using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabDespawn : DespawnByTime
{
    public override void DespawnObject()
    {
        VFXSpawner.Instance.Despawn(this.transform);
    }
}