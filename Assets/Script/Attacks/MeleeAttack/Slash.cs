using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash : BaseAttack
{
    public Slash()
    {
        this.attackName = "Slash";
        this.attackDescription = "Fast Slash Attack with Your Weapon";
        this.attackDamage = 10f;
        this.attackCost = 0;
    }
}
