using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: tạo 3 class charstatATKmodifierSO, charstatDEFmodifierSO, charstatMAGICmodifierSO
[CreateAssetMenu]
public class CharacterStatHealthModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float hpVal, float mpVal)
    {
        HeroStateMachine hsm = character.GetComponent<HeroStateMachine>();
        Debug.Log("CharacterStatHealthModifierSO: AffectCharacter: " + hsm);
        if (hsm != null)
        {
            hsm.baseHero.curHP += hpVal;
            hsm.baseHero.curMP += mpVal;
        }
        else Debug.Log("CharacterStatHealthModifierSO: AffectCharacter: No HeroStateMachine found in parent");
    }
}

