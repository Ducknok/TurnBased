using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tornado : SkillBehaviour
{
    [Header("Tornado Settings")]
    public float aoeRadius = 5f;

    // B?n có th? ghi ?è ph??ng th?c Activate ?? thêm logic ??c bi?t
    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {

        HeroStateMachine hsm = attacker.GetComponent<HeroStateMachine>();
        Animator anim = hsm.transform.Find("Body").GetComponent<Animator>();
        if (anim != null)
        {
            anim.Play(hsm.currentAttack.skillData.attackName);
        }
        float animationDuration = GetAnimationDuration(anim, skillData.attackName);
        yield return new WaitForSeconds(animationDuration);
        Vector3 impactPoint = target.transform.position;
        this.ApplyAoEDamage(attacker, target, impactPoint, aoeRadius);
    }
}