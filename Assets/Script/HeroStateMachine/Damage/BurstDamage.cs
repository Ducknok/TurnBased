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

        if (hsm.enemyToAttack == null)
        {
            Debug.LogWarning("M?c ti¨ºu ?? b? ti¨ºu di?t ho?c kh?ng t?n t?i. B? qua s¨¢t th??ng.");
            return;
        }

        EnemyStateMachine esm = hsm.enemyToAttack.GetComponent<EnemyStateMachine>();

        if (esm == null) return;

        Vector3 targetPosition = esm.transform.Find("Body").position;

        if (CombatController.Instance.CBM.performList.Count > 0)
        {
            float calDamage = hsm.baseHero.curATK + CombatController.Instance.CBM.performList[0].choosenAttack.attackDamage;

            ManaController.Instance.ManaBar(hsm);
            Transform hitParticle = VFXSpawner.Instance.Spawn(hsm.currentAttack.skillData.hitParticleName, targetPosition, Quaternion.identity);

            if (hitParticle != null)
            {
                hitParticle.gameObject.SetActive(true);
            }

            hsm.enemyToAttack.GetComponent<EnemyTakeDamage>().TakeDamage(
                hsm.enemyToAttack.gameObject,
                calDamage,
                CombatController.Instance.CBM.performList[0].choosenAttack.effect1,
                CombatController.Instance.CBM.performList[0].choosenAttack.effect2
            );

            hsm.heroPanelHandler.UpdateHeroPanel();
        }
        else
        {
            Debug.LogWarning("performList tr?ng, kh?ng th? l?y th?ng tin chi¨ºu th?c.");
            return;
        }
    }
}