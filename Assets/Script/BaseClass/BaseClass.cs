using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseClass
{
    public string theName;
    public float baseHP;
    public float curHP;

    public float baseMP;
    public float curMP;

    public float baseATK;
    public float curATK;

    public float baseMATK;
    public float curMATK;

    public float baseDEF;
    public float curDEF;

    public float baseMDEF;
    public float curMDEF;

    public List<BaseAttack> attacks = new List<BaseAttack>();
}
