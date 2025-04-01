using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterStatHealthModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float val)
    {
        HeroStateMachine hsm = character.GetComponent<HeroStateMachine>();
        Debug.Log("CharacterStatHealthModifierSO: AffectCharacter: " + hsm);
        if (hsm != null)
        {
            hsm.baseHero.curHP += val;
        }
        else Debug.Log("CharacterStatHealthModifierSO: AffectCharacter: No HeroStateMachine found in parent");
    }
}

