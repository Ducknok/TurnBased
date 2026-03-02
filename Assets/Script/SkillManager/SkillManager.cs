using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillManager : DucMonobehaviour
{
    protected virtual void SpawnEffect(string prefab, Vector3 position)
    {
        Transform newVFX = VFXSpawner.Instance.Spawn(prefab, position, Quaternion.identity);
        if (newVFX == null) return;

        newVFX.gameObject.SetActive(true);
    }  
}
