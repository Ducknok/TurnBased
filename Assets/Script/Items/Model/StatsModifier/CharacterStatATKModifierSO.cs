using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterStatATKModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, int atkVal, int apVal)
    {
        HeroStateMachine hsm = character.GetComponent<HeroStateMachine>();
        //Debug.Log("CharacterStatHealthModifierSO: AffectCharacter: " + hsm);
        if (hsm != null)
        {
            hsm.baseHero.baseATK += atkVal;
            hsm.baseHero.baseMATK += apVal;
            hsm.baseHero.curATK = hsm.baseHero.baseATK;
            hsm.baseHero.curMATK = hsm.baseHero.baseMATK;
        }
        else Debug.Log("CharacterStatHealthModifierSO: AffectCharacter: No HeroStateMachine found in parent");
    }
}
