using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSkillManager : SkillManager
{
    [SerializeField] private Transform position;
    [SerializeField] private HeroStateMachine hsm;
    protected override void Awake()
    {
        LoadHeroSM();
    }

    private void LoadHeroSM()
    {
        if (this.hsm == null)
            this.hsm = this.transform.parent.GetComponent<HeroStateMachine>();
    }

    public void SwordSlashEffect()
    {
        SpawnEffect(VFXSpawner.swordSlash, this.position.position);
    }

    public void LanceSlashEffect()
    {
        SpawnEffect(VFXSpawner.lanceSlash, this.position.position);
    }

    public void LightnintStrikeEffect()
    {
        Vector2 enemyPosition = new Vector2(this.hsm.enemyToAttack.transform.position.x,
                                            this.hsm.enemyToAttack.transform.position.y + 3.25f);
        if (float.IsNaN(enemyPosition.x) || float.IsNaN(enemyPosition.y))
        {
            Debug.LogError("enemyToAttack ?ang c¨® position NaN!");
            return;
        }

        SpawnEffect(VFXSpawner.lightningtStrike, enemyPosition);
    }
    public void LightningTrail()
    {
        Transform body = hsm.transform.Find("Body");

        Transform effect = VFXSpawner.Instance.Spawn(VFXSpawner.lightningTrail, new Vector3(hsm.transform.Find("Body").position.x + 4f, hsm.transform.Find("Body").position.y, 0), Quaternion.identity);
        effect.gameObject.SetActive(true);
        if (effect != null)
        {
            effect.SetParent(body);
        }
    }
    public void GroundSlash()
    {
        Transform origin = this.hsm.transform.Find("Body").Find("GroundSlashPosition");
        if (origin == null) return;

        Vector3 startPos = origin.position;
        Vector3 targetPos = this.hsm.enemyToAttack.transform.Find("Body").position;

        Transform effect = VFXSpawner.Instance.Spawn(VFXSpawner.groundSlash, startPos, Quaternion.identity);
        if (effect != null)
        {
            effect.gameObject.SetActive(true);

            Vector3 dir = (targetPos - startPos).normalized;
            effect.right = dir;

            effect.DOMove(targetPos, 0.4f)
                  .SetEase(Ease.InQuad)
                  .OnComplete(() =>
                  {
    
                          VFXSpawner.Instance.Despawn(effect);
                  });
        }
    }
    public void Tornado()
    {
        Vector2 enemyPosition = new Vector2(this.hsm.enemyToAttack.transform.position.x,
                                           this.hsm.enemyToAttack.transform.position.y - 1f);
        if (float.IsNaN(enemyPosition.x) || float.IsNaN(enemyPosition.y))
        {
            return;
        }

        SpawnEffect(VFXSpawner.tonardo, enemyPosition);
    }

    protected override void SpawnEffect(string prefab, Vector3 position)
    {
        base.SpawnEffect(prefab, position);
    }
}
