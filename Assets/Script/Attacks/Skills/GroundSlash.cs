using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSlash : SkillBehaviour
{
    public override IEnumerator Activate(HeroStateMachine hsm, GameObject target)
    {
        // G?i logic c? b?n t? l?p cha
        Animator anim = hsm.transform.Find("Body").GetComponent<Animator>();
        if (anim != null)
        {
            anim.Play(hsm.currentAttack.skillData.attackName);
        }
        float animationDuration = GetAnimationDuration(anim, skillData.attackName);
        yield return new WaitForSeconds(animationDuration);
        this.ApplySkillEffects(hsm);
    }

    protected override void ApplySkillEffects(HeroStateMachine hsm)
    {
        hsm.DoDamage();
    }
}
