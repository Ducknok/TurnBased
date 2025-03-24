using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[Serializable]
public class BaseHero : BaseClass
{
    public int stamina;     //Suc ben
    public int intellect;   //Tri tue
    public int dexterity;   //Kheo leo
    public int agility;     //Lanh le

    public List<BaseAttack> skills = new List<BaseAttack>();
}
