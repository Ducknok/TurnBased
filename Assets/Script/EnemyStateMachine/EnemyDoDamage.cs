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
        Vector3 targetPosition = esm.playerToAttack.GetComponent<HeroStateMachine>().transform.Find("Body").position;
        float calDamage = esm.baseEnemy.curATK + esm.currentAttack.skillData.attackDamage;
        Transform hitParticle = VFXSpawner.Instance.Spawn(esm.currentAttack.skillData.hitParticleName, targetPosition, Quaternion.identity);
        hitParticle.gameObject.SetActive(true);
        esm.playerToAttack.GetComponent<HeroTakeDamage>().TakeDamage(esm.playerToAttack.gameObject, calDamage);
    }
}
