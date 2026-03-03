using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : SkillBehaviour
{
    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {
        HeroStateMachine hsm = attacker.GetComponent<HeroStateMachine>();
        Animator anim = hsm.transform.Find("Body").GetComponent<Animator>();
        // T¨ªnh to¨¢n damage d?a tr¨ºn c¨¢c thu?c t¨ªnh c?a hero v¨¤ target
        Vector3 enemyPosition = new Vector3(target.transform.Find("Body").position.x - 1f, target.transform.Find("Body").position.y, target.transform.Find("Body").position.z);
        while (MoveTowardsTarget(attacker, enemyPosition))
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        //Debug.LogError(hero.currentAttack.skillData.attackName);
        anim.Play(hsm.currentAttack.skillData.attackName);
        yield return new WaitForSeconds(0.5f);
        this.ApplySingleTargetDamage(hsm.gameObject, target);
    }

}
