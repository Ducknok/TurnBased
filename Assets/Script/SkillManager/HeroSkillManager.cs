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
            Debug.LogError("enemyToAttack ?ang có position NaN!");
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
        Transform origin = hsm.transform.Find("Body").Find("GroundSlashPosition");
        Vector3 startPos = origin.position;


        Vector3 targetPos = this.hsm.enemyToAttack.transform.Find("Body").position;

        Transform effect = VFXSpawner.Instance.Spawn(VFXSpawner.windBlade, startPos, Quaternion.identity);
        if (effect != null)
        {
            effect.gameObject.SetActive(true);
            Vector3 dir = (targetPos - startPos).normalized;
            effect.right = dir;
            Vector3 perpendicular = new Vector3(-dir.y, dir.x, 0);
            Vector3 controlPoint = (startPos + targetPos) / 2f + perpendicular * 1.5f; 

            Vector3[] path = new Vector3[] { controlPoint, targetPos };

            effect.DOPath(path, 0.4f, PathType.CatmullRom)
                          .SetEase(Ease.InQuad)
                          .OnComplete(() =>
                          {
                              VFXSpawner.Instance.Despawn(effect);
                          });
        }
    }
    public void Tornado()
    {
        Transform origin = hsm.transform.Find("Body").Find("GroundSlashPosition");
        if (origin == null) return;

        Vector3 startPos = origin.position;

        Transform enemyBody = hsm.enemyToAttack.transform.Find("Body");
        Vector3 targetPos = (enemyBody != null) ? enemyBody.position : hsm.enemyToAttack.transform.position;

        targetPos.y -= 0.6f;

        Transform effect = VFXSpawner.Instance.Spawn(VFXSpawner.tonardo, startPos, Quaternion.identity);

        if (effect != null)
        {
            effect.gameObject.SetActive(true);

            effect.localScale = Vector3.one * 0.1f;
            effect.rotation = Quaternion.identity;

            Sequence tornadoSeq = DOTween.Sequence();

            tornadoSeq.Join(effect.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0.7f).SetEase(Ease.InQuad));

            Vector3 dir = (targetPos - startPos).normalized;
            Vector3 sideDir = new Vector3(-dir.y, dir.x, 0);
            Vector3 p1 = startPos + dir * 0.33f + sideDir * 0.7f;
            Vector3 p2 = startPos + dir * 0.66f - sideDir * 0.7f;
            Vector3[] path = new Vector3[] { p1, p2, targetPos };


            tornadoSeq.Join(effect.DOPath(path, 0.7f, PathType.CatmullRom).SetEase(Ease.OutQuad));

            for (int i = 0; i < 3; i++)
            {
                tornadoSeq.AppendCallback(() => {

                    if (hsm.enemyToAttack != null)
                        hsm.enemyToAttack.transform.DOShakePosition(0.2f, 0.15f, 10);

                    effect.DOShakePosition(0.2f, 0.1f, 5);
                });

                tornadoSeq.AppendInterval(1.0f); 
            }

            tornadoSeq.Append(effect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
            tornadoSeq.OnComplete(() => {
                VFXSpawner.Instance.Despawn(effect);
            });
        }
    }
    protected override void SpawnEffect(string prefab, Vector3 position)
    {
        base.SpawnEffect(prefab, position);
    }
}
