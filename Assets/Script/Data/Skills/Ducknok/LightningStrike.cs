using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningStrike : SkillBehaviour
{

    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {
        HeroStateMachine hsm = attacker.GetComponent<HeroStateMachine>();
        Animator anim = hsm.transform.Find("Body").GetComponent<Animator>();
        if(anim != null)
        {
            anim.Play(hsm.currentAttack.skillData.attackName);
        }
        float animationDuration = GetAnimationDuration(anim, skillData.attackName);
        yield return new WaitForSeconds(animationDuration);
        this.ApplySingleTargetDamage(hsm.gameObject, target);
    }
}
