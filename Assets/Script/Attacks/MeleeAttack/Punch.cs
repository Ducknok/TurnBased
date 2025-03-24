using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Punch : BaseAttack
{
    public Punch()
    {
        this.attackName = "Punch";
        this.attackDescription = "This is a Punch Attack";
        this.attackDamage = 10f;
        this.attackCost = 0f;
    }
}
