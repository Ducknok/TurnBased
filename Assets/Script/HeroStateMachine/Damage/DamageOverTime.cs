using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOverTime : DealDamageController
{
    private static DamageOverTime instance;
    public static DamageOverTime Instance => instance;
    public float interval = 1f; // mỗi 3 giây
    public int totalTicks = 3;
    protected void Awake()
    {
        if (instance != null) return;
        instance = this;
    }
    public override void DoDamage(GameObject attacker)
    {
        HeroStateMachine hsm = attacker.GetComponent<HeroStateMachine>();
        StartCoroutine(DoTCoroutine(hsm));
    }
    private IEnumerator DoTCoroutine(HeroStateMachine hsm)
    {
        if (CombatController.Instance.CBM.performList.Count == 0)
        {
            Debug.LogWarning("performList đang rỗng!");
            yield break;
        }

        // Cache thông tin một lần
        var attack = CombatController.Instance.CBM.performList[0].choosenAttack;
        float calDamage = hsm.baseHero.curATK + attack.attackDamage;

        int tickCount = 0;
        while (tickCount < totalTicks)
        {
            if (hsm.enemyToAttack != null)
            {
                EnemyStateMachine enemy = hsm.enemyToAttack.GetComponent<EnemyStateMachine>();
                if (enemy != null)
                {
                    enemy.GetComponent<EnemyTakeDamage>().TakeDamage(enemy.gameObject, calDamage, attack.effect1, attack.effect2);
                    hsm.UpdateHeroPanel();
                    Debug.Log($"[DOT] Tick {tickCount + 1}: Gây {calDamage} sát thương");
                }
                else
                {
                    Debug.LogWarning("Không tìm thấy EnemyStateMachine.");
                    yield break;
                }
            }
            else
            {
                Debug.LogWarning("enemyToAttack null.");
                yield break;
            }

            tickCount++;
            yield return new WaitForSeconds(interval);
        }
    }

}
