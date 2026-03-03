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

    private bool isDashing = false;

    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {
        HeroStateMachine hsm = attacker.GetComponent<HeroStateMachine>();
        Transform body = attacker.transform.Find("Body");
        SpriteRenderer sr = body.GetComponent<SpriteRenderer>();
        Animator anim = body.GetComponent<Animator>();
        Vector3 startPos = body.position;

        // Tìm danh sách quái trong vùng đánh lan
        List<GameObject> targetList = GetTargetsInAoE(attacker, target, target.transform.position, aoeRadius);

        if (anim != null) anim.Play(skillData.attackName);
        foreach (GameObject t in targetList)
        {
            if (t == null || !t.activeInHierarchy) continue;

            Vector3 targetBodyPos = t.transform.Find("Body")?.position ?? t.transform.position;

            // Tính toán hướng từ Hero đến Quái
            Vector3 dirToEnemy = (targetBodyPos - body.position).normalized;
            if (dirToEnemy == Vector3.zero) dirToEnemy = Vector3.right;

            // Vị trí 1: Trước mặt quái
            Vector3 frontPos = targetBodyPos - dirToEnemy * 1.2f;
            // Vị trí 2: Sau lưng quái
            Vector3 backPos = targetBodyPos + dirToEnemy * 1.2f;

            // --- NHÁT 1: ĐÂM TRƯỚC MẶT ---
            yield return StartDash(body, frontPos);

            // Cập nhật Flip ngay nhát đầu để chắc chắn Hero đối mặt với quái
            // Vì Animation mặc định quay trái, nên nếu Hero nằm bên TRÁI quái (x < target.x), ta cần Flip (true) để quay PHẢI.
            if (sr != null)
            {
                sr.flipX = (body.position.x < targetBodyPos.x);
            }

            ExecuteStrike(attacker, t, targetBodyPos, body.position);
            yield return new WaitForSeconds(0.05f);

            // --- NHÁT 2: LƯỚT XUYÊN ĐÂM SAU LƯNG ---
            yield return StartDash(body, backPos);

            // Sau khi lướt ra sau lưng, vị trí X đã thay đổi. 
            // Ta tính toán lại Flip để Hero quay đầu lại nhìn quái.
            if (sr != null)
            {
                sr.flipX = (body.position.x < targetBodyPos.x);
            }

            ExecuteStrike(attacker, t, targetBodyPos, body.position);
            yield return new WaitForSeconds(0.1f);
        }


        yield return new WaitForSeconds(0.2f);
        if (sr != null) sr.flipX = false; 

        isDashing = true;
        StartCoroutine(SpawnGhostTrails(body));
        yield return body.DOMove(startPos, 0.3f).SetEase(Ease.OutQuad).WaitForCompletion();
        isDashing = false;
    }

    // Hàm bổ trợ thực hiện việc lướt
    private IEnumerator StartDash(Transform body, Vector3 destination)
    {
        isDashing = true;
        StartCoroutine(SpawnGhostTrails(body));

        yield return body.DOMove(destination, dashSpeed).SetEase(Ease.OutCubic).WaitForCompletion();

        isDashing = false;
    }

    private void ExecuteStrike(GameObject attacker, GameObject target, Vector3 vfxPos, Vector3 attackerPos)
    {
        SpawnSlashVFX(vfxPos, attackerPos);
        target.transform.DOShakePosition(0.1f, 0.2f, 15);
        this.ApplySingleTargetDamage(attacker, target);
    }

    private IEnumerator SpawnGhostTrails(Transform heroBody)
    {
        SpriteRenderer heroSprite = heroBody.GetComponent<SpriteRenderer>();
        if (heroSprite == null) yield break;

        while (isDashing)
        {
            GameObject ghostObj = new GameObject("OmniGhost");
            ghostObj.transform.position = heroBody.position;
            ghostObj.transform.localScale = heroBody.lossyScale;

            SpriteRenderer ghostSprite = ghostObj.AddComponent<SpriteRenderer>();
            ghostSprite.sprite = heroSprite.sprite;
            ghostSprite.flipX = heroSprite.flipX; // Chép hướng hiện tại của Hero
            ghostSprite.sortingOrder = heroSprite.sortingOrder - 1;
            ghostSprite.color = new Color(0.3f, 0.9f, 1f, 0.5f);

            // CẬP NHẬT: Dùng OnComplete để hủy thay vì Destroy(ghostObj, 0.3f) nhằm tránh lỗi DOTween
            ghostObj.transform.DOScale(heroBody.lossyScale * 0.85f, 0.3f);
            ghostSprite.DOFade(0f, 0.3f).OnComplete(() =>
            {
                if (ghostObj != null)
                {
                    Destroy(ghostObj);
                }
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