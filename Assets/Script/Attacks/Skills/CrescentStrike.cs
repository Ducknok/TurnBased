using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrescentStrike : BaseAttack
{
    public CrescentStrike()
    {
        this.attackName = "Crescent Strike";
        this.attackDescription = "Unleashes a bolt of lunar energy in an arc";
        this.attackDamage = 20f;
        this.attackCost = 3f;
    }

    //public override IEnumerator Activate(HeroStateMachine hero, GameObject target)
    //{
    //    throw new System.NotImplementedException();
    //}
}
