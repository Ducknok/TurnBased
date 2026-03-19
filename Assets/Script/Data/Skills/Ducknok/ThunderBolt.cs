using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderOrbsStrike : SkillBehaviour
{
    [Header("Thunder Orbs Settings")]
    public float orbRadius = 2.0f;       
    public float orbTravelTime = 0.15f;   

    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {
        HeroStateMachine hsm = attacker.GetComponent<HeroStateMachine>();
        Transform body = hsm.transform.Find("Body");
        Transform enemy = target.transform.Find("Body");
        Animator anim = body.GetComponent<Animator>();
        SpriteRenderer sr = body.GetComponent<SpriteRenderer>();

        if (enemy == null)
        {
            Debug.LogError("Không có enemy để tấn công.");
            yield break;
        }

        Vector3 startPos = body.position;
        Vector3 enemyPos = enemy.position;


        if (anim != null) anim.Play(hsm.currentAttack.skillData.attackName); // Animation giơ vũ khí

        Vector3[] positions = new Vector3[3];
        for (int i = 0; i < 3; i++)
        {
            float angle = (i * 120f + 90f) * Mathf.Deg2Rad;
            positions[i] = enemyPos + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * orbRadius;
        }

        List<Transform> orbs = new List<Transform>();

        for (int i = 0; i < positions.Length; i++)
        {
            Transform orb = VFXSpawner.Instance.Spawn(VFXSpawner.thunderBolt, positions[i], Quaternion.identity);
            if (orb != null)
            {
                orb.gameObject.SetActive(true);
                // Hiệu ứng "pop" lên từ từ (Zoom in)
                orb.localScale = Vector3.zero;
                orb.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
                orbs.Add(orb);
            }
            yield return new WaitForSeconds(0.1f); // Xuất hiện tạch-tạch-tạch
        }

        yield return new WaitForSeconds(0.2f); // Chờ 1 nhịp


        foreach (Transform orb in orbs)
        {
            if (orb != null)
            {
                orb.DOMove(enemyPos, orbTravelTime)
                     .SetEase(Ease.InExpo) // Bay với gia tốc nhanh dần
                     .OnComplete(() =>
                     {
                         if (target != null && target.activeInHierarchy)
                         {
                             // Rung quái
                             target.transform.DOShakePosition(0.15f, 0.15f, 15);

                             this.ApplySingleTargetDamage(attacker, target);
                             Transform hit = VFXSpawner.Instance.Spawn(VFXSpawner.thunderBolt, enemyPos, Quaternion.identity);
                             if (hit != null)
                                 {
                                     hit.gameObject.SetActive(true);
                                     DOVirtual.DelayedCall(0.3f, () => VFXSpawner.Instance.Despawn(hit));
                                 }
                             }
                         VFXSpawner.Instance.Despawn(orb);
                     });

                yield return new WaitForSeconds(0.15f);
            }
        }

 
        if (sr != null) sr.flipX = (startPos.x < enemyPos.x);

        Vector3 finalDashPos = enemyPos + (enemyPos - startPos).normalized * 1.5f;


        yield return body.DOMove(finalDashPos, 0.15f).SetEase(Ease.OutExpo).WaitForCompletion();


        if (target != null && target.activeInHierarchy)
        {

            target.transform.DOShakePosition(0.3f, 0.3f, 25);

            this.ApplySingleTargetDamage(attacker, target);
        }


        if (!string.IsNullOrEmpty(skillData.hitParticleName))
        {
            Transform hitVFX = VFXSpawner.Instance.Spawn(skillData.hitParticleName, enemyPos, Quaternion.identity);
            if (hitVFX != null)
            {
                hitVFX.gameObject.SetActive(true);
                hitVFX.localScale = Vector3.one * 1.5f; 
                DOVirtual.DelayedCall(0.5f, () => VFXSpawner.Instance.Despawn(hitVFX));
            }
        }

        yield return new WaitForSeconds(0.4f); // Pose dáng

        if (sr != null) sr.flipX = false;

        yield return body.DOMove(startPos, 0.3f).SetEase(Ease.InOutQuad).WaitForCompletion();

    }
}