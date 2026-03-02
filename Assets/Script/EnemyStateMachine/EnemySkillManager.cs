using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySkillManager : SkillManager
{
    
    [SerializeField] private EnemyStateMachine esm;
    [SerializeField] private Transform position;

    protected override void Awake()
    {
        LoadHeroSM();
    }

    private void LoadHeroSM()
    {
        if (this.esm == null)
            this.esm = this.transform.parent.GetComponent<EnemyStateMachine>();
    }

    public void FireBall() 
    {
        Transform origin = this.esm.transform.Find("Body").Find("SkillPosition");
        if (origin == null) return;

        Vector3 startPos = origin.position;
        Vector3 targetPos = this.esm.playerToAttack.transform.Find("Body").position;

        Transform effect = VFXSpawner.Instance.Spawn(VFXSpawner.fireBall, startPos, Quaternion.identity);
        if (effect != null)
        {
            effect.gameObject.SetActive(true);

            Vector3 dir = (targetPos - startPos).normalized;
            effect.right = dir;

            // Di chuy?n t?i enemy
            effect.DOMove(targetPos, 0.4f)
                  .SetEase(Ease.InQuad);
        }
    }
    public void WaterBall()
    {
        Transform origin = this.esm.transform.Find("Body").Find("SkillPosition");
        if (origin == null) return;

        Vector3 startPos = origin.position;
        Vector3 targetPos = this.esm.playerToAttack.transform.Find("Body").position;

        Transform effect = VFXSpawner.Instance.Spawn(VFXSpawner.waterBall, startPos, Quaternion.identity);
        if (effect != null)
        {
            effect.gameObject.SetActive(true);

            // H??ng effect v? ph¨ªa enemy
            Vector3 dir = (targetPos - startPos).normalized;
            effect.right = dir; // Gi? ??nh VFX xoay theo tr?c right

            // Di chuy?n t?i enemy
            effect.DOMove(targetPos, 0.4f)
                  .SetEase(Ease.InQuad);
        }
    }
    public void Grass()
    {
        Transform origin = this.esm.transform.Find("Body").Find("SkillPosition");
        if (origin == null) return;

        Vector3 startPos = origin.position;
        Vector3 targetPos = this.esm.playerToAttack.transform.Find("Body").position;

        Transform effect = VFXSpawner.Instance.Spawn(VFXSpawner.grass, startPos, Quaternion.identity);
        if (effect != null)
        {
            effect.gameObject.SetActive(true);

            // H??ng effect v? ph¨ªa enemy
            Vector3 dir = (targetPos - startPos).normalized;
            effect.right = dir; // Gi? ??nh VFX xoay theo tr?c right

            // Di chuy?n t?i enemy
            effect.DOMove(targetPos, 0.4f)
                  .SetEase(Ease.InQuad);
        }
    }
    protected override void SpawnEffect(string prefab, Vector3 position)
    {
        base.SpawnEffect(prefab, position);
    }
}
