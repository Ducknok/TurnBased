using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BaseEnemy: BaseClass
{
    public enum Type
    {
        Grass,
        Fire,
        Water,
        ELETRIC,
    }

    public enum Rarity
    {
        COMMON,
        UNCOMMON,
        RARE,
        SUPPERRARE,
    }

    public Type enemyType;
    public Rarity rarity;
}
