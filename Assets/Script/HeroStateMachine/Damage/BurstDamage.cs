using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstDamage : DealDamageController
{
    private static BurstDamage instance;
    public static BurstDamage Instance => instance;
    protected void Awake()
    {
        if (instance != null) return;
        instance = this;
    }
    public override void DoDamage(GameObject attacker)
    {
        HeroStateMachine hsm = attacker.GetComponent<HeroStateMachine>();
        float calDamage = hsm.baseHero.curATK + CombatController.Instance.CBM.performList[0].choosenAttack.attackDamage;
        if (CombatController.Instance.CBM.performList.Count > 0)
        {
            ManaController.Instance.ManaBar(hsm);
            hsm.enemyToAttack.GetComponent<EnemyTakeDamage>().TakeDamage(hsm.enemyToAttack.gameObject,calDamage, CombatController.Instance.CBM.performList[0].choosenAttack.effect1, CombatController.Instance.CBM.performList[0].choosenAttack.effect2);
            hsm.UpdateHeroPanel();
        }
        else
        {
            return;
        }
    }
}
