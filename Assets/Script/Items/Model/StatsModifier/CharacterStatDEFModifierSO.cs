using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterStatDEFModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float defVal, float mDefVal)
    {
        HeroStateMachine hsm = character.GetComponent<HeroStateMachine>();
        Debug.Log("CharacterStatDefModifierSO: AffectCharacter: " + hsm);
        if (hsm != null)
        {
            hsm.baseHero.baseDEF += defVal;
            hsm.baseHero.baseMDEF += mDefVal;
        }
        else Debug.Log("CharacterStatHealthModifierSO: AffectCharacter: No HeroStateMachine found in parent");
    }
}
