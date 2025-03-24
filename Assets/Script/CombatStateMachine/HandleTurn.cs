using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HandleTurn
{
    public string Attacker;                 // name of attacker
    public string Type;
    public GameObject AttacksGameObject;    // Whos attacks
    public GameObject AttackerTarget;       //Who is going to be attacked

    //Which attack is performed
    public BaseAttack choosenAttack;

}
