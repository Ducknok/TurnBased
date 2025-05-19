using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "RPG/Skills/New Skill")]
[Serializable]
public class BaseAttack : ScriptableObject
{
    public enum Effect
    {
        Sword,
        Lance,
        Light,
        Thunder
    }
    public enum AttackType
    {
        NormalAttack,
        SpecialAttack,
    }
    public Sprite attackImage;
    public string attackName;
    public string attackDescription;
    public float attackDamage;  //Base Damage 15, lvl 10, stamina 35 = basedmg + lvl + stamina = 60
    public float attackCost;    //Mana cost
    public AttackType attackType;
    public Effect effect1;
    public Effect effect2;

}

public abstract class SkillBehaviour : MonoBehaviour
{
    public BaseAttack skillData;

    // Kích hoạt kỹ năng
    public virtual IEnumerator Activate(HeroStateMachine hero, GameObject target)
    {
        // Kiểm tra mana và các điều kiện khác
        if (hero.baseHero.curMP < skillData.attackCost)
        {
            yield break;
        }
        // Trừ mana
        hero.baseHero.curMP -= skillData.attackCost;

        // Kích hoạt animation
        Animator animator = hero.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(skillData.attackName);
        }

        //// Phát âm thanh
        //if (skillData.skillSound != null)
        //{
        //    AudioSource.PlayClipAtPoint(skillData.skillSound, hero.transform.position);
        //}

        // Đợi animation hoàn thành
        float animationDuration = GetAnimationDuration(animator, skillData.attackName);
        yield return new WaitForSeconds(animationDuration);

        // Áp dụng các hiệu ứng của kỹ năng (damage, buff, debuff, v.v.)
        ApplySkillEffects(hero, target);
    }

    // Phương thức để áp dụng các hiệu ứng của kỹ năng
    protected abstract void ApplySkillEffects(HeroStateMachine hero, GameObject target);

    // Lấy thời gian của animation
    private float GetAnimationDuration(Animator animator, string triggerName)
    {
        // Thực hiện logic để lấy thời gian của animation
        // Hoặc đơn giản hơn, bạn có thể lưu trữ thời gian trong SkillData
        return 1.0f; // Giá trị mặc định
    }
}
