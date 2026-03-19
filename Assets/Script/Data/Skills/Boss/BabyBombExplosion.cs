using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BabyBombExplosion : SkillBehaviour
{
    [Header("Explosion Settings")]
    public int explosionDamage = 150;
    public float aoeRadius = 5f; // Bán kính nổ lan

    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {
        BabySlimeBomb bombLogic = attacker.GetComponent<BabySlimeBomb>();

        Transform body = attacker.transform.Find("Body");
        if (body == null) body = attacker.transform;

        SpriteRenderer sr = body.GetComponent<SpriteRenderer>();

        if (target == null)
        {
            var sm = attacker.GetComponent<EnemyStateMachine>();
            target = sm.playerToAttack != null ? sm.playerToAttack : sm.savedAttack.AttackerTarget;
        }

        Vector3 targetPos = target != null ? (target.transform.Find("Body")?.position ?? target.transform.position) : attacker.transform.position;
        targetPos += new Vector3(0.5f, 0, 0);


        body.DOScale(new Vector3(0.7f, 1.5f, 1f), 0.15f);
        yield return new WaitForSeconds(0.15f);

        body.DOScale(new Vector3(2.5f, 0.8f, 1f), 0.1f);
        yield return attacker.transform.DOMove(targetPos, 0.2f).SetEase(Ease.OutQuad).WaitForCompletion();

        if (sr != null) sr.DOColor(Color.red, 0.1f).SetLoops(10, LoopType.Yoyo);

        body.DOScale(new Vector3(1.5f, 1.5f, 1f), 0.8f).SetEase(Ease.OutQuad);
        body.DOShakePosition(1f, 0.3f, 30);

        yield return new WaitForSeconds(1f);

        body.DOScale(new Vector3(4f, 4f, 1f), 0.05f);
        yield return new WaitForSeconds(0.05f);

        if (sr != null) sr.enabled = false;

        if (CameraController.Instance != null && CameraController.Instance.virtualCamera != null)
        {
            CameraController.Instance.virtualCamera.transform.DOShakePosition(0.4f, 1.5f, 20);
        }

        if (!string.IsNullOrEmpty(skillData.hitParticleName))
        {
            Transform vfx = VFXSpawner.Instance.Spawn(skillData.hitParticleName, targetPos, Quaternion.identity);
            if (vfx != null)
            {
                vfx.gameObject.SetActive(true);
                DOVirtual.DelayedCall(1.5f, () => VFXSpawner.Instance.Despawn(vfx));
            }
        }


        if (target != null)
        {
            this.ApplySingleTargetDamage(attacker, target);
        }


        yield return new WaitForSeconds(0.5f);
    }
}