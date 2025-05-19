using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Punch : SkillBehaviour
{
    public override IEnumerator Activate(HeroStateMachine hero, GameObject target)
    {
        return base.Activate(hero, target);
    }

    protected override void ApplySkillEffects(HeroStateMachine hero, GameObject target)
    {
        throw new System.NotImplementedException();
    }
}
