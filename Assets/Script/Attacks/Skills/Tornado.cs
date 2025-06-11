using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tornado : SkillBehaviour
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
        // Gọi logic cơ bản từ lớp cha
        Animator anim = hsm.transform.Find("Body").GetComponent<Animator>();
        if (anim != null)
        {
            anim.Play(hsm.currentAttack.skillData.attackName);
        }
        float animationDuration = GetAnimationDuration(anim, skillData.attackName);
        yield return new WaitForSeconds(animationDuration);
        this.ApplySkillEffects(hsm);
    }
}