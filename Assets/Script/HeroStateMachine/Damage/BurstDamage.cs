using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstDamage : DealDamageController
{
    private static BurstDamage instance;
    public static BurstDamage Instance => instance;
    protected override void Awake()
    {
        if (instance != null && this.gameObject != null) Destroy(this.gameObject);
        instance = this;
        DontDestroyOnLoad(this);
    }
    public override void DoDamage(GameObject attacker)
    {
        HeroStateMachine hsm = attacker.GetComponent<HeroStateMachine>();
        EnemyStateMachine esm = hsm.enemyToAttack.GetComponent<EnemyStateMachine>();
        Vector3 targetPosition = esm.transform.Find("Body").position;
        float calDamage = hsm.baseHero.curATK + CombatController.Instance.CBM.performList[0].choosenAttack.attackDamage;
        if (CombatController.Instance.CBM.performList.Count > 0)
        {
            ManaController.Instance.ManaBar(hsm);      
            Transform hitParticle = VFXSpawner.Instance.Spawn(hsm.currentAttack.skillData.hitParticleName, targetPosition, Quaternion.identity);
            hitParticle.gameObject.SetActive(true);
            hsm.enemyToAttack.GetComponent<EnemyTakeDamage>().TakeDamage(hsm.enemyToAttack.gameObject,calDamage, CombatController.Instance.CBM.performList[0].choosenAttack.effect1, CombatController.Instance.CBM.performList[0].choosenAttack.effect2);
            hsm.heroPanelHandler.UpdateHeroPanel();
        }
        else
        {
            return;
        }
    }
}
