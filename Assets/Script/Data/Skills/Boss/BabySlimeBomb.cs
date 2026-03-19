using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BabySlimeBomb : EnemyStateMachine
{
    private BossStateMachine motherBoss;
    private bool isResolved = false;

    public void Initialize(BossStateMachine boss)
    {
        motherBoss = boss;
    }


    protected override void Start()
    {
        this.baseEnemy = Instantiate(this.baseEnemy);
        base.Start();


        if (this.combatStateMachine != null && !this.combatStateMachine.enemiesInCombat.Contains(this.gameObject))
        {
            this.combatStateMachine.enemiesInCombat.Add(this.gameObject);


            this.StartCombatFlow();
        }
    }

    protected override void Update()
    {
        base.Update();

        if (isResolved) return;

        if (this.isLockBrokenOnce || this.baseEnemy.curHP <= 0)
        {
            isResolved = true;
            this.baseEnemy.curHP = 0;

            if (motherBoss != null) motherBoss.OnBombDestroyed();

            this.currentState = TurnState.DEAD;
        }
    }


    protected override IEnumerator TimeForAction()
    {
        if (this.actionStarted || this.isResolved) yield break;
        this.actionStarted = true;

        yield return StartCoroutine(this.currentAttack.Activate(this.gameObject, this.playerToAttack));

        this.combatStateMachine.enemiesAttacked.Add(this.gameObject);

        this.baseEnemy.curHP = 0;

        this.currentState = TurnState.DEAD;
    }
}