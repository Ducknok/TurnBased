using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "New Hero data", menuName = "RPG/Base Data/New Hero Base Data")]
public class BaseHero : BaseClass
{
    public Sprite heroImage;
    public int stamina;     
    public int intellect;   
    public int dexterity;   
    public int agility;     

    
    public HeroType heroType;
    public Elemental elemental;
}
