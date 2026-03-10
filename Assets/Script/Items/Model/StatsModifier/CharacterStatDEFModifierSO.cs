using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterStatDEFModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, int defVal, int mDefVal)
    {
        HeroStateMachine hsm = character.GetComponent<HeroStateMachine>();
        //Debug.Log("CharacterStatDefModifierSO: AffectCharacter: " + hsm);
        if (hsm != null)
        {
            hsm.baseHero.baseDEF += defVal;
            hsm.baseHero.baseMDEF += mDefVal;

            hsm.baseHero.curDEF = hsm.baseHero.baseDEF;
            hsm.baseHero.curMDEF = hsm.baseHero.baseMDEF;
        }
        else Debug.Log("CharacterStatDEFModifierSO: AffectCharacter: No HeroStateMachine found in parent");
    }
}