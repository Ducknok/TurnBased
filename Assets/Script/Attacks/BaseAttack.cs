using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BaseAttack : MonoBehaviour
{
    public enum Effect
    {
        Sword,
        Lance,
        Light,
        Thunder
    }
    public enum AttackType
    {
        NormalAttack,
        SpecialAttack,
    }
    public string attackName;
    public string attackDescription;
    public float attackDamage;  //Base Damage 15, lvl 10, stamina 35 = basedmg + lvl + stamina = 60
    public float attackCost;    //Mana cost
    public AttackType attackType;
    public Effect effect1;
    public Effect effect2;
}
