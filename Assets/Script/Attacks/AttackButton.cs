using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackButton : MonoBehaviour
{
    public SkillBehaviour skillAttackToPerform;

    public void CastSkillAttack()
    {
        //Debug.LogWarning(skillAttackToPerform);
        GameObject.Find("StartCombat").GetComponent<ButtonController>().SkillAttack(skillAttackToPerform);
    }
}
