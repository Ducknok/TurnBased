using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OmniGaleStrike : SkillBehaviour
{
    [Header("Omni Gale Settings")]
    public float dashSpeed = 0.12f;      // Tốc độ lướt
    public float aoeRadius = 5f;        // Bán kính tìm mục tiêu
    public float ghostInterval = 0.02f; // Tốc độ tạo bóng mờ

    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {
        HeroStateMachine hsm = attacker.GetComponent<HeroStateMachine>();
        Transform body = attacker.transform.Find("Body");
        SpriteRenderer sr = body.GetComponent<SpriteRenderer>();
        Animator anim = body.GetComponent<Animator>();
        Vector3 startPos = body.position;

        // Lấy đúng danh sách enemy được chọn trong vùng aoeRadius
        List<GameObject> targetList = GetTargetsInAoE(attacker, target, target.transform.position, aoeRadius);
        if (targetList.Count == 0 && target != null) targetList.Add(target); // Đảm bảo luôn có target chính

        if (anim != null) anim.Play(skillData.attackName);

        foreach (GameObject t in targetList)
        {
            if (t == null || !t.activeInHierarchy) continue;

            Vector3 targetBodyPos = t.transform.Find("Body")?.position ?? t.transform.position;

            Vector3 dirToEnemy = (targetBodyPos - body.position).normalized;
            if (dirToEnemy == Vector3.zero) dirToEnemy = Vector3.right;

            Vector3 frontPos = targetBodyPos - dirToEnemy * 1.2f;
            Vector3 backPos = targetBodyPos + dirToEnemy * 1.2f;


            yield return StartDash(body, frontPos, dashSpeed);

            if (t == null || !t.activeInHierarchy) continue;

            if (sr != null)
            {
                sr.flipX = (body.position.x < targetBodyPos.x);
            }

            ExecuteStrike(attacker, t, targetBodyPos, body.position);
            yield return new WaitForSeconds(0.05f);

            if (t == null || !t.activeInHierarchy) continue;

            yield return StartDash(body, backPos, dashSpeed);

            if (t == null || !t.activeInHierarchy) continue;

            if (sr != null)
            {
                sr.flipX = (body.position.x < targetBodyPos.x);
            }

            ExecuteStrike(attacker, t, targetBodyPos, body.position);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.2f);
        if (sr != null) sr.flipX = false;

        yield return StartDash(body, startPos, 0.3f);
    }

    private IEnumerator StartDash(Transform body, Vector3 destination, float duration)
    {
        Coroutine ghostRoutine = null;

        if (this.gameObject.activeInHierarchy && body != null)
        {
            ghostRoutine = StartCoroutine(SpawnGhostTrails(body));
        }

        if (body != null && body.gameObject.activeInHierarchy)
        {
            body.DOMove(destination, duration).SetEase(Ease.OutCubic);
        }

        yield return new WaitForSeconds(duration);

        if (ghostRoutine != null)
        {
            StopCoroutine(ghostRoutine);
        }
    }

    private void ExecuteStrike(GameObject attacker, GameObject target, Vector3 vfxPos, Vector3 attackerPos)
    {
        if (target == null || !target.activeInHierarchy) return;

        SpawnSlashVFX(vfxPos, attackerPos);

        target.transform.DOComplete();
        target.transform.DOShakePosition(0.1f, 0.2f, 15);
        try
        {
            this.ApplySingleTargetDamage(attacker, target);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[OmniGaleStrike] Bỏ qua lỗi nhận sát thương (Mục tiêu có thể đã chết từ nhát chém trước): " + ex.Message);
        }
    }

    private IEnumerator SpawnGhostTrails(Transform heroBody)
    {
        SpriteRenderer heroSprite = heroBody != null ? heroBody.GetComponent<SpriteRenderer>() : null;
        if (heroSprite == null) yield break;

        while (true)
        {
            if (heroBody == null || !heroBody.gameObject.activeInHierarchy) yield break;

            GameObject ghostObj = new GameObject("OmniGhost");
            ghostObj.transform.position = heroBody.position;
            ghostObj.transform.localScale = heroBody.lossyScale;

            SpriteRenderer ghostSprite = ghostObj.AddComponent<SpriteRenderer>();
            ghostSprite.sprite = heroSprite.sprite;
            ghostSprite.flipX = heroSprite.flipX;
            ghostSprite.sortingOrder = heroSprite.sortingOrder - 1;
            ghostSprite.color = new Color(0.3f, 0.9f, 1f, 0.5f);

            ghostObj.transform.DOScale(heroBody.lossyScale * 0.85f, 0.3f);
            ghostSprite.DOFade(0f, 0.3f).OnComplete(() =>
            {
                if (ghostObj != null) Destroy(ghostObj);
            });

            yield return new WaitForSeconds(ghostInterval);
        }
    }

    private void SpawnSlashVFX(Vector3 targetPos, Vector3 attackerPos)
    {
        if (string.IsNullOrEmpty(skillData.hitParticleName)) return;

        Transform vfx = VFXSpawner.Instance.Spawn(skillData.hitParticleName, targetPos, Quaternion.identity);
        if (vfx != null)
        {
            vfx.gameObject.SetActive(true);
            Vector3 dir = (targetPos - attackerPos).normalized;
            vfx.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            DOVirtual.DelayedCall(0.4f, () => VFXSpawner.Instance.Despawn(vfx));
        }
    }
}