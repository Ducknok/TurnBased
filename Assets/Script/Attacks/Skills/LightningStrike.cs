using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningStrike : SkillBehaviour
{
    protected override void ApplySkillEffects(HeroStateMachine hsm)
    {
        hsm.DoDamage();
        //// Tạo hiệu ứng va chạm
        //if (impactEffectPrefab != null)
        //{
        //    Instantiate(impactEffectPrefab, target.transform.position, Quaternion.identity);
        //}
    }

    // Bạn có thể ghi đè phương thức Activate để thêm logic đặc biệt
    public override IEnumerator Activate(HeroStateMachine hsm, GameObject target)
    {
        // Gọi logic cơ bản từ lớp cha
        Animator anim = hsm.transform.Find("Body").GetComponent<Animator>();
        if(anim != null)
        {
            anim.Play(hsm.currentAttack.skillData.attackName);
        }
        float animationDuration = GetAnimationDuration(anim, skillData.attackName);
        yield return new WaitForSeconds(animationDuration);
        this.ApplySkillEffects(hsm);
    }
    protected override float GetAnimationDuration(Animator animator, string triggerName)
    {
        return base.GetAnimationDuration(animator, triggerName);
    }
}
