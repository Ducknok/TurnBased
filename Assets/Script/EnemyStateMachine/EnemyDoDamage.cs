using UnityEngine;

public class EnemyDoDamage : DealDamageController
{
    private static EnemyDoDamage instance;
    public static EnemyDoDamage Instance => instance;
    protected void Awake()
    {
        if (instance != null) return;
        instance = this;
    }
    public override void DoDamage(GameObject attacker)
    {
        EnemyStateMachine esm = attacker.GetComponent<EnemyStateMachine>();
        float calDamage = esm.baseEnemy.curATK + esm.currentAttack.skillData.attackDamage;
        esm.playerToAttack.GetComponent<HeroTakeDamage>().TakeDamage(esm.playerToAttack.gameObject, calDamage);
    }
}
