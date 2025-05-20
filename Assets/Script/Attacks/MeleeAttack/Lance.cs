using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lance : SkillBehaviour
{
    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {
        HeroStateMachine hero = attacker.transform.GetComponent<HeroStateMachine>();
        Animator anim = hero.transform.Find("Body").GetComponent<Animator>();
        // T¨ªnh to¨¢n damage d?a tr¨ºn c¨¢c thu?c t¨ªnh c?a hero v¨¤ target
        Vector3 enemyPosition = new Vector3(target.transform.Find("Body").position.x - 1f, target.transform.Find("Body").position.y, target.transform.Find("Body").position.z);
        while (MoveTowardsEnemy(hero, enemyPosition))
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        anim.Play(hero.currentAttack.skillData.attackName);
    }

    protected override void ApplySkillEffects(GameObject hero, GameObject target)
    {
        throw new System.NotImplementedException();
    }

    protected override float GetAnimationDuration(Animator animator, string triggerName)
    {
        return base.GetAnimationDuration(animator, triggerName);
    }

    protected override bool MoveTowardsEnemy(HeroStateMachine hero, Vector3 target)
    {
        return base.MoveTowardsEnemy(hero, target);
    }
}
