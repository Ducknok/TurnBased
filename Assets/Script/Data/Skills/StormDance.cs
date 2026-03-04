using System.Collections;
using UnityEngine;
using DG.Tweening;

public class StormDance : SkillBehaviour
{
    [Header("Storm Dance Settings")]
    public int strikeCount = 4;        
    public float teleportRadius = 1.5f;
    public float delayBetweenStrikes = 0.15f; 

    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {
        HeroStateMachine hsm = attacker.GetComponent<HeroStateMachine>();
        Transform body = attacker.transform.Find("Body");
        Animator anim = body.GetComponent<Animator>();
        SpriteRenderer sr = body.GetComponent<SpriteRenderer>();

        Vector3 startPos = body.position;
        Transform targetBody = target.transform.Find("Body");
        Vector3 targetPos = targetBody != null ? targetBody.position : target.transform.position;
        targetPos.y += 0.5f;

        Vector3[] teleportOffsets = new Vector3[]
        {
            new Vector3(-1, 1, 0).normalized * teleportRadius,
            new Vector3(1, -0.5f, 0).normalized * teleportRadius,
            new Vector3(-1, -0.5f, 0).normalized * teleportRadius,
            new Vector3(1, 1, 0).normalized * teleportRadius
        };

        if (anim != null) anim.Play(skillData.attackName);

        for (int i = 0; i < strikeCount; i++)
        {

            if (target == null || !target.activeInHierarchy) break;


            Vector3 jumpPos = targetPos + teleportOffsets[i % teleportOffsets.Length];
            body.position = jumpPos;

            if (sr != null)
            {
                sr.flipX = (body.position.x > targetPos.x);
            }

            SpawnDirectionalSlash(targetPos, body.position);
            target.transform.DOShakePosition(0.1f, 0.1f, 10);

            this.ApplySingleTargetDamage(attacker, target);

            yield return new WaitForSeconds(delayBetweenStrikes);
        }

        if (target != null && target.activeInHierarchy)
        {
            body.position = targetPos + new Vector3(0, 2.5f, 0);
            if (sr != null) sr.flipX = false;

            yield return new WaitForSeconds(0.15f);

            Vector3 landPos = targetPos + new Vector3(2f, -1.5f, 0);
            yield return body.DOMove(landPos, 0.15f).SetEase(Ease.InExpo).WaitForCompletion();

            SpawnDirectionalSlash(targetPos, targetPos + Vector3.up, true);

            this.ApplySingleTargetDamage(attacker, target);

            yield return new WaitForSeconds(0.5f);
        }


        if (sr != null) sr.flipX = false;


        yield return body.DOMove(startPos, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();
    }

    private void SpawnDirectionalSlash(Vector3 targetPos, Vector3 attackerPos, bool isFinalHit = false)
    {
        if (string.IsNullOrEmpty(skillData.hitParticleName)) return;

        Transform slashVFX = VFXSpawner.Instance.Spawn(skillData.hitParticleName, targetPos, Quaternion.identity);
        if (slashVFX != null)
        {
            slashVFX.gameObject.SetActive(true);

            // Tính góc xoay từ Hero chĩa thẳng vào tâm Quái
            Vector3 direction = (targetPos - attackerPos).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            slashVFX.rotation = Quaternion.Euler(0, 0, angle);

            // Phóng to nếu là hit cuối
            slashVFX.localScale = isFinalHit ? Vector3.one * 2.0f : Vector3.one;

            // Xóa sau 0.4s
            DOVirtual.DelayedCall(0.4f, () => VFXSpawner.Instance.Despawn(slashVFX));
        }
    }
}