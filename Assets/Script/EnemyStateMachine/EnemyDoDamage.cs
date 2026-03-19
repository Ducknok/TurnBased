using UnityEngine;

public class EnemyDoDamage : DealDamageController
{
    private static EnemyDoDamage instance;
    public static EnemyDoDamage Instance => instance;

    protected override void Awake()
    {
        if (instance != null && this.gameObject != null) Destroy(this.gameObject);
        instance = this;
        DontDestroyOnLoad(this);
    }

    public override void DoDamage(GameObject attacker)
    {
        if (attacker == null) return;

        EnemyStateMachine esm = attacker.GetComponent<EnemyStateMachine>();

        if (esm == null || esm.playerToAttack == null)
        {
            return;
        }

        HeroStateMachine hsm = esm.playerToAttack.GetComponent<HeroStateMachine>();
        if (hsm == null) return;

        Vector3 targetPosition = hsm.transform.Find("Body").position;
        if (esm.currentAttack != null && esm.currentAttack.skillData != null)
        {
            int calDamage = esm.baseEnemy.curATK + esm.currentAttack.skillData.attackDamage;

            Transform hitParticle = VFXSpawner.Instance.Spawn(esm.currentAttack.skillData.hitParticleName, targetPosition, Quaternion.identity);
            if (hitParticle != null)
            {
                hitParticle.gameObject.SetActive(true);
            }

            esm.playerToAttack.GetComponent<HeroTakeDamage>().TakeDamage(esm.playerToAttack.gameObject, calDamage);
        }
        else
        {
            Debug.LogWarning("Kh?ng t¨¬m th?y d? li?u ?¨°n t?n c?ng c?a qu¨¢i v?t!");
        }
    }
}