using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowTriangle : SkillBehaviour
{
    protected override void ApplySkillEffects(HeroStateMachine hsm)
    {
        hsm.DoDamage();
        //// T?o hi?u ?ng va ch?m
        //if (impactEffectPrefab != null)
        //{
        //    Instantiate(impactEffectPrefab, target.transform.position, Quaternion.identity);
        //}
    }

    // B?n có th? ghi ?è ph??ng th?c Activate ?? thêm logic ??c bi?t
    public override IEnumerator Activate(HeroStateMachine hsm, GameObject target)
    {
        GameObject body = hsm.transform.Find("Body").gameObject;
        Transform enemy = target.transform.Find("Body");
        Animator anim = hsm.transform.Find("Body").GetComponent<Animator>();

        if (enemy == null)
        {
            Debug.LogError("Không có enemy để tấn công.");
            yield break;
        }

        if (anim != null)
        {
            anim.Play(hsm.currentAttack.skillData.attackName); // animation chuẩn bị
        }

        Vector3 enemyPos = enemy.position;
        float radius = 1.5f;
        Vector3[] positions = new Vector3[3];

        for (int i = 0; i < 3; i++)
        {
            float angle = i * 120f * Mathf.Deg2Rad;
            positions[i] = enemyPos + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
        }

        List<Transform> clones = new List<Transform>();

        for (int i = 0; i < positions.Length; i++)
        {
            // Di chuyển tới vị trí
            yield return body.transform.DOMove(positions[i], 0.25f).SetEase(Ease.InOutSine).WaitForCompletion();

            Transform clone = VFXSpawner.Instance.Spawn(VFXSpawner.ducknokClone, positions[i], Quaternion.identity);
            if (clone != null)
            {
                clone.gameObject.SetActive(true);
                clones.Add(clone);
            }
        }

        // Quay lại vị trí ban đầu
        Vector3 originalPos = body.transform.position; // hoặc lưu từ đầu
        yield return body.transform.DOMove(originalPos, 0.2f).SetEase(Ease.InOutSine).WaitForCompletion();

        yield return new WaitForSeconds(1f);

        foreach (Transform clone in clones)
        {
            if (clone != null)
            {
                clone.DOMove(enemyPos, 0.2f)
                     .SetEase(Ease.InQuad)
                     .OnComplete(() =>
                     {
                         // Thêm VFX, damage, hoặc destroy clone ở đây
                         
                         VFXSpawner.Instance.Despawn(clone.transform);
                        

                     });
            }
        }
        this.ApplySkillEffects(hsm);
    }
}
