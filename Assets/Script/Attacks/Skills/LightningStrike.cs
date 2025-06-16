using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningStrike : SkillBehaviour
{
    protected override void ApplySkillEffects(GameObject attacker)
    {
        base.ApplySkillEffects(attacker);
    }

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
        this.ApplySkillEffects(hsm.gameObject);
    }
    protected override float GetAnimationDuration(Animator animator, string triggerName)
    {
        return base.GetAnimationDuration(animator, triggerName);
    }
}
