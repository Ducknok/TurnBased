using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackButton : MonoBehaviour
{
    public BaseAttack skillAttackToPerform;

    public void CastSkillAttack()
    {
        GameObject.Find("StartCombat").GetComponent<CombatStateMachine>().SkillAttack(skillAttackToPerform);
    }
}
