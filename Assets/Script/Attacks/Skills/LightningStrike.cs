using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningStrike : SkillBehaviour
{
    protected override void ApplySkillEffects(GameObject attacker, GameObject target)
    {
        HeroStateMachine hero = attacker.transform.GetComponent<HeroStateMachine>();
        // Tính toán damage dựa trên các thuộc tính của hero và target
        float damage = skillData.attackDamage * hero.baseHero.baseATK;

        // Áp dụng damage vào target
        EnemyStateMachine enemy = target.GetComponent<EnemyStateMachine>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, skillData.effect1, skillData.effect2);
        }

        //// Tạo hiệu ứng va chạm
        //if (impactEffectPrefab != null)
        //{
        //    Instantiate(impactEffectPrefab, target.transform.position, Quaternion.identity);
        //}
    }

    // Bạn có thể ghi đè phương thức Activate để thêm logic đặc biệt
    public override IEnumerator Activate(GameObject attacker, GameObject target)
    {
        HeroStateMachine hero = attacker.transform.GetComponent<HeroStateMachine>();
        // Gọi logic cơ bản từ lớp cha
        Animator anim = attacker.transform.Find("Body").GetComponent<Animator>();
        if(anim != null)
        {
            anim.Play(hero.currentAttack.skillData.attackName);
        }
        float animationDuration = GetAnimationDuration(anim, skillData.attackName);
        yield return new WaitForSeconds(animationDuration);
        this.ApplySkillEffects(hero.gameObject, target);
    }
    protected override float GetAnimationDuration(Animator animator, string triggerName)
    {
        return base.GetAnimationDuration(animator, triggerName);
    }
}
