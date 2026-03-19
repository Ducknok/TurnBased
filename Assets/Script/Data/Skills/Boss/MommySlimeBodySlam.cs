using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MommySlimeBodySlam : SkillBehaviour
{
    [Header("Mommy Slime Slam Settings")]
    public float jumpDuration = 0.8f;      // Thời gian bay trên không
    public float jumpHeight = 3.5f;        // Độ cao của cú nhảy
    public float aoeRadius = 15f;          // Tầm sát thương lan (để đủ rộng quét hết 3 heroes)
    public float slamDelay = 0.5f;         // Thời gian nằm đè lên người hero trước khi nhảy về

    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {
        Transform body = attacker.transform.Find("Body");
        Animator anim = body != null ? body.GetComponent<Animator>() : attacker.GetComponent<Animator>();
        Vector3 startPos = body != null ? body.position : attacker.transform.position;

        // ==========================================
        // TÍNH TOÁN VỊ TRÍ RƠI: TRUNG ĐIỂM CỦA TEAM HERO
        // ==========================================
        Vector3 targetCenter = GetHeroTeamMidpoint();

        // Dự phòng: Nếu không tìm thấy Hero nào, lấy vị trí của Target được truyền vào
        if (targetCenter == Vector3.zero)
        {
            targetCenter = target.transform.Find("Body")?.position ?? target.transform.position;
        }

        // Bạn có thể tùy chỉnh targetCenter lùi lại một tí để nó nhảy vào GIỮA các hero thay vì đè lên 1 hero
        targetCenter += new Vector3(-0.5f, 0, 0);

        // ==========================================
        // BƯỚC 1: LẤY ĐÀ & CHƠI ANIMATION
        // ==========================================
        if (anim != null) anim.Play("Charge"); // Cần có animation gồng/lấy đà (ép mình xuống)

        // Hiệu ứng "nhún" bằng DOTween nếu chưa có animation
        if (body != null)
        {
            body.DOScale(new Vector3(1.3f, 0.7f, 1f), 0.3f);
        }
        yield return new WaitForSeconds(0.3f);

        if (anim != null) anim.Play(skillData.attackName); // Đổi sang hình nhảy lên

        // ==========================================
        // BƯỚC 2: BAY TRÊN KHÔNG
        // ==========================================
        if (body != null)
        {
            // Trở lại scale bình thường
            body.DOScale(Vector3.one, 0.2f);

            // DOJump tạo ra đường cong hoàn hảo
            yield return body.DOJump(targetCenter, jumpHeight, 1, jumpDuration)
                             .SetEase(Ease.InQuad)
                             .WaitForCompletion();
        }

        // ==========================================
        // BƯỚC 3: ĐÁP XUỐNG & GÂY SÁT THƯƠNG (Slam!)
        // ==========================================
        // Rung màn hình thật mạnh
        if (CameraController.Instance != null && CameraController.Instance.virtualCamera != null)
        {
            CameraController.Instance.virtualCamera.transform.DOShakePosition(0.4f, 0.8f, 15);
        }

        // Spawn hiệu ứng khói bụi đáp đất (VFX)
        if (!string.IsNullOrEmpty(skillData.hitParticleName))
        {
            Transform impactVfx = VFXSpawner.Instance.Spawn(skillData.hitParticleName, targetCenter, Quaternion.identity);
            if (impactVfx != null)
            {
                impactVfx.gameObject.SetActive(true);
                DOVirtual.DelayedCall(1.0f, () => VFXSpawner.Instance.Despawn(impactVfx));
            }
        }

        // Ép dẹt cục Slime ra tạo cảm giác "đập mạnh bẹp dí"
        if (body != null) body.DOScale(new Vector3(1.5f, 0.5f, 1f), 0.1f);

        // --- GÂY SÁT THƯƠNG LAN (AoE) ---
        // Hàm này sẽ tự động tìm tất cả Hero trong aoeRadius và gọi TakeDamage
        this.ApplyAoEDamage(attacker, target, targetCenter, aoeRadius);

        // ==========================================
        // BƯỚC 4: NẰM ĐÈ VÀ QUAY VỀ
        // ==========================================
        // Nằm đè lên một lúc cho "bá đạo"
        yield return new WaitForSeconds(slamDelay);

        if (body != null)
        {
            body.DOScale(Vector3.one, 0.2f);
            yield return body.DOJump(startPos, jumpHeight * 0.5f, 1, jumpDuration * 0.7f)
                             .SetEase(Ease.Linear)
                             .WaitForCompletion();
        }
    }

    // Hàm hỗ trợ tính toán trung điểm của đội hình
    private Vector3 GetHeroTeamMidpoint()
    {
        GameObject[] heroes = GameObject.FindGameObjectsWithTag("Hero");

        if (heroes.Length == 0) return Vector3.zero;

        Vector3 sumPosition = Vector3.zero;
        foreach (GameObject hero in heroes)
        {
            sumPosition += hero.transform.position;
        }

        return sumPosition / heroes.Length;
    }
}