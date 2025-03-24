using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : BaseAttack
{
    public FireBall()
    {
        this.attackName = "Fire Ball";
        this.attackDescription = "Shoot a fire ball forward enemy";
        this.attackDamage = 20f;
        this.attackCost = 4f;
    }
}
