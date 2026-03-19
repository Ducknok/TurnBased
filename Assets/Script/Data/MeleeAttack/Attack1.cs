using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack1 : SkillBehaviour
{
    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {
        EnemyStateMachine esm = attacker.GetComponent<EnemyStateMachine>();
        Animator anim = esm.transform.Find("Body").GetComponent<Animator>();
        string animName = esm.currentAttack.skillData.attackName;
        //Debug.Log($"[DEBUG] Trying to play anim: {animName}");
        Vector3 heroPosition = new Vector3(target.transform.Find("Body").position.x + 1f, target.transform.Find("Body").position.y, target.transform.Find("Body").position.z);
        while (MoveTowardsTarget(attacker, heroPosition))
        {
            yield return null;
        }

        if (anim.HasState(0, Animator.StringToHash(animName)))
        {
            anim.Play(animName);
        }
        else
        {
            yield return null;
        }
        float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);
        this.ApplySingleTargetDamage(esm.gameObject, target);
    }
}
