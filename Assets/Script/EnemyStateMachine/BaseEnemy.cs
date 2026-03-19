using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "RPG/Base Data/New Enemy Base Data")]
public class BaseEnemy: BaseClass
{
    public enum Type
    {
        Grass,
        Fire,
        Water,
        ELETRIC,
        None,
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
