using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : SkillBehaviour
{


    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {
        EnemyStateMachine esm = attacker.GetComponent<EnemyStateMachine>();
        Animator anim = esm.transform.Find("Body").GetComponent<Animator>();
        if (anim != null)
        {
            anim.Play(esm.currentAttack.skillData.attackName);
        }
        float animationDuration = GetAnimationDuration(anim, skillData.attackName);
        yield return new WaitForSeconds(animationDuration);
        this.ApplySingleTargetDamage(attacker,target);
    }
}