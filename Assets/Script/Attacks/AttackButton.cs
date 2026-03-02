using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AttackButton : DucMonobehaviour
{
    public SkillBehaviour skillAttackToPerform;

    public void CastSkillAttack()
    {
        //Debug.LogWarning(skillAttackToPerform);
        ButtonController.Instance.SkillAttack(skillAttackToPerform);
    }
}
