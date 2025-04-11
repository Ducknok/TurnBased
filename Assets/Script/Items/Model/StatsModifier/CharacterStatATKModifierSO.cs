using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterStatATKModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float atkVal, float apVal)
    {
        HeroStateMachine hsm = character.GetComponent<HeroStateMachine>();
        Debug.Log("CharacterStatHealthModifierSO: AffectCharacter: " + hsm);
        if (hsm != null)
        {
            hsm.baseHero.baseATK += atkVal;
            hsm.baseHero.baseAP += apVal;
        }
        else Debug.Log("CharacterStatHealthModifierSO: AffectCharacter: No HeroStateMachine found in parent");
    }
}
